module Index

open Elmish
open Feliz

Fable.Core.JsInterop.importAll "./Index.scss"

type Model = { x: int }

type Msg =
    | Increment
    | Decrement

let init () = { x = 0 }, Cmd.ofMsg Increment

let update (msg: Msg) (state: Model) =
    match msg with
    | Increment -> { state with x = state.x + 1 }, Cmd.none
    | Decrement -> { state with x = state.x - 1 }, Cmd.none

let view model dispatch =
    Html.div [ Html.button [ prop.onClick (fun _ -> dispatch Increment)
                             prop.text "Increment" ]

               Html.button [ prop.onClick (fun _ -> dispatch Decrement)
                             prop.text "Decrement" ]

               Html.h1 model.x ]
