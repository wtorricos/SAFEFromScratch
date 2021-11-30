module Server.Tests

open System.Net
open System.Net.Http
open System.Text
open Microsoft.AspNetCore.Mvc.Testing
open Expecto

open Shared
open Server

let serverUnitTests =
    testList
        "Server"
        [ testCase "Adding valid Todo"
          <| fun _ ->
              let storage = Storage()
              let validTodo = Todo.create "TODO"
              let expectedResult = Ok()

              let result = storage.AddTodo validTodo

              Expect.equal result expectedResult "Result should be ok"
              Expect.contains (storage.GetTodos()) validTodo "Storage should contain new todo" ]

open System
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection

/// .Net core utility for integration testing https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0#customize-webapplicationfactory
type ServerAppFactory<'T when 'T: not struct>() =
    inherit WebApplicationFactory<'T>()
    /// override the CreateHostBuilder method and return the Saturn application
    override _.CreateHostBuilder() = app

    /// override ConfigureWebHost to customize the Factory https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0#customize-webapplicationfactory
    override _.ConfigureWebHost builder =
        let configureServices (_: IServiceCollection) = ()

        builder
            .UseEnvironment("Test")
            .ConfigureServices(Action<IServiceCollection> configureServices)
        |> ignore

// We don't have a type for the Program, that's why we added this TestType
// ServerAppFactory only requires the generic parameter to be defined in the Server Assembly.
let server =
    (new ServerAppFactory<Server>()).Server

let serverIntegrationTests =
    testList
        "Server Integration Tests"
        [ testCase "Get todos"
          <| fun _ ->
              let client = server.CreateClient()

              let content =
                  new StringContent("", Encoding.UTF8)

              let response =
                  client
                      .PostAsync(
                          "/api/ITodosApi/getTodos",
                          content
                      )
                      .Result

              Expect.equal response.StatusCode HttpStatusCode.OK "Should be successful response"
              () ]

let all =
    testList
        "All"
        [ Tests.shared
          serverUnitTests
          serverIntegrationTests ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all
