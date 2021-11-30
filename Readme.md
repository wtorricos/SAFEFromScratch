# SAFE template from scratch
In this guide we'll start from a blank project and start adding functionality so we can get a template similar to the [SAFE Template](https://safe-stack.github.io/docs/quickstart/), this way you will understand the role of every file and dependency.
You can follow this guide from top to bottom or you can review it with the git history as every step correspond to a commit that has the described changes.

Note that one difference with the SAFE template is that in this project we'll use .net6.0, along with the latest version of dotnet tools, npm packages and nuget packages while SAFE may not be in the latest version of some of them, for this reason some small differences may be seen.

# List of contents
- [1. Create the solution and projects](#solution)
- [2. Saturn](#saturn)
- [3. Server Unit tests](#server-unit-tests)
- [4. Server Integration Tests](#server-integration-tests)
- [5. Fable](#fable)
- [6. Create a bundle with Webpack](#webpack-bundle)
- [7. Webpack plugins](#webpack-plugins)
- [8. Loading Styles](#loading-styles)
- [9. Hot Reload](#hot-reload)
- [10. Fake Build](#fake-build)
- [11. Fantomas](#fantomas)
- [12. Elmish](#elmish)
- [13. Source maps and debugging](#debugging)
- [14. Client Unit Tests](#client-unit-tests)
- [15. Expecto](#expecto)
- [16. Clean the project](#clean-the-project)
  - 16.1 Webpack
  - 16.2 Client
  - 16.3 Server
  - 16.4 Build
  - 16.5 Shared tests
- [17. Client-Server Communication](#client-server-communication)
- [18. Prod Bundle](#prod-bundle)
- [19. Feliz.Bulma](#feliz-bulma)
- [20. Publish the application](#publish)
- [21. Paket optional](#paket)

#<h1 id="solution">Create the solution and projects</h1>

First we are going to create the solution and the main projects.
  - Create the solution: $ `dotnet new sln --name SafeFromScratch`
  - src folder: $ `mkdir src`
  - Client Project: 
    - create project (by default the folder name is used): `dotnet new console --output src/Server -lang F#`
  - Server Project:
    - `dotnet new console --output src/Server -lang F#`
  - Shared Project:
    - `dotnet new classlib --output src/Shared -lang F#`
  - Add projects to the solution:
    - `dotnet sln SafeFromScratch.sln add src/Client/Client.fsproj src/Server/Server.fsproj src/Shared/Shared.fsproj`
  - .gitignore
    - Create a .gitignore file in the root folder with the content specified below.
      - powershell: New-Item -Name ".gitignore" -ItemType "file"
      - Note that this is a simplistic .gitignore and we'll add more things as needed.
    - optional open the solution from an IDE: 
      - Create a virtual directory to navigate the project easier from an IDE: Add -> New Solution Folder -> `Solution Items`
      - Right click on `Solution Items` folder -> Add -> existing items -> .gitignore
      - Right click on `Solution Items` folder -> Add -> existing items -> Readme.md
```gitignore
# .gitignore

# Ignore IDE files
# VS Code
.vscode/
.ionide/
# Rider
.idea/
# Visual Studio
.vs/

# Dotnet Build files
obj/
bin/

# User Settings
*.user

# Mac files
*DS_Store
```

#<h1 id="saturn">Saturn</h1>

Add [Saturn](https://saturnframework.org/) package to the server project. 
  - $ `cd src/Server`
  - `dotnet add package Saturn`
Edit Program.fs with a minimal implementation that exposes an endpoint.
```f#
open Giraffe
open Saturn

let routes = router {
    get "/api/foo" (text "Hello from Saturn!")
}

let app =
    application {
        use_router routes
    }

run app
```
Now you can run the server `dotnet watch run` and test the endpoint we just added.

Saturn is build on top of [Giraffe](https://github.com/giraffe-fsharp/Giraffe), so alternatively you can work with Giraffe.

#<h1 id="server-unit-tests">Server Unit Tests</h1>

First create the project:
  - At the root of the repo run: `dotnet new xunit --output tests/Server -lang F# --name Server.Test`
  - Add the project to the solution: `dotnet sln SafeFromScratch.sln add tests/Server/Server.Test.fsproj`
  - cd tests/Server
  - you can run tests with: `dotnet test`
  - Reference the server project: `dotnet add reference ..\..\src\Server\Server.fsproj`

<h1 id="server-integration-tests">Server Integration Tests</h1>

Optionally you can add integration tests to the server.
  - Integration testing with [WebApplicationFactory](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0#test-app-prerequisites), some examples:
    - [dotnet-minimal-api-integration-testing](https://github.com/martincostello/dotnet-minimal-api-integration-testing)
    - [Clean Architecture](https://github.com/jasontaylordev/CleanArchitecture#:~:text=The%20easiest%20way%20to%20get%20started%20is%20to,the%20back%20end%20%28ASP.NET%20Core%20Web%20API%29%20) 
  - Add testing package: `dotnet add package Microsoft.AspNetCore.Mvc.Testing`
  - Create a Program to be the entry point and a WebApplicationFactory fixture for testing.

```f#
// The type program is used as the entry point for WebApplicationFactory for testing.
type Program () =
    let routes = router {
        get "/api/foo" (text "Hello from Saturn!")
        getf "/api/foo/%s" (fun n -> n |> greet |> text)
    }

    let app =
        application {
            use_router routes
        }

    member x.main (_: string array) = run app

Program().main [||]
```
Now you can create a WebApplicationFactory that you can use in your unit tests.
```f#
type ServerFixture () =
    inherit WebApplicationFactory<Program>()
type IntegrationTests (factory: ServerFixture) =
    interface IClassFixture<ServerFixture>

    [<Fact>]
    member _.``Get greeting`` () =
        let client = factory.CreateClient()

        let response = client.GetAsync("/api/foo/John").Result

        response.EnsureSuccessStatusCode()
```
Other style is to create an instance of WebApplicationFactory.
```f#
type IntegrationTests' () =
    let server = (new ServerFixture()).Server

    [<Fact>]
    let ``Get greeting`` () =
        let client = server.CreateClient()
        let response = client.GetAsync("/api/foo/John").Result
        response.EnsureSuccessStatusCode()
```
Note that you can override different methods of the WebApplication factory to customize the setup for your tests.

#<h1 id="fable">Fable<h1/>

First we need to install the fable compiler. 

The Fable compiler job is to convert your .fs files into javascript files.

We didn't create a tool manifest so we need to create one first: `dotnet new tool-manifest`
  - Now that we have a manifest we can start adding tools, starting with fable: `dotnet tool install fable`
  - optionally you can add the .config/dotnet-tools.json file to the `Solution Items` virtual folder.
  - Now you can `run dotnet fable` to compile the client project
All of our tools can be found in the `.config/dotnet-tool.json` file
```js
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "fable": {
      "version": "3.6.1",
      "commands": [
        "fable"
      ]
    }
  }
}
```
Now we can add the Fable packages to the Client.  
  - Move to the client folder: `cd src/Client`
  - Add the fable packages: 
    - `dotnet add package Fable.Browser.DOM`
    - `dotnet add package Fable.Core`

Now you can run dotnet `fable watch src/Client` which will transpile your .fs files to JS.
  - After you run the command notice how the Program.fs now has a Program.fs.js generated by Fable.

Now we need a simple html entry point, for this create a `src/Client/index.html` file and import the generated JS file that will be generated by Fable.

index.html
```html
<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Fable</title>
</head>
<body>
    <h1>Building from scratch!</h1>
    <h2 id="hello"></h2>
    <!--  we are going to generate the bundle.js file with fable  -->
    <script src="Program.fs.js"></script>
</body>
</html>
```
Program.fs
```f#
module App
open Browser.Dom
let helloH2 = document.querySelector("#hello") :?> Browser.Types.HTMLDivElement
helloH2.innerText <- "Welcome to Fable!!"
```
Note that if you open index.html in your browser you will get an error because fable transpiles code using features that are not supported by all browsers (In this case module imports and exports)


#<h1 id="webpack-bundle">Create a bundle with Webpack</h1>

Use [webpack](https://webpack.js.org/guides/installation/) module bundler to create a bundle.js file and solve the module issues we have seen above in the generated js file.
  - In the root of the project run: `npm init` to create the package.json file
    - remember to add "private": true, to make it explicit that this package won't be published. 
  - Let's add webpack with `npm install webpack webpack-cli --save-dev`
    - [webpack-cli](https://webpack.js.org/api/cli/) to run webpack using npx or as an npm command.
  - Add node_modules/ to the .gitignore file. 
  - Manually create a `webpack.config.js` file or run `npx webpack init`. 
    - webpack.config.js is the default file that webpack looks for.
  - Now you can run `npx webpack` and our bundle.js file will be generated.
  - We are going to send the bundle to a dist folder in the client so add the dist/ folder to .gitignore.
  - All that's missing is to update our index.js to reference bundle.js instead of Program.fs.js
  - optionally you can add package-lock.json to the `Solution Items` virtual folder. 

Minimal webpack.config.js
```js
const path = require("path");

module.exports = {
    // required property, either "development" or "production".
    mode: "development",
    // Webpack uses this file as a starting point for dependency tree walking.
    // We use the main file generated by Fable.
    entry: "./src/Client/Program.fs.js",
    // the resulting output
    output: {
        // An absolute path for the resulting bundle.
        path: path.join(__dirname, "./src/Client/dist"),
        // the resulting file, by convention bundle.js
        filename: "bundle.js",
    }
}
```
Update index.html to point to the generated bundle file
```html
<script src="./dist/bundle.js"></script>
```
You can also add a build task to the package.json file:
```js
"scripts": {
    "build": "webpack --config webpack.config.js"
  }
```
Now you can run `npm run build` instead of `npx webpack`. 
If you are wondering why you can't run `webpack` directly from the command line, this is because we don't have it as a global package,
You don't have this problem when you add this command to the package.json file because when you run it from there the context will be the project and it will look for webpack in the node_modules folder.

#<h1 id="webpack-plugins">Webpack plugins</h1>

We are going to start with a simple plugin that will allow us to copy files from a public folder to the dist folder.
  - install the npm copy-webpack-plugin plugin: `npm install copy-webpack-plugin --save-dev`
  - We are going to copy the index.html file along with public assets like a favicon.png file.
  - Create a public directory in the Client project and move the index.html there, you can add a favicon or other assets as well.
Now you need to update the webpack.config.js file to use the plugin:
```js
// import the plugin
const CopyPlugin = require("copy-webpack-plugin");
```
Now use the plugin to copy the files from the `from` directory to the output folder (in our case dist)
```js
plugins: [
        new CopyPlugin({
            patterns: [
                // by default copies to output folder
                { from: path.join(__dirname,'./src/Client/public') }
            ],
        }),
    ]
```
Now that we are copying our html file from the public folder to the distribution folder we also need to update the reference to the bundle file:
```html
<script src="bundle.js"></script>
```

#<h1 id="loading-styles">Loading Styles</h1>

We are going to use webpack loaders in order to load css, sass and scss files. 
  - Check the official docs for more details: [webpack sass loader](https://webpack.js.org/loaders/sass-loader/)
  - install webpack css packages: `npm install --save-dev style-loader css-loader sass-loader sass`
Now update the webpack.config.js file to chain the sass-loader with the css-loader and the style-loader to immediately apply all styles to the DOM.
```js
module: {
    // Loaders allow webpack to process files other than JS and convert them into valid
    // modules that can be consumed by your application and added to the dependency graph
    rules: [
        // style loaders
        {
            // The test property identifies which file or files should be transformed.
            test: /\.(sass|scss|css)$/,
            // The use property indicates which loader should be used to do the transforming.
            use: [
                'style-loader',
                'css-loader',
                'sass-loader'
            ]
        }
    ]
}
```
Now you can import scss files in your fs files with `Fable.Core.JsInterop.importAll "./Program.scss"`
  - Note that you still need to run `dotnet fable` and `npm run build` every time you make a change

In order to check the actual files in the dev console you need to add [source maps](https://webpack.js.org/loaders/sass-loader/#sourcemap)
In this case this is really straight forward an you can just update loaders like this:
```js
use: [
    'style-loader',
    {
        loader: 'css-loader',
        options: {
            sourceMap: true,
        },
    },
    {
        loader: 'sass-loader',
        options: {
            sourceMap: true,
        },
    }
]
```

#<h1 id="hot-reload">Hot Reload</h1>

One of the most productive features you can get with webpack is hot reload and is really simple to add.
  - First install the dev-server package: `npm install --save-dev webpack-dev-server`
  - Add the [devServer](https://webpack.js.org/guides/hot-module-replacement/) configuration to the webpack.config.js file.
```js
devServer: {
        static: './src/Client/dist',
        hot: true,
        port: 8080,
    },
```
Now you can run `dotnet fable watch --run webpack-dev-server` to run fable and the web server in parallel and see your changes be reloaded in real time.

#<h1 id="fake-build">Fake Build</h1>

We still need to start the server, the client and fable. So it's time to simplify this process with the help of [fake](https://fake.build/)
Although we could use the fake-cli along with a .fsx script we are going to use the same approach as the SAFE template and create a build project.
  - Create a Build project in the root folder of the repo.
    - dotnet new console -lang F# --name Build --output ./
    - Note output ./ this is very important since we want the .fsproj file to be in the root folder so we can call commands with dotnet run directly.
  - Add Fake.Core.Target package `dotnet add package Fake.Core.Target`.
  - Add Fake.IO.FileSystem `dotnet add package Fake.IO.FileSystem` we are going to use it to delete folders in the clean step later.
  - Add the project to the solution.
Let's create our first Target, modify Program.fs so it looks like this:
```f#
open Fake.Core
Target.create "Run" (fun _ ->
    printfn "-- run --"
    )
[<EntryPoint>]
let main args =
    Target.runOrDefault "Run" // always run the Run target 
    0
```
If we execute `dotnet run` which will invoke the Run Target it will fail miserably,this is because we first need to add a context in which fake will run.
Inspired in the Fable template let's add the following lines before the Run target.
```f#
let execContext = Context.FakeExecutionContext.Create false "build.fsx" []
Context.setExecutionContext (Context.RuntimeContext.Fake execContext)
```
if we execute `dotnet run` now we will see `-- run --` printed in the console. Now we are ready to add some real tasks.
```f#
open Fake.Core
open Fake.IO

// initialize context for Fake to run
let execContext = Context.FakeExecutionContext.Create false "build.fsx" []
Context.setExecutionContext (Context.RuntimeContext.Fake execContext)

let serverPath = "./src/Server"
let clientPath = "./src/Client"
let clientPathDistFolder = "./src/Client/dist"

// helper functions to run terminal commands.
let createProcess exe arg dir =
    CreateProcess.fromRawCommandLine exe arg
    |> CreateProcess.withWorkingDirectory dir
    |> CreateProcess.ensureExitCode

let runProcess proc = proc |> Proc.run

// Define targets, we can run each target with dotnet run for example `dotnet run Clean`
Target.create "Clean" (fun _ ->
    createProcess "dotnet" "clean" serverPath |> runProcess |> ignore
    createProcess "dotnet" "clean" clientPath |> runProcess |> ignore
    createProcess "dotnet" "fable clean --yes" clientPath |> runProcess |> ignore
    Shell.cleanDir clientPathDistFolder
    )

Target.create "Run" (fun _ ->
    createProcess "dotnet" "build" serverPath |> runProcess |> ignore
    createProcess "dotnet" "build" clientPath |> runProcess |> ignore
    [| createProcess "dotnet" "watch run" serverPath
       createProcess "dotnet" $"fable watch {clientPath} --run webpack-dev-server" "." |]
    |> Array.Parallel.map runProcess
    |> ignore
    )

// The entry point allows us to run any task defined as a Target.
// Ex. dotnet run Clean
[<EntryPoint>]
let main args =
    try
        match args with
        | [| target |] -> Target.runOrDefault target
        | _ -> Target.runOrDefault "Run"
        0
    with e ->
        printfn $"{e}"
        1
```
Now we can just use `dotnet run Clean` to clean our projects or `dotnet run` to run the Client and the Server in parallel with hor reload and everything else in place.
Additionally we can add dependencies like:
```f#
// Define dependencies
open Fake.Core.TargetOperators
let dependencies = [
    "Clean" // Clean has no dependencies at the moment
        ==> "Run" // Run depends on Clean so Clean will run first every time we call Run 
]
```

#<h1 id="fantomas">Fantomas</h1>

[Fantomas](https://github.com/fsprojects/fantomas) is a tool to format the src code.
  - To add the tool run the following command in the root folder: dotnet tool install fantomas-tool
  - Add a new target task on fake to easily run the tool and format your code
```f#
Target.create "Format" (fun _ ->
    createProcess "dotnet" "fantomas . -r" "./src" |> runProcess |> ignore
)
```
  - Try it with `dotnet run Format`

#<h1 id="elmish">Elmish</h1>

Now that we can easily build the project with Fake, we are going to add Elmish to the Client.
- Navigate to the client: `cd src/Client`
- Add the Elmish package `dotnet add package Fable.Elmish`

- Let's update Client/Program.fs to look like the basic [Elmish sample](https://elmish.github.io/elmish/basics.html):
```f#
open Elmish
Fable.Core.JsInterop.importAll "./Program.scss"

type Model =
    { x : int }

type Msg =
    | Increment
    | Decrement

let init () =
    { x = 0 }, Cmd.ofMsg Increment

let update msg model =
    match msg with
    | Increment ->
        { model with x = model.x + 1 }, Cmd.none
    | Decrement ->
        { model with x = model.x - 1 }, Cmd.none

let view = (fun model _ -> printfn $"{model}")

Program.mkProgram init update view
|> Program.run
```

Now let's improve the view with Feliz, for this we have two options: 
1. Manually install the nuget package and then the npm package.
- Add the Elmish package `dotnet add package Feliz`
- install the npm dependencies: `npm install react@17.0.1 react-dom@17.0.1`
2. Use [femto](https://github.com/Zaid-Ajaj/Femto) to install the package.
- Since this is the first time we are going to use femto first install the tool: `dotnet tool install femto`
- Use femto to install both nuget and npm packages: `dotnet femto install Feliz src/Client/Client.fsproj`
- Femto compatible libraries can be found in [awesome-safe-components](https://safe-stack.github.io/docs/awesome-safe-components/)

Now we are also going to also add [Feliz.UseElmish](dotnet add package Feliz.UseElmish) so we can create functional components that use hooks.
- `dotnet add package Feliz.UseElmish`

Also Elmish.React which has some extension methods to hook our react components into Elmish.
- `dotnet femto install Fable.Elmish.React src/Client/Client.fsproj`

Now you can update src/Client/Program.fs and use Feliz and react.
```f#
open Elmish
open Elmish.React
open Feliz
(* ... *)
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

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "elmish-app" // this is the dom element that react will hook up to.
|> Program.run
```
You also need to update the public/index.html file and add the `elmish-app` element to hook app our application
```html
<body>
    <!--  React will bind to this element to insert it's content.  -->
    <div id="elmish-app"></div>
    <!--  we are going to generate the bundle.js file with fable  -->
    <script src="bundle.js"></script>
</body>
```

#<h1 id="debugging">Source maps and debugging</h1>

We already added source maps for the css files, similarly we are going to add source maps for the js files. 
First we need to add a devtool to the webpack.config.js file:
```js
// integrated in webpack, controls how source maps are generated https://webpack.js.org/configuration/devtool/
devtool: "eval-source-map",
```
And also a js source map loader, so first install it with: `npm install --save-dev source-map-loader`
And then add it below the css loader:
```js
// JS source map loader https://webpack.js.org/loaders/source-map-loader/
// extracts existing source maps from all JavaScript entries and passes them to the specified devtool
{
    test: /\.js$/,
    enforce: "pre",
    use: ['source-map-loader']
}
```
If you run the application and review the sources you will see some nicely formatted js files, however we want to see our fs files.
For this we need to modify the command to run fable and pass the --sourceMaps option:

Build/Program.fs Run Target
```f#
createProcess "dotnet" $"fable watch {clientPath} --sourceMaps --run webpack-dev-server" "."
```
Now you may want to add `*.js.map` to .gitignore.

Now we are able to debug our fs files!

Finally we are going to add some additional tools for development:
- Integration for [Remote DevTools](https://github.com/elmish/debugger) 
  - `dotnet add package Fable.Elmish.Debugger`
  - `npm install --save-dev  remotedev@^0.2.9`
[HMR](https://elmish.github.io/hmr/) allows us to modify the application while it's running, without a full refresh
  - `dotnet add package Fable.Elmish.HMR`

Now let's integrate this tools with elmish in the Client/Program.fs
```f#
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
```

#<h1 id="client-unit-tests">Client Unit Tests</h1>

Remember that our client although written in F# is finally run on a web browser and with Javascript,
for this reason we test it with the [Mocha](https://mochajs.org/) Javascript testing framework. 
For this we are going to use the [Fable.Mocha](https://github.com/Zaid-Ajaj/Fable.Mocha) library that easily integrates with our F# code.

Create the test project: 
  - Run: `dotnet new console --output tests/Client -lang F# --name Client.Test` 
  - Add the project to the solution: `dotnet sln SafeFromScratch.sln add tests/Client/Client.Test.fsproj`
  - Reference the client project: `dotnet add tests\Client\Client.Test.fsproj reference src\Client\Client.fsproj`
 
Add [Fable.Mocha](https://github.com/Zaid-Ajaj/Fable.Mocha) package
  - `cd tests/Client`
  - Install with femto: `dotnet femto install Fable.Mocha`

Time to add our unit tests to tests/Client/Program.fs
```f#
module Client.Tests

open Fable.Mocha
open App

let client = testList "Client" [
    testCase "Count starts in 0" <| fun _ ->
        let model, _ = init ()
        Expect.equal 0 model.x "Count should start at 0"

    testCase "Increase increments the count in 1" <| fun _ ->
        let model, _ = init ()
        let actual, _ = update Increment model
        Expect.equal 1 actual.x "Increment should increase the count in 1"

    testCase "Decrease decreases the count in 1" <| fun _ ->
        let actual, _ = update Increment { x = 1 }
        Expect.equal 0 actual.x "Decrease should decrement the count in 1"
]

let all = testList "All" [ client ]

[<EntryPoint>]
let main _ = Mocha.runTests all
```
But we are not ready to run the tests yet, similar to the Client project we need to use fable to compile the project and then run it with webpack.
For this we are going to create a webpack.test.config.js, this file will be quite similar to the webpack.config.js file we already have, so just copy paste it and change the name.
Once you have your webpack.tests.config.js you can add it to your `Solution Items` virtual folder. 
Remember that our webpack.config.js has the minimal configuration needed for development and that's exactly what we need so there are just a few changes that we need to do.

First update the entry:
```js
// from entry: "./src/Client/Program.fs.js" to:
entry: "./tests/Client/Program.fs.js",
```
Similarly update all the references to `src/Client` to `tests/Client`

Remember that our config will copy the files from the public folder including the index.html file, so you also need to create an index.html file. 
For this we can just copy paste the index.html file from the Client project.

Now you can run the tests:
- Run fable to compile the code: `dotnet fable --sourceMaps --cwd tests/Client`
- Build with webpack: `npx webpack --config webpack.tests.config.js`
- If you open the dist/index.html file you will see a report in the browser.

We can also run them in one step with watch enabled: 
- `dotnet fable watch tests/Client --sourceMaps --run webpack-dev-server --config webpack.tests.config.js`

But we now that there is an even better way to run this commands with Fake, so next we are going to add A Target task:
```f#
Target.create "ClientTests" (fun _ ->
    createProcess
        "dotnet" $"fable watch {clientPath} --sourceMaps --run webpack-dev-server --config webpack.tests.config.js"
        "."
    |> runProcess
    |> ignore)
```
And now we can run: `dotnet run ClientTests`

#<h1 id="expecto">Expecto</h1>

Now we are going to add [Expecto](https://github.com/haf/expecto) to the server tests.
- First add the package: `cd tests/Server` and then `dotnet add package Expecto`

Now you need to update the .fsproj file to be an executable and we can remove xunit since we are not going to use it anymore:
```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net6.0</TargetFramework>
        <GenerateProgramFile>false</GenerateProgramFile>
    </PropertyGroup>

    <ItemGroup>
        <Compile Include="Tests.fs" />
    </ItemGroup>

    <ItemGroup>
        <PackageReference Include="Expecto" Version="9.0.4" />
        <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="6.0.0" />
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="16.11.0" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\..\src\Server\Server.fsproj" />
    </ItemGroup>

</Project>
```
Not it's turn to update our unit tests and use Expecto instead of XUnit
```f#
module Server.Tests

open System.Net
open Microsoft.AspNetCore.Mvc.Testing
open Expecto

open Main

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
      let response = client.GetAsync("/api/foo/John").Result
      Expect.equal HttpStatusCode.OK response.StatusCode "Should be sucessful response"
      ()
]

let all = testList "All" [ serverUnitTests; serverIntegrationTests ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all
```
You can run your tests with the command: `dotnet watch run`

However we now it's better to add a Fake task for this, so we are going to update the `ClientTests` task to be just `Tests` and we are going to run both Client and Server Tests.
```f#
Target.create "Tests" (fun _ ->
    [| createProcess "dotnet" "watch run" serverTestsPath
       createProcess
        "dotnet" $"fable watch {clientTestPath} --sourceMaps --run webpack-dev-server --config webpack.tests.config.js"
        "." |]
    |> Array.Parallel.map runProcess
    |> ignore)
```

#<h1 id="clean-the-project">Clean the project</h1>

We are in really good shape now, we finished setting up the client and the server and we are ready to start adding some features.
However we added a bunch of small changes and we need to clean the project a little bit so it's more maintainable.

## Webpack
Let's start by cleaning our webpack configuration, for this we are just going to take the hardcoded paths and take them to a CONFIG object.
```js
const CONFIG = {
    fsharpEntry: "./src/Client/Program.fs.js",
    outputDir: "./src/Client/dist",
    assetsDir: './src/Client/public',
    devServerPort: 8080,
}
```

## Client
For the client we are actually going to split the Program.fs project in two: App.fs and Index.fs

Move the setup of Elmish to run the application to App.fs
```f#
module App

open Elmish
open Elmish.React

#if DEBUG
// integration for Remote DevTools like Redux dev tools. https://github.com/elmish/debugger
open Elmish.Debug
//  always include open Elmish.HMR after your others open Elmish.XXX statements. This is needed to shadow the supported APIs.
open Elmish.HMR
#endif

Program.mkProgram Index.init Index.update Index.view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
```
Now rename Program.fs to Index.fs and rename the module App to Index, you may want to update Program.scss to Index.scss and update the import as well.

Don't forget to update the Unit tests for the client as well, now you need to open the Index module instead of the App module.
Also it would be better to rename tests/Client/Program.fs to App.Test.fs. 

Looking a lot better now, however we are going to have some problems with webpack since webpack is going to look for Program.fs.js which will no longer be generated.

For this we are going to go one step further and we are going to use the [html-webpack-plugin](https://github.com/jantimon/html-webpack-plugin) which will add the bundled js and css files to the index.html page for us.
- Move index.html back to root folder of the Client project.
- Install the plugin: `npm install --save-dev html-webpack-plugin`

Update webpack.config.js
- First load the plugin and add a new configuration for the index.html path and update the fsharpEntry.
```js
var HtmlWebpackPlugin = require('html-webpack-plugin');

const CONFIG = {
    // The tags to include the generated JS and CSS will be automatically injected in the HTML template
    // See https://github.com/jantimon/html-webpack-plugin
    indexHtmlTemplate: './src/Client/index.html',
    fsharpEntry: "./src/Client/App.fs.js",
    /* ... */
}
```
- Use the plugin:
```js
plugins: [
        new HtmlWebpackPlugin({
            filename: 'index.html',
            template: path.join(__dirname, CONFIG.indexHtmlTemplate)
        }),
        /* ... */
]
```
- We no longer need to manually load the bundle.js file inside index.html, so you can remove the following line:
```html
<script src="bundle.js"></script>
```

So far so good, the client is looking a lot better but as we keep adding more files fable will generate more files as well and it will pollute our Client folder, 
so let's update our Target task to use a specific folder that we'll call output.
- Update the Target Run task to use the output folder:
```f#
Target.create "Run" (fun _ ->
    (* ... *)
       createProcess "dotnet" "fable watch --outDir output --sourceMaps --run npm run start --prefix ../.." clientPath
    (* ... *)
    )
```
- Note that we are now running `npm run start` to run webpack, this is an improvement that will let us better customize the start task in the package.json file if necessary.
- Now update the fsharpEntry:
```js
const CONFIG = {
    fsharpEntry: "./src/Client/output/App.js",
    /* ... */
}
```
Now repeat the same steps to update the webpack.tests.config.js and test Target task.

Additionally for the test project now that we are using HtmlWebpackPlugin we can simplify the index.html file a lot since we no longer need to load anything manually:
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <title>SafeHello Client Tests</title>
</head>

<body>
</body>

</html>
```

## Server
The server is going to be really easy, just rename src/Server/Program.fs to src/Server/Server.fs and tests/Server/Tests.fs to tests/Server/Server.Tests.fs 

## Build 
Although our Build project works great, the SAFE template added some really cool helpers to print with color and some high order functions to execute commands.
Instead of implementing them one by one I'm just going to copy the Helpers.fs file from the SAFE template and add some comments and small modifications that may help you review it.
Finally we are going to use this helper functions to simplify our Target tasks.

## Shared tests
Add the shared tests project for completeness.
The Shared project is referenced by both Client and Server, so it also needs to be run in both environments.
For this reason we create the project as a library that we'll reference in both test projects as well.
- Add the project: `dotnet new classlib --output tests/Shared -lang F# --name Shared.Test`
- Add it to the solution: `dotnet sln SafeFromScratch.sln add tests/Shared/Shared.Test.fsproj`
- Since this project is going to run in both Client and Server we are going to add both Expecto and Fable.Mocha.
  - `cd tests/Shared`
  - Install Fable.Mocha with femto: `dotnet femto install Fable.Mocha`
  - Add Expecto package: `dotnet add package Expecto`
- Add a reference to the shared project: `dotnet add reference ..\..\src\Shared\Shared.fsproj`
- Add Shared.Tests.fs (Note that I added a simple `let add a b = a + b` function in shared that we can test. )
```f#
module Shared.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open Shared

let shared = testList "Shared" [
    testCase "Test add" <| fun _ ->
        let actual = Helpers.add 1 2
        Expect.equal 3 actual "Should be 3"
]
```
- Pay attention to this line `#if FABLE_COMPILER` for this preprocessor directive to work we need to add it to the Client.fsproj file
```html
<PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <DefineConstants>FABLE_COMPILER</DefineConstants>
  </PropertyGroup>
```
- This way whenever the Client.fsproj is included (inside Client.Tests.fsproj for example) this directive will be present.
- Now it's time to include our tests along the Client and Server tests, for this you need to add a reference to the Shared.Test project in both test projects.

Server.Tests.fs
```f#
let all = testList "All" [ Shared.Tests.shared; serverUnitTests; serverIntegrationTests ]
```
Client.Tests.fs
```f#
let all =
    testList "All" [
        client
#if FABLE_COMPILER // This preprocessor directive makes editor happy
        Shared.Tests.shared
#endif
    ]
```
Now if you run `dotnet run RunTests` notice how your Shared.Tests are run along with both Client and Server tests.

## Bonus
Our code looks pretty good now, but let's run fatomas to make sure is well formatted: `dotnet run Format`

#<h1 id="client-server-communication">Client-Server Communication</h1>

In order to send requests from the client to the server we are going to use [Fable.Remoting](https://github.com/Zaid-Ajaj/Fable.Remoting).
- Add Fable.Remoting.Client to the Client project: `dotnet add src/Client/Client.fsproj package Fable.Remoting.Client`
- Add Fable.Remoting.Giraffe to the Server project: `dotnet add src/Server/Server.fsproj package Fable.Remoting.Giraffe`
- Add a shared interface to Shared/Shared.fs
```f#
type IGreetingApi = {
  greet : string -> Async<string>
}
```
- Reference the shared project from the Client and the Server.
- `dotnet add src/Client/Client.fsproj reference src/Shared/Shared.fsproj`
- `dotnet add src/Server/Server.fsproj reference src/Shared/Shared.fsproj`
- Implement the interface in the Server
```f#
open Shared

let greet name = sprintf $"Hello {name} from Saturn!"
let greetingApi = {
  greet = fun name ->
    async {
      return name |> greet
    }
}
```
- Use the implementation to generate a route
```f#
open Saturn
open Fable.Remoting.Server
open Fable.Remoting.Giraffe

let greetingsRouter =
    Remoting.createApi ()
    |> Remoting.fromValue greetingApi
    |> Remoting.buildHttpHandler

let app = application { use_router greetingsRouter }
```
- Create a proxy in the client that will let you perform requests:
```f#
open Fable.Remoting.Client
open Shared

let greetingApi =
    Remoting.createApi ()
    |> Remoting.buildProxy<IGreetingApi>
```
- Call the api inside the init function:
```f#
type Model = { x: int; Greet: string option }

type Msg =
    | Increment
    | Decrement
    | GotGreeting of string

let init () =
    let model = { x = 0; Greet = "" }

    // Queries the greet end point with the "Client" parameter
    // When it gets the response back it will send the GotGreeting message.
    let cmd = Cmd.OfAsync.perform greetingApi.greet "Client" GotGreeting

    model, cmd
```
- You also need to handle the GotGreeting message in the update function:
```f#
| GotGreeting msg -> { state with Greet = Some msg }, Cmd.none
```
- Finally let's display the message in the view:
```f#
Html.h1 (match model.Greet with Some msg -> msg | _ -> "Loading...")
```

We have our code in place the question is, will it work?
If you run the tests `dotnet run RunTests` you will see that we already have a failing test (Note: fix any problem you encounter in the tests for the projects to compile.)

We can see that our Server Integration Test fails with NotFound, this is because although we added the endpoint the route changed.
I'm going to skip one step and go straight to borrow the Router.builder from the SAFE template and add it to the Shared.fs file:
```f#
module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName
```
Note: As the implementation suggests this builder will generate routes of the type `/api/IGreetingApi/greet` 
Now we need to use it in the server to construct our greetingRouter:
```f#
let greetingsRouter =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue greetingApi
    |> Remoting.buildHttpHandler
```
Our test is still failing since the route is still different, in this case I'm going to update the test implementation:
```f#
let serverIntegrationTests = testList "Server Integration Tests" [
    testCase "Get greeting" <| fun _ ->
      let client = server.CreateClient()
      let content = new StringContent("""["John"]""", Encoding.UTF8);
      let response = client.PostAsync("/api/IGreetingApi/greet", content).Result
      Expect.equal HttpStatusCode.Ambiguous response.StatusCode "Should be successful response"
      ()
]
```
Now our test passes but if we run the application `dotnet run` the request fails, this is because we need to use the same Route.builder for our proxy:
```f#
Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IGreetingApi>
```
Now we have all of our code in place but it still fails, if you check the console you will see that the requests are being performed to the http://localhost:8080 url
but our server is running in the http://localhost:5000 port.
- First let's make our server run in the port 8085 like the SAFE one:
```f#
let app = application {
    url "http://localhost:8085"
    use_router greetingsRouter
}
```
- Now let's make the client sends the request to the correct Server url, for this we are going to modify webpack.config.js and add a server proxy that will route the requests to the correct endpoint:
```js
const CONFIG = {
    /* ... */
    
    // When using webpack-dev-server, you may need to redirect some calls
    // to a external API server. See https://webpack.js.org/configuration/dev-server/#devserver-proxy
    devServerProxy: {
        // redirect requests that start with /api/ to the server on port 8085
        '/api/**': {
            target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
            changeOrigin: true
        },
        // redirect websocket requests that start with /socket/ to the server on the port 8085
        '/socket/**': {
            target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
            ws: true
        }
    }
}

module.exports = {
    /* ... */
    devServer: {
        /* ... */
        proxy: CONFIG.devServerProxy,
    },
}
```
If we run the application again `dotnet run` we'll see it working!

## Bonus
Use either instead of perform to handle a request error:
```f#
let cmd = Cmd.OfAsync.either greetingApi.greet "Client" GotGreeting ApiError
```

#<h1 id="prod-bundle">Prod Bundle</h1>

Our project looks great and we are ready to start adding features, however at some point we'll need to deploy to production and we not ready for that yet.

## Update webpack
- First we need to identify which environment are we using:
```js
// The NODE_ENV is passed to npm for example: npm run start --node-env=development
process.env.NODE_ENV = process.env.NODE_ENV ?? "development";
const isProduction = process.env.NODE_ENV === 'production';
const environment = isProduction ? 'production' : 'development';
console.log('Bundling for ' + environment + '...');
```
- Once we know the environment we can start updating the configuration:
```js
const CONFIG = {
    outputDir: "./deploy/public",
}

module.exports = {
    mode: environment,
    devtool: isProduction ? 'source-map' : 'eval-source-map',
    filename: isProduction ? '[name].[fullhash].js' : '[name].js'
}
```
- Now let's update the plugins and start by adding the [MiniCssExtractPlugin](https://webpack.js.org/plugins/mini-css-extract-plugin/) which extracts CSS into separate files. It creates a CSS file per JS file which contains CSS.
  - `npm install --save-dev mini-css-extract-plugin`
  - We are going to add a function to get the plugins depending on the environment:
```js
const getPlugins = () => {
    const commonPlugins = [
        // The HtmlWebpackPlugin allows us to use a template for the index.html page
        // and automatically injects <script> or <link> tags for generated bundles.
        new HtmlWebpackPlugin({
            filename: 'index.html',
            template: path.join(__dirname, CONFIG.indexHtmlTemplate)
        }),
        // Copies static assets to output directory
        new CopyPlugin({
            patterns: [
                // by default copies to output folder
                { from: path.join(__dirname, CONFIG.assetsDir) }
            ],
        }),
    ];
    return isProduction ?
        [
            ...commonPlugins,

            // https://webpack.js.org/plugins/mini-css-extract-plugin/
            // extracts CSS into separate files. It creates a CSS file per JS file which contains CSS.
            new MiniCssExtractPlugin({ filename: 'style.[name].[fullhash].css' }),
        ] :
        commonPlugins;
}

module.exports = {
    plugins: getPlugins(),
}
```
  - Finally we need to update our rule loaders:
```js
{
    // The test property identifies which file or files should be transformed.
    test: /\.(sass|scss|css)$/,
    // The use property indicates which loader should be used to do the transforming.
    use: [
        isProduction ? MiniCssExtractPlugin.loader : 'style-loader',
        {
            loader: 'css-loader',
            options: isProduction ? {} : { sourceMap: true, },
        },
        {
            loader: 'sass-loader',
            options: isProduction ? {} : { sourceMap: true, },
        }
    ]
}
```
- Add optimization to split into chunks, this will prevent having just one big file.
```js
module.exports = {
    optimization: {
        splitChunks: {
            chunks: 'all'
        },
    },
}
```
- Also we are going to add symlinks false to prevent a previously reported issue with NuGet (don't forget to add this to webpack.tests.config.js as well):
```js
resolve: {
    // See https://github.com/fable-compiler/Fable/issues/1490
    symlinks: false
},
```
- As a bonus we are going to add another loader, the [file-loader](https://webpack-v3.jsx.app/loaders/file-loader/) which will resolve imported files into a url and will include the files in the output directory.
  - `npm install file-loader --save-dev`
  - Add it to the rules:
```js
{
    test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot)(\?.*)?$/,
    use: ['file-loader']
}
```
  - Now similar to how we import styles you could import a file into your fs Client files.
```f#
Fable.Core.JsInterop.importAll "./file.png"
```
We are done with the webpack.config.js file however we still need to set the `process.env.NODE_ENV` so it recognizes our environment, for this we are going to update our package.json scripts and pass the `--node-env` parameter.
```js
{
    "scripts": {
        "build": "webpack --node-env=development",
            "build:prod": "webpack --node-env=production",
            "build:tests": "webpack --config webpack.tests.config.js --node-env=development",
            "start": "webpack-dev-server --node-env=development",
            "start:tests": "webpack-dev-server --config webpack.tests.config.js --node-env=development"
    },
}
```
Since our build project uses `npm run` we don't need to make updates to the existing commands however we need a new Bundle task:
```f#
Target.create "Bundle" (fun _ ->
    [ "server", dotnet $"publish -c Release -o \"{deployPath}\"" serverPath
      "client", dotnet "fable --outDir output --sourceMaps --run npm run build:prod --prefix ../.." clientPath ]
    |> runParallel
)
```
If you run the `dotnet run Bundle` command you will see our deployment build generated in the deployPath folder!
However we are going to add a dependency so Bundle depends on the Clean task
```f#
"Clean"
    ==> "Bundle"
```
And we don't want to commit the generated files to our repo so don't forget to add `deploy/` to the .gitignore file.

#<h1 id="feliz-bulma">Feliz.Bulma</h1>

Now we are going to add [Feliz.Bulma](https://dzoukr.github.io/Feliz.Bulma/#/) and we are going to improve our UI.
- Install Feliz.Bulma with femto: `dotnet femto install Feliz.Bulma`
- Import the bulma styles
  - One option is to just import it inside the index.scss file
```scss
@import "~bulma";
```
  - Other option is to add the link to the cnd in the index.html page:
```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bulma@0.9.3/css/bulma.min.css">
```
  - For this project I'm going to stick with the first option.
Now we can start using feliz:
```f#
open Feliz
open Feliz.Bulma

let view model dispatch =
    Bulma.hero [
        hero.isFullHeight
        color.isPrimary
        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]
        prop.children [
            Bulma.heroBody [
                Bulma.container [
                    Bulma.column [
                        column.is6
                        column.isOffset3
                        prop.children [
                            Bulma.title [
                                text.hasTextCentered
                                prop.text (match model.Greet with Some msg -> msg | _ -> "Loading...")
                            ]
                            Html.div [
                                Html.button [ prop.onClick (fun _ -> dispatch Increment)
                                              prop.text "Increment" ]

                                Html.button [ prop.onClick (fun _ -> dispatch Decrement)
                                              prop.text "Decrement" ]

                                Html.h1 model.x ]
                        ]
                    ]
                ]
            ]
        ]
    ]
```

## Bonus 1
Since we are building the SAFE template now we are ready to implement a simple Todos app like the template, but I'll leave this as an extra exercise.

## Bonus 2
Instead of using a type Program to in the Server that we use to create our WebApplicationFactory for our integration tests, we are going to use a generic type.
This way our Server.js will look cleaner:
```f#
// Server Type is only used in our unit tests to identify this assembly and create a WebApplicationFactory.
type Server = class end

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
```
Now we need to update our Server.Tests.fs WebApplicationFactory:
```f#
open System
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
/// .Net core utility for integration testing https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0#customize-webapplicationfactory
type ServerAppFactory<'T when 'T : not struct> () =
    inherit WebApplicationFactory<'T>()
    /// override the CreateHostBuilder method and return the Saturn application
    override _.CreateHostBuilder () = app
    /// override ConfigureWebHost to customize the Factory https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0#customize-webapplicationfactory
    override _.ConfigureWebHost builder =
        let configureServices (services : IServiceCollection) = ()
        builder
            .UseEnvironment("Test")
            .ConfigureServices(Action<IServiceCollection> configureServices)
        |> ignore

// We don't have a type for the Program, that's why we added the Server Type.
// ServerAppFactory only requires the generic parameter to be defined in the Server Assembly so this works as expected.
let server = (new ServerAppFactory<Server>()).Server
```

#<h1 id="publish">Publish the Application</h1>

We have a working application, so it's time to publish it!
We are going to publish the application to Azure Apps using [Farmer](https://compositionalit.github.io/farmer/)
- Start by adding Farmer to the build project: `dotnet add package Farmer`
- Add a Target task in the Build.fs file called Azure:
```f#
open Farmer
open Farmer.Builders

Target.create "Azure" (fun _ ->
    let web = webApp {
        name "SafeHello" // the name of the app
        zip_deploy "deploy" // deploy the deploy folder as zip
    }
    let deployment = arm {
        location Location.WestEurope // location of the resource
        add_resource web
    }

    // generate json file
    deployment
    |> Writer.quickWrite "SafeHello"
)
```
-  Update the dependencies and add one entry for the tasks that need to run before the Azure task:
```f#
let dependencies = [
    "Clean"
        ==> "InstallClient"
        ==> "Bundle"
        ==> "Azure"
]
```
- Run the task `dotnet run Azure`
- The application was not published but a SafeHello.json file was generated, this is because we specified that:
```f#
deployment |> Writer.quickWrite "SafeHello"
```
- To actually deploy to azure you need to change that line for:
```f#
deployment
|> Deploy.execute "SafeHello" Deploy.NoParameters
|> ignore
```
- Note that you need to install and be logged into the [azure cli](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?tabs=azure-cli), for more information in how to complete the deploy review the official [Farmer](https://compositionalit.github.io/farmer/quickstarts/quickstart-3/) docs.

#<h1 id="paket">Paket Optional</h1>

There is a debate between using [Paket](https://fsprojects.github.io/Paket/index.html) or [Nuget](https://docs.microsoft.com/en-us/nuget/what-is-nuget) to manage dependencies for this reason I leave it up to you to implement the following steps to add Paket:
- Install paket: `dotnet tool install paket`
- Convert the solution to paket: `dotnet paket convert-from-nuget`
- Update your .gitignore:
```gitignore
# Paket
packages/
paket-files/
```
That's it! now you can try to run your tests `dotnet run RunTests` or run the app `dotnet run`
If you want to review this step checkout the paket branch.

# Todos
- Create a dotnet template based on this project. 
