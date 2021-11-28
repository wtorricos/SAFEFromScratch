module Shared.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open Shared

let shared = testList "Shared" [
    testCase "Test add" <| fun _ ->
        let actual = Helpers.add 1 2
        Expect.equal 3 actual "Should be 3"
]
