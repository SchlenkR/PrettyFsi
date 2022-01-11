# PrettyFsi

> PrettyFsi is an F# Interactive table printer.

It's my contribution to the [F# Advent Calendar](https://sergeytihon.com/fsadvent), 11th Door, organized by [Sergey Tihon](https://twitter.com/sergey_tihon) - thank you very much for organizing this event :)

**Important**
    * PrettyFsi isn't a full-blown electric toy christmas present. It's more like a piece of chocolate in your advent calendar.
    * It's born from my daily needs.
    * I'm interested in other requirements and needs that could help, and there are quite a few TODOs. So feel free to raise an issue in case something doesn't work for you!

[![NuGet Badge](http://img.shields.io/nuget/v/PrettyFsi.svg?style=flat)](https://www.nuget.org/packages/PrettyFsi)

## Usage

See also: [demo.fsx](./demos/demo.fsx)

**Bootstrap**

```fsharp
#r "nuget: PrettyFsi"

open System
open PrettyFsi

PrettyFsi.addPrinters(fsi, TableMode.Implicit)
```

**Print table of objects**

```fsharp
type Test1 =
    { name: string
      degree: int
      weight: float
      birthDate: DateTime
      ids: int list }

[
    { name = "Hans Günther"
      degree = 23
      weight = 56.3
      birthDate = DateTime(2000, 12, 15, 23, 45, 00)
      ids = [ 1; 23; 45; 23; 556 ]
    }
    { name = "Jenny Lawrence"
      degree = 2
      weight = 56.3
      birthDate = DateTime(2000, 12, 15, 23, 45, 00)
      ids = [ 14; 63; 5; 8856 ]
    }
]

// result:
//
//       name              | degree  | weight  | birthDate            | ids                   | 
// ---------------------------------------------------------------------------------------------
// 0 :   "Hans Günther"    |     23  |   56.3  | 15.12.2000 23:45:00  | [1; 23; 45; 23; 556]  | 
// 1 :   "Jenny Lawrence"  |      2  |   56.3  | 15.12.2000 23:45:00  | [14; 63; 5; 8856]     | 

```

## Build / FSharp.Compiler.Interactive.Settings.dll reference

See [these instructions](./devDependeicies/README.md) for building the solution and resolving the `FSharp.Compiler.Interactive.Settings.dll` reference.
