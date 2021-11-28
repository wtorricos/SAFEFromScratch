module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared

type Storage() =
    let todos = ResizeArray<_>()

    member __.GetTodos() = List.ofSeq todos

    member __.AddTodo(todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

let storage = Storage()

// Seed DB
[ "Create new SAFE project"; "Write your app"; "Ship it !!!" ]
|> List.iter (Todo.create >> storage.AddTodo >> ignore)


let todosApi =
    { getTodos = fun () -> async { return storage.GetTodos() }
      addTodo =
          fun todo ->
              async {
                  match storage.AddTodo todo with
                  | Ok () -> return todo
                  | Error e -> return failwith e
              } }

// The type program is used as the entry point for WebApplicationFactory for testing.
type Program() =
    let webApp =
        Remoting.createApi ()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromValue todosApi
        |> Remoting.buildHttpHandler


    let app = application {
        url "http://localhost:8085"
        use_router webApp
        // Adds the distributed memory cache https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-6.0
        memory_cache
        // Enables static files to be served https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-6.0
        use_static "public"
        // Response compression for assets https://docs.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-6.0
        use_gzip
    }

    member x.main(_: string array) = run app


Program().main [||]
