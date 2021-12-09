
#r "./src/PrettyFsi/bin/Debug/netstandard2.0/PrettyFsi.dll"

open System
open PrettyFsi

PrettyFsi.addPrinters(fsi, TableMode.Implicit)


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
        let random = Random()
        let getRandomComment() = String.replicate (random.Next(3, 10)) "x"
        
        { name = "Hans"; degree = 23; weight = 56.3; birthDate = DateTime(2000, 12, 15, 23, 45, 00); address = address1; comment = getRandomComment() }
        { name = "Jürgen"; degree = 12; weight = 114.1; birthDate = DateTime(1983, 2, 3, 14, 05, 00); address = address2; comment = getRandomComment() }
        { name = "Hans"; degree = 23; weight = 56.3; birthDate = DateTime(2000, 12, 15, 23, 45, 00); address = address1; comment = getRandomComment() }
        { name = "Jürgen"; degree = 12; weight = 114.1; birthDate = DateTime(1983, 2, 3, 14, 05, 00); address = address2; comment = getRandomComment() }
        { name = "Hans"; degree = 23; weight = 56.3; birthDate = DateTime(2000, 12, 15, 23, 45, 00); address = address1; comment = getRandomComment() }
        { name = "Jürgen"; degree = 12; weight = 114.1; birthDate = DateTime(1983, 2, 3, 14, 05, 00); address = address2; comment = getRandomComment() }
        { name = "Hans"; degree = 23; weight = 56.3; birthDate = DateTime(2000, 12, 15, 23, 45, 00); address = address1; comment = getRandomComment() }
        { name = "Jürgen"; degree = 12; weight = 114.1; birthDate = DateTime(1983, 2, 3, 14, 05, 00); address = address2; comment = getRandomComment() }
        { name = "Hans"; degree = 23; weight = 56.3; birthDate = DateTime(2000, 12, 15, 23, 45, 00); address = address1; comment = getRandomComment() }
        { name = "Jürgen"; degree = 12; weight = 114.1; birthDate = DateTime(1983, 2, 3, 14, 05, 00); address = address2; comment = getRandomComment() }
        { name = "Hans"; degree = 23; weight = 56.3; birthDate = DateTime(2000, 12, 15, 23, 45, 00); address = address1; comment = getRandomComment() }
        { name = "Jürgen"; degree = 12; weight = 114.1; birthDate = DateTime(1983, 2, 3, 14, 05, 00); address = address2; comment = getRandomComment() }
    ]

input

//fsi.GetType().Assembly

fsi.AddPrinter (fun (x: DateTime) -> x.ToShortTimeString())

Nullable(DateTime.Now)

Nullable<DateTime>()

