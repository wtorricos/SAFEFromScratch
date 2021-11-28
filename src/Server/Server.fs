module Server

open Saturn
open Fable.Remoting.Server
open Fable.Remoting.Giraffe

open Shared

let greet name = sprintf $"Hello {name} from Saturn!"
let greetingApi = {
  greet = fun name ->
    async {
      return name |> greet
    }
}

// The type program is used as the entry point for WebApplicationFactory for testing.
type Program() =
    let greetingsRouter =
        Remoting.createApi ()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromValue greetingApi
        |> Remoting.buildHttpHandler


    let app = application {
        url "http://localhost:8085"
        use_router greetingsRouter
    }

    member x.main(_: string array) = run app


Program().main [||]
