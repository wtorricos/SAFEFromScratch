module Program

open Giraffe
open Saturn

let greet name = sprintf $"Hello {name} from Saturn!"

let routes = router {
    get "/api/foo" (text "Hello from Saturn!")
    getf "/api/foo/%s" (fun n -> n |> greet |> text)
}

let app =
    application {
        use_router routes
    }

run app
