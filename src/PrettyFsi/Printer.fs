module PrettyFsi

open System
open System.Collections
open System.Collections.Generic

module Config =
    
    // TODO: string trim/wrap
    // TODO: alignment per data type
    
    let mutable maxCellWidth = 50
    let mutable maxRows = 100
    let mutable maxTableWidth = try Console.BufferWidth with _ -> 250
    
    let mutable dateTimeFormat = "G"
    let mutable timeSpanFormat = "c"

    let mutable rowCellSeparator = ":   "
    let mutable valueCellSeparator = " | "

let private fsi = FSharp.Compiler.Interactive.Shell.Settings.fsi

type private Alignment = | Left | Right
type private Property = { name: string }

let private tryGetElementType (input: obj) =
    match input with
    | :? IEnumerable as enumerable ->
        // typeof<'a>
        enumerable.GetType().GetInterfaces()
        |> Array.tryFind (fun x -> 
            x.IsGenericType && 
            x.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>)
        |> Option.map (fun x -> x.GetGenericArguments().[0])
    | _ -> None

let private toTableString (properties: Property list) (input: IEnumerable<obj seq>) =
    let truncateString (maxLength: int) (s: string) =
        if s.Length < maxLength then s else s.Substring(0, maxLength)

    let getColumnWidth (cells: string []) =
        cells
        |> Array.map (fun value -> value.Length)
        |> Array.fold max 0
        |> min Config.maxCellWidth

    let headerRow = properties |> Seq.map (fun prop -> prop.name, Left) |> Seq.toList

    let inputRows =
        [ for row in input |> Seq.truncate Config.maxRows do
            [ for cell in row do
                match cell with
                | :? string as v ->
                    v, Left
                | :? float as v ->
                    v.ToString(fsi.FormatProvider), Right
                | :? Int32 as v ->
                    v.ToString(fsi.FormatProvider), Right
                | :? Int64 as v ->
                    v.ToString(fsi.FormatProvider), Right
                | :? DateTime as v ->
                    v.ToString(Config.dateTimeFormat), Right
                | :? TimeSpan as v ->
                    v.ToString(Config.timeSpanFormat), Right
                // TODO: Special case formatting for sequences of prim. values
                | null -> 
                    "", Left
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

    let renderedCells =
        table |> Array2D.mapi (fun rowi coli (cellValue, alignment) ->
            // TODO: alignment
            let colWidth = columnWidths.[coli]
            let truncatedCellValue = truncateString colWidth cellValue
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

    rawRowStrings |> String.concat "\n" |> sprintf "%s\n"

// TODO: break table apart when width exceeds max width
type Table =
    static member toTable (input: IEnumerable<(string * string) []>) =
        let properties =
            input 
            |> Seq.tryHead 
            |> Option.map (Array.map fst)
            |> Option.map (fun row -> 
                row 
                |> Seq.map (fun v -> { Property.name = v })
                |> Seq.toList)
            |> Option.defaultValue []
        toTableString properties (input |> Seq.map (Array.map snd >> Seq.cast<obj>))
    static member toTable (input: IEnumerable<'a>) =
        let reflectionProps =
            (tryGetElementType input).Value.GetProperties()
            |> Seq.filter (fun p -> p.CanRead)
        let properties =
            reflectionProps
            |> Seq.map (fun p -> { Property.name = p.Name })
            |> Seq.toList
        let enumerable =
            seq {
                for x in input |> Seq.cast<obj> do
                    seq {
                        for p in reflectionProps do
                            p.GetValue x
                    }
            }
        toTableString properties enumerable



let addTablePrinting() =
    fsi.AddPrinter (fun (input: IEnumerable<(string * string) []>) -> Table.toTable input)
    fsi.AddPrinter (fun (input: IEnumerable<'a>) -> Table.toTable input)
