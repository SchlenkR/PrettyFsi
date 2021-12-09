module PrettyFsi

open System
open System.Collections
open System.Collections.Generic
open FSharp.Compiler.Interactive
open Microsoft.FSharp.Reflection

// TODO: Document this
// this doesn't work, since FSharp.Compiler.Interactive.Shell.Settings.fsi and the
// fsi object in the interactive session differ.
//let private fsi = FSharp.Compiler.Interactive.Shell.Settings.fsi
        
type private Alignment = | Left | Right

type FsiConfig =
    { formatProvider: IFormatProvider
      fsiPrintSize: int
      fsiPrintWidth: int }

module Config =
    type PrintWidth =
        | TryUseConsoleThenFsiPrintWidth
        | UseWidth of int
        | UseFsiPrintWidth

    type MaxRows =
        | UseFsiPrintSize
        | UseRows of int
    
    // TODO: string trim/wrap
    // TODO: alignment per data type
    
    let mutable maxCellWidth = 50
    let mutable maxRows = UseFsiPrintSize
    let mutable maxTableWidth = TryUseConsoleThenFsiPrintWidth
    
    let mutable dateTimeFormat = "G"
    let mutable timeSpanFormat = "c"

    let mutable rowCellSeparator = ":   "
    let mutable valueCellSeparator = " | "

let isSupportedElementType (t: Type) =
    let isUnsupportedKnownBaseTypes =
        t.Namespace = "System" && t.IsValueType
        || t = typeof<string>
    let isUnsupportedFSharpType =
        FSharpType.IsFunction t 
        || FSharpType.IsModule t 
        || FSharpType.IsTuple t
        || FSharpType.IsUnion t
    not (isUnsupportedKnownBaseTypes || isUnsupportedFSharpType)

let isNumeric =
    let numberTypes = 
        HashSet [
            typeof<byte>; typeof<sbyte>
            typeof<int16>; typeof<uint16>
            typeof<int32>; typeof<uint32>
            typeof<uint64>; typeof<int64>
            typeof<decimal>; typeof<float>; typeof<single>
            typeof<bigint>
        ]
    fun (t: Type) -> numberTypes.Contains(t)

let private (|TrySupportedEnumerable|_|) (input: obj) =
    match input with
    | :? IEnumerable as enumerable ->
        let enumerableType = 
            input.GetType().GetInterfaces()
            |> Array.tryFind (fun x -> 
                x.IsGenericType
                && x.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>)
        match enumerableType with
        | Some enumerableType ->
            let elemType = enumerableType.GetGenericArguments().[0]
            match isSupportedElementType elemType with
            | true -> Some (enumerable, elemType)
            | false -> None
        | None -> None
    | _ -> None

