module App

open Elmish
open Elmish.React
open Feliz
Fable.Core.JsInterop.importAll "./Program.scss"

type Model =
    { x : int }

type Msg =
    | Increment
    | Decrement

let init () =
    { x = 0 }, Cmd.ofMsg Increment

let update (msg: Msg) (state: Model) =
    match msg with
    | Increment -> { state with x = state.x + 1 }, Cmd.none
    | Decrement -> { state with x = state.x - 1 }, Cmd.none

let view model dispatch =
    Html.div [
        Html.button [
            prop.onClick (fun _ -> dispatch Increment)
            prop.text "Increment"
        ]

        Html.button [
            prop.onClick (fun _ -> dispatch Decrement)
            prop.text "Decrement"
        ]

        Html.h1 model.x
    ]

#if DEBUG
// integration for Remote DevTools like Redux dev tools. https://github.com/elmish/debugger
open Elmish.Debug
//  always include open Elmish.HMR after your others open Elmish.XXX statements. This is needed to shadow the supported APIs.
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
