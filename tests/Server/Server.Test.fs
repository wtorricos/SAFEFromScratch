module Server.Tests

open System.Net
open System.Net.Http
open System.Text
open Microsoft.AspNetCore.Mvc.Testing
open Expecto

open Server

let serverUnitTests = testList "Server Unit tests" [
    testCase "Greet welcome message" <| fun _ ->
        let actual = greet "John"
        Expect.equal "Hello John from Saturn!" actual "Should greet John"
]

type ServerFixture () =
    inherit WebApplicationFactory<Program>()

let server = (new ServerFixture()).Server
let serverIntegrationTests = testList "Server Integration Tests" [
    testCase "Get greeting" <| fun _ ->
      let client = server.CreateClient()
      let content = new StringContent("""["John"]""", Encoding.UTF8);
      let response = client.PostAsync("/api/IGreetingApi/greet", content).Result
      Expect.equal HttpStatusCode.OK response.StatusCode "Should be successful response"
      ()
]

let all = testList "All" [ Shared.Tests.shared; serverUnitTests; serverIntegrationTests ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all
