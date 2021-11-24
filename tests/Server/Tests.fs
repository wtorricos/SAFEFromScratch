module Tests

open System
open Xunit

open Program

[<Fact>]
let ``Greet welcome message`` () =
    let actual = greet "John"

    Assert.Equal("Hello John from Saturn!", actual)
