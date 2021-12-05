#r "./src/PrettyFsi/bin/Debug/netstandard2.0/PrettyFsi.dll"

open System
open PrettyFsi


type Address =
    { city: string
      street: string
      zipCode: int }

type Test1 =
    { name: string
      degree: int
      weight: float
      birthDate: DateTime
      address: Address
      comment: string }

let address1 = { city = "Frankfurt"; street = "Hospitalstr."; zipCode = 60002 }
let address2 = { city = "NYC"; street = "Lex. Av."; zipCode = 43545 }

let input = 
    [
        let defaultComment = "abcdefgh abcdefgh abcdefgh abcdefgh"
        
        { name = "Hans"; degree = 23; weight = 56.3; birthDate = DateTime(2000, 12, 15, 23, 45, 00); address = address1; comment = defaultComment }
        { name = "Jürgen"; degree = 12; weight = 114.1; birthDate = DateTime(1983, 2, 3, 14, 05, 00); address = address2; comment = defaultComment }
        { name = "Hans"; degree = 23; weight = 56.3; birthDate = DateTime(2000, 12, 15, 23, 45, 00); address = address1; comment = defaultComment }
        { name = "Jürgen"; degree = 12; weight = 114.1; birthDate = DateTime(1983, 2, 3, 14, 05, 00); address = address2; comment = defaultComment }
        { name = "Hans"; degree = 23; weight = 56.3; birthDate = DateTime(2000, 12, 15, 23, 45, 00); address = address1; comment = defaultComment }
        { name = "Jürgen"; degree = 12; weight = 114.1; birthDate = DateTime(1983, 2, 3, 14, 05, 00); address = address2; comment = defaultComment }
        { name = "Hans"; degree = 23; weight = 56.3; birthDate = DateTime(2000, 12, 15, 23, 45, 00); address = address1; comment = defaultComment }
        { name = "Jürgen"; degree = 12; weight = 114.1; birthDate = DateTime(1983, 2, 3, 14, 05, 00); address = address2; comment = defaultComment }
        { name = "Hans"; degree = 23; weight = 56.3; birthDate = DateTime(2000, 12, 15, 23, 45, 00); address = address1; comment = defaultComment }
        { name = "Jürgen"; degree = 12; weight = 114.1; birthDate = DateTime(1983, 2, 3, 14, 05, 00); address = address2; comment = defaultComment }
        { name = "Hans"; degree = 23; weight = 56.3; birthDate = DateTime(2000, 12, 15, 23, 45, 00); address = address1; comment = defaultComment }
        { name = "Jürgen"; degree = 12; weight = 114.1; birthDate = DateTime(1983, 2, 3, 14, 05, 00); address = address2; comment = defaultComment }
    ]

let result = Table.toTable input

printfn "%s" result
