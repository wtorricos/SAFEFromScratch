module Index

open Elmish
open Fable.Remoting.Client
open Feliz
open Shared

Fable.Core.JsInterop.importAll "./Index.scss"

let greetingApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IGreetingApi>

type Model = { x: int; Greet: string option }

type Msg =
    | Increment
    | Decrement
    | GotGreeting of string
    | ApiError of exn

let init () =
    let model = { x = 0; Greet = None }

    // Queries the greet end point with the "Client" parameter
    // When it gets the response back it will send the GotGreeting message.
    let cmd = Cmd.OfAsync.either greetingApi.greet "Client" GotGreeting ApiError

    model, cmd

let update (msg: Msg) (state: Model) =
    match msg with
    | Increment -> { state with x = state.x + 1 }, Cmd.none
    | Decrement -> { state with x = state.x - 1 }, Cmd.none
    | GotGreeting msg -> { state with Greet = Some msg }, Cmd.none
    | ApiError exn -> { state with Greet = Some exn.Message }, Cmd.none

let view model dispatch =
    Html.div [
        Html.h1 (match model.Greet with Some msg -> msg | _ -> "Loading...")

        Html.button [ prop.onClick (fun _ -> dispatch Increment)
                      prop.text "Increment" ]

        Html.button [ prop.onClick (fun _ -> dispatch Decrement)
                      prop.text "Decrement" ]

        Html.h1 model.x ]
