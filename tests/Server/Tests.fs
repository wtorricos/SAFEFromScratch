module Tests

open Microsoft.AspNetCore.Mvc.Testing
open Xunit

open Main

[<Fact>]
let ``Greet welcome message`` () =
    let actual = greet "John"
    Assert.Equal("Hello John from Saturn!", actual)

type ServerFixture () =
    inherit WebApplicationFactory<Program>()

type IntegrationTests (factory: ServerFixture) =
    interface IClassFixture<ServerFixture>

    [<Fact>]
    member _.``Get greeting`` () =
        let client = factory.CreateClient()

        let response = client.GetAsync("/api/foo/John").Result

        response.EnsureSuccessStatusCode()

type IntegrationTests' () =
    let server = (new ServerFixture()).Server

    [<Fact>]
    let ``Get greeting`` () =
        let client = server.CreateClient()
        let response = client.GetAsync("/api/foo/John").Result
        response.EnsureSuccessStatusCode()
