#if INTERACTIVE
#r "../PrettyFsi/bin/Debug/netstandard2.0/PrettyFsi.dll"
#endif

module PrettyFsi.Tests.``ToTableString Tests``

open System
open System.Globalization
open FsUnit
open NUnit.Framework
open PrettyFsi

let normNewLine (s: string) = s.Replace("\r", "")

type Test1 =
    { name: string
      degree: int
      weight: float
      birthDate: DateTime
      ids: int list }

let [<TestCase>] ``Simple property as subset``() =
    
    let config =
        { FsiConfig.formatProvider = CultureInfo("de-DE")
          FsiConfig.fsiPrintSize = 250
          FsiConfig.fsiPrintWidth = 10
        }

    let res =
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
        |> Table(config).toTableString
    
    let expected = normNewLine """
      name              | degree  | weight  | birthDate            | ids                   | 
---------------------------------------------------------------------------------------------
0 :   "Hans Günther"    |     23  |   56.3  | 15.12.2000 23:45:00  | [1; 23; 45; 23; 556]  | 
1 :   "Jenny Lawrence"  |      2  |   56.3  | 15.12.2000 23:45:00  | [14; 63; 5; 8856]     | 
"""

    res |> should equal expected
    