let private (|SupportedEnumerable|) (input: IEnumerable<'a>) =
    ((|TrySupportedEnumerable|_|) input).Value

let private toTableString
    (fsiConfig: FsiConfig)
    (properties: string list)
    (input: IEnumerable<obj seq>)
    =
    let tostring (x: 'a) = sprintf "%A" x
    
    let finalMaxRows =
        match Config.maxRows with
        | Config.UseFsiPrintSize -> fsiConfig.fsiPrintSize
        | Config.UseRows value -> value

    let finalMaxTableWidth =
        match Config.maxTableWidth with
        | Config.TryUseConsoleThenFsiPrintWidth ->
            try Console.BufferWidth with _ -> fsiConfig.fsiPrintWidth
        | Config.UseWidth value -> value
        | Config.UseFsiPrintWidth -> fsiConfig.fsiPrintWidth

    let getColumnWidth (cells: string []) =
        cells
        |> Array.map (fun value -> value.Length)
        |> Array.fold max 0
        |> min Config.maxCellWidth

    let headerRow = properties |> Seq.map (fun prop -> prop, Left) |> Seq.toList

    let inputRows =
        [ for row in input |> Seq.truncate finalMaxRows do
            [ for cell in row do
                match cell with
                | null -> "", Left
                | v when isNumeric (v.GetType()) -> tostring v, Right
                | :? DateTime as v -> tostring v, Right
                | :? TimeSpan as v -> tostring v, Right
                | :? DateTime as v -> tostring v, Right
                | :? TimeSpan as v -> tostring v, Right
                // TODO: Special case formatting for sequences of prim. values
                | v ->
                    // TODO: multi line cell values?
                    sprintf "%A" v |> fun s -> s.Replace("\r", "").Replace("\n", "\\n"), Left
            ]
        ]

    let table =
        [
            yield headerRow

            // TODO: alignment
            yield! inputRows
        ]
        |> Seq.mapi (fun i row ->
            // prepend row index column
            let rowIndexString = if i = 0 then "" else string (i - 1)
            Seq.append [ rowIndexString, Left ] row
        )
        |> array2D

    let columnWidths =
        [
            for coli in [0.. Array2D.length2 table - 1 ] do
                yield getColumnWidth (table.[0.., coli] |> Array.map fst)
        ]

    let toCellValue (maxLength: int) (s: string) =
        if s.Length < maxLength then s else s.Substring(0, maxLength)

    let renderedCells =
        table |> Array2D.mapi (fun rowi coli (cellValue, alignment) ->
            // TODO: alignment
            let colWidth = columnWidths.[coli]
            let truncatedCellValue = toCellValue colWidth cellValue
            let normedCellValue =
                match alignment with
                | Left -> sprintf "%-*s " colWidth truncatedCellValue
                | Right -> sprintf "%*s " colWidth truncatedCellValue
            let ws = " "
            match rowi,coli with
            | 0,0 -> $"{normedCellValue}{String.replicate Config.rowCellSeparator.Length ws}"
            | _,0 -> $"{normedCellValue}{Config.rowCellSeparator}"
            | _ -> $"{normedCellValue}{Config.valueCellSeparator}"
        )

    let rawRowStrings =
        [
            for rowi in [0.. Array2D.length1 renderedCells - 1 ] do
                let rowValue = renderedCells.[rowi, 0..] |> String.concat ""
                yield renderedCells.[rowi, 0..] |> String.concat ""
                if rowi = 0 then
                    yield String.replicate rowValue.Length "-"
        ]

    rawRowStrings |> String.concat "\n" |> sprintf "\n%s\n"

// TODO: break table apart when width exceeds maxTableWidth / finalMaxTableWidth
type Table =
    static member toTable(fsiConfig: FsiConfig, input: IEnumerable<(string * string) seq>) =
        let input = input |> Seq.cache
        let properties =
            input 
            |> Seq.tryHead 
            |> Option.map (Seq.map fst)
            |> Option.map (fun row -> row |> Seq.toList)
            |> Option.defaultValue []
        let rows = input |> Seq.map (Seq.map snd >> Seq.cast<obj>)
        toTableString fsiConfig properties rows
    static member internal toTable(fsiConfig: FsiConfig, input: IEnumerable, seqType: Type) =
        let reflectionProps =
            seqType.GetProperties()
            |> Seq.filter (fun p -> p.CanRead)
        let properties =
            reflectionProps
            |> Seq.map (fun p -> p.Name)
            |> Seq.toList
        let enumerable =
            seq {
                for x in input |> Seq.cast<obj> do
                    seq {
                        for p in reflectionProps do
                            p.GetValue x
                    }
            }
        toTableString fsiConfig properties enumerable
    static member toTable (fsiConfig: FsiConfig, input: IEnumerable<'a>) =
        match input with SupportedEnumerable (enumerable, elemType) ->
            Table.toTable(fsiConfig, enumerable, elemType)


// TODO: test both cases

// TODO: Document
// TODO: Exclude value types for IEnumerable
type FsiTable =
    | NamedSeq of IEnumerable<(string * string) array>
    | TypedSeq of IEnumerable * Type

// TODO: Document
type TableMode = Implicit | Explicit

let addPrinters (fsi: InteractiveSession, mode: TableMode) =
    match mode with
    | Implicit -> 
        fsi.AddPrintTransformer (fun (x: IEnumerable) ->
            match x with
            | :? IEnumerable<(string * string) array> as x -> NamedSeq x
            | TrySupportedEnumerable x -> TypedSeq x
            | _ -> null
        )
    | _ -> ()

    // TODO: document this
    fsi.AddPrinter (fun (input: DateTime) ->
        input.ToString(Config.dateTimeFormat))
    fsi.AddPrinter (fun (input: TimeSpan) ->
        input.ToString(Config.timeSpanFormat))

    fsi.AddPrinter (fun (input: FsiTable) ->
        let fsiConfig =
            { formatProvider = fsi.FormatProvider
              fsiPrintSize = fsi.PrintSize
              fsiPrintWidth = fsi.PrintWidth }
        match input with
        | NamedSeq x -> Table.toTable(fsiConfig, x)
        | TypedSeq (enumerable, elemType) -> Table.toTable(fsiConfig, enumerable, elemType)
    )

