﻿module App.Tests

open Fable.Mocha
open Index

let client = testList "Client" [
    testCase "Count starts in 0" <| fun _ ->
        let model, _ = init ()
        Expect.equal 0 model.x "Count should start at 0"

    testCase "Increase increments the count in 1" <| fun _ ->
        let model, _ = init ()
        let actual, _ = update Increment model
        Expect.equal 1 actual.x "Increment should increase the count in 1"

    testCase "Decrease decreases the count in 1" <| fun _ ->
        let model, _ = init ()
        let actual, _ = update Decrement { model with x = 1 }
        Expect.equal 0 actual.x "Decrease should decrement the count in 1"
]

let all =
    testList "All" [
        client
#if FABLE_COMPILER // This preprocessor directive makes editor happy
        Shared.Tests.shared
#endif
    ]

[<EntryPoint>]
let main _ = Mocha.runTests all
