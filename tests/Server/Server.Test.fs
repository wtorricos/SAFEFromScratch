module Server.Tests

open System.Net
open System.Net.Http
open System.Text
open Microsoft.AspNetCore.Mvc.Testing
open Expecto

open Shared
open Server

let serverUnitTests = testList "Server" [
    testCase "Adding valid Todo" <| fun _ ->
        let storage = Storage()
        let validTodo = Todo.create "TODO"
        let expectedResult = Ok ()

        let result = storage.AddTodo validTodo

        Expect.equal result expectedResult "Result should be ok"
        Expect.contains (storage.GetTodos()) validTodo "Storage should contain new todo"
]

type ServerFixture () =
    inherit WebApplicationFactory<Program>()

let server = (new ServerFixture()).Server
let serverIntegrationTests = testList "Server Integration Tests" [
    testCase "Get todos" <| fun _ ->
      let client = server.CreateClient()
      let content = new StringContent("", Encoding.UTF8);
      let response = client.PostAsync("/api/ITodosApi/getTodos", content).Result
      Expect.equal HttpStatusCode.OK response.StatusCode "Should be successful response"
      ()
]

let all = testList "All" [ Tests.shared; serverUnitTests; serverIntegrationTests ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all
