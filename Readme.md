# SAFE template from scratch
In this guide we'll start from a blank project and start adding functionality so we can get a template similar to the [SAFE Template](https://safe-stack.github.io/docs/quickstart/), this way you will understand the role of every file and dependency.
You can follow this guide from top to bottom or you can review it with the git history as every step corresponds to a commit that has the described changes.

Note that there may be some small differences and bonuses that I added, however after reading and understanding the template I encourage you to use/contribute to the [official template](https://github.com/SAFE-Stack/SAFE-template).

Disclaimer: The main focus of this tutorial is to be easy to follow and to have the commits as guidelines. For this reason I'm going to squash commits and rearrange the history to remove noise and to keep it clean for new readers. If you are a contributor I really appreciate your help and I apologize in advance for the problems this may cause you.  

# List of contents
- [1. Create the solution and projects](#solution)
- [2. Saturn](#saturn)
- [3. Server Unit tests](#server-unit-tests)
- [4. Server Integration Tests Bonus](#server-integration-tests)
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
  - 16.6 Run fantomas
- [17. Client-Server Communication](#client-server-communication)
- [18. Prod Bundle](#prod-bundle)
- [19. Feliz.Bulma](#feliz-bulma)
  - 19.1 Replicate todo app from SAFE template
  - 19.2 Alternative way to setup integration tests
- [20. Publish the application](#publish)
- [21. Paket](#paket)
- [22. Bonus](#bonus-content)
  - 22.1 Warning as Error 
  - 22.2 Server Configuration
  - 22.3 Webpack Refactoring 2
  - 22.4 Webpack Refactoring 3
  - 22.5 Build Soft Dependencies
  - 22.6 Fantomas Settings
- [23. Final Thoughts](#final-thoughts)

# Pre-requisites
You need to install the following things before proceeding:
1. .NET Core SDK (For this tutorial I used [.Net Core 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0))
2. [Node LTS](https://nodejs.org/en/)

# Test the repo in your machine
I highly recommend you to clone the repository, this way you will be able to review each commit and the changes that are involved.
1. Clone the repo `git clone https://github.com/tico321/SAFEFromScratch.git`
2. Test that it works:
  - In the root folder run `dotnet tool restore` (You will learn about these tools when you get to the [5. Fable](#fable) section)
  - Now in the same folder run `dotnet run` (You will learn how we run multiple tasks when you get to the [10. Fake Build](#fake-build) section) 
  - The previous command runs the Client and the Server, so you can navigate to [http://localhost:8080](http://localhost:8080) in your browser to test that everything is working.

<h1 id="solution">Create the solution and projects</h1>

First we are going to create the solution and the main projects.
  - Start by creating an empty folder for your project, I'm going to refer to this folder as the root folder, and then add the following items to this folder:
  - Create the solution: $ `dotnet new sln --name SafeFromScratch`
  - src folder: $ `mkdir src`
  - Client Project: 
    - create project (by default the folder name is used): `dotnet new console --output src/Client -lang F#`
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
      - Create a virtual directory to easily navigate from an IDE: Add -> New Solution Folder -> `Solution Items`
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

<h1 id="saturn">Saturn</h1>

Add [Saturn](https://saturnframework.org/) package to the server project. 
  - Move to the server project and add the package. (You can use an IDE if it's simpler for you)
    - $ `cd src/Server`
    - `dotnet add package Saturn`

Edit src/Server/Program.fs with a minimal implementation that exposes an endpoint.
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
Now you can run the server by executing `dotnet watch run` from src/Server, and test the endpoint we just added.

Saturn is build on top of [Giraffe](https://github.com/giraffe-fsharp/Giraffe), so alternatively you can work with Giraffe.

<h1 id="server-unit-tests">Server Unit Tests</h1>

First create the project:
  - At the root of the repo run: `dotnet new xunit --output tests/Server -lang F# --name Server.Test`
  - Add the project to the solution: `dotnet sln SafeFromScratch.sln add tests/Server/Server.Test.fsproj`
  - Now you can run the tests:
    - move to the tests project folder: `cd tests/Server`
    - run the tests: `dotnet test`
  - To reference the server project run the following command from the tests/Server folder: `dotnet add reference ..\..\src\Server\Server.fsproj`

<h1 id="server-integration-tests">Server Integration Tests Bonus</h1>

Optionally you can add integration tests to the server.
  - Integration testing with [WebApplicationFactory](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0#test-app-prerequisites), some examples:
    - [dotnet-minimal-api-integration-testing](https://github.com/martincostello/dotnet-minimal-api-integration-testing)
    - [Clean Architecture](https://github.com/jasontaylordev/CleanArchitecture#:~:text=The%20easiest%20way%20to%20get%20started%20is%20to,the%20back%20end%20%28ASP.NET%20Core%20Web%20API%29%20) 
  - Navigate to the test project `cd tests/Server`
  - Add the dotnet testing package: `dotnet add package Microsoft.AspNetCore.Mvc.Testing`
  - Create a Program inside src/Server/Program.fs to be the entry point:
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
Now you can create a WebApplicationFactory inside tests/Server that you can use in your unit tests.
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

<h1 id="fable">Fable</h1>

First we need to install the fable compiler. 

The Fable compiler job is to convert your .fs files into javascript files.

In the root folder of the project follow this steps to add the dotnet fable compiler tool:
  - We didn't create a tool manifest so we need to create one first: `dotnet new tool-manifest`
  - Now that we have a manifest we can start adding tools, starting with fable: `dotnet tool install fable`
  - optionally you can add the .config/dotnet-tools.json file to the `Solution Items` virtual folder of the solution.
  - Now you can run `dotnet fable` to compile the client project

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

Now from the root folder you can run `dotnet fable watch src/Client` which will transpile your .fs files to JS.
  - After you run the command notice how the Program.fs now has a Program.fs.js generated by Fable.

Now we need a simple html entry point, for this create a `src/Client/index.html` file and import the generated JS file that will be generated by Fable.

src/Client/index.html
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
src/Client/Program.fs
```f#
module App
open Browser.Dom
let helloH2 = document.querySelector("#hello") :?> Browser.Types.HTMLDivElement
helloH2.innerText <- "Welcome to Fable!!"
```
Note that if you open index.html in your browser you will get an error because fable transpiles code using features that are not supported by all browsers (In this case `module imports` and `module exports`)

<h1 id="webpack-bundle">Create a bundle with Webpack</h1>

Use [webpack](https://webpack.js.org/guides/installation/) module bundler to create a bundle.js file and solve the module issues we have seen above in the generated js file.
  - In the root folder of the project run: `npm init` to create the package.json file
    - remember to add "private": true, to make it explicit that this package won't be published. 
  - Let's add webpack with `npm install webpack webpack-cli --save-dev`
    - [webpack-cli](https://webpack.js.org/api/cli/) is used to run webpack using npx or as an npm command.
  - Add `node_modules/` to the .gitignore file, all the npm packages are going to be installed in this folder. 
  - Manually create a `webpack.config.js` file or run `npx webpack init`. 
    - webpack.config.js is the default file that webpack looks for.
  - Now you can run `npx webpack` and our bundle.js file will be generated.
  - We are going to send the bundle to a dist folder in the client so add the `dist/` folder to .gitignore.
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

<h1 id="webpack-plugins">Webpack plugins</h1>

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

<h1 id="loading-styles">Loading Styles</h1>

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
                // Creates `style` nodes from JS strings
                "style-loader",
                // Translates CSS into CommonJS
                "css-loader",
                // Compiles Sass to CSS
                "sass-loader",
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
    // Creates `style` nodes from JS strings
    "style-loader",
    // Translates CSS into CommonJS
    {
        loader: "css-loader",
        options: { sourceMap: true }
    },
    // Compiles Sass to CSS
    {
        loader: "sass-loader",
        options: { sourceMap: true }
    },
]
```

<h1 id="hot-reload">Hot Reload</h1>

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

<h1 id="fake-build">Fake Build</h1>

We still need to start the server, the client and fable. So it's time to simplify this process with the help of [fake](https://fake.build/).

Note: This is one of my favorite tools as it will speed up your development process by automating some tasks that you do regularly. Be sure to pay attention so you can keep adding tasks that make your life easier.

Although we could use the fake-cli along with a .fsx script we are going to use the same approach as the SAFE template and create a build project.
  - Create a Build project in the root folder of the repo.
    - `dotnet new console -lang F# --name Build --output ./`
    - Note the parameter `--output ./` this is very important since we want the .fsproj file to be in the root folder so we can call commands with dotnet run directly.
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
if we execute `dotnet run` now we will see `-- run --` printed in the console. 

Now we are ready to add some real tasks (please read the comments and follow the code, I believe it's self explanatory)
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
Now we can just use `dotnet run Clean` to clean our projects or `dotnet run` to run the Client and the Server in parallel with hot reload and everything else in place.
Additionally we can add dependencies like:
```f#
// Define dependencies
open Fake.Core.TargetOperators
let dependencies = [
    "Clean" // Clean has no dependencies at the moment
        ==> "Run" // Run depends on Clean so Clean will run first every time we call Run 
]
```

<h1 id="fantomas">Fantomas</h1>

[Fantomas](https://github.com/fsprojects/fantomas) is a tool to format the src code.
  - To add the tool run the following command in the root folder: dotnet tool install fantomas-tool
  - Add a new target task on fake to easily run the tool and format your code
```f#
Target.create "Format" (fun _ ->
    createProcess "dotnet" "fantomas . -r" "./src" |> runProcess |> ignore
)
```
  - Try it with `dotnet run Format`

<h1 id="elmish">Elmish</h1>

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

<h1 id="debugging">Source maps and debugging</h1>

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

<h1 id="client-unit-tests">Client Unit Tests</h1>

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

<h1 id="expecto">Expecto</h1>

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
Now it's time to update our unit tests and use Expecto instead of XUnit
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

<h1 id="clean-the-project">Clean the project</h1>

We are in really good shape now, we finished setting up the client and the server and we are ready to start adding some features.
However we added a bunch of small changes and we need to clean the project a little bit so it's more maintainable.

## Webpack
Let's start by cleaning our webpack configuration, for this we are just going to take the hardcoded paths and take them to a CONFIG object.
So at the beginning of our ```./webpack.config.js``` file, we add the lines below.
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
Our code looks pretty good now, but let's run fantomas to make sure is well formatted: `dotnet run Format`

<h1 id="client-server-communication">Client-Server Communication</h1>

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

<h1 id="prod-bundle">Prod Bundle</h1>

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

<h1 id="feliz-bulma">Feliz.Bulma</h1>

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
Instead of using a type Program in the Server to create our WebApplicationFactory for our integration tests, we are going to use a generic type.
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

<h1 id="publish">Publish the Application</h1>

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

<h1 id="paket">Paket</h1>

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

<h1 id="bonus-content">Bonus</h1>

## Warnings as Errors

What a best way to keep your project in shape that preventing warnings from taking over your project.
For this we just need to add the flag `TreatWarningsAsErrors` to all of our .fsproj projects.

Server.fsproj example:
```xml
<PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <!-- https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#treatwarningsaserrors -->
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```
This will force you to write better code and for example handle all scenarios for a discriminated union when using pattern matching.

## Server Configuration

### launchSettings.json
Since Saturn is build on top of ASP.NET Core adding the launchSettings file is quite straight forward. Our main motivation to set this file is to run the server in debug mode from and IDE. 

For those of you that are not familiar with the ASP.NET Core Web API template, by default the template creates a folder called `Properties` with a file called [launchSettings.json](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-6.0#development-and-launchsettingsjson). This file allows you to set **environment variables** that you can read and use for your configuration. Note that this file is only used for local development and you should set real environment variables in your prod environment. 
- Create the file: `src/Server/Properties/launchSettings.json`
- Now we are going to add the minimal configuration that will be used when we run the project as a self hosted application:
  - By self hosted I mean that the application is hosted in kestrel and runs as an executable, this is very common specially for dockerized applications. But we could as well host it on [IIS](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-6.0) or IIS Express, or other dedicated web servers.
```json
{
  "profiles": {
    "SelfHostedServer": {
      "commandName": "Project",
      "launchBrowser": false,
      "applicationUrl": "http://localhost:8085",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```
- `dotnet run` will use this profile by default, but in case you add more profiles you will need to specify which one you want to use for example `dotnet run --launch-profile "SelfHostedServer"`.
- Note that we are passing one environment variable `ASPNETCORE_ENVIRONMENT`, this one is a default environment variable that is used by ASP.NET Core, but you can pass ass many variables ass necessary.
  - As a quick tip if you have an environment variable that you want to override like `Logging.LogLevel.Microsoft` you need to replace the dots with two underscores `Logging__LogLevel__Microsoft: Warning` ;)
- We are defining the applicationUrl in this configuration so we can remove it from our application builder.
```f#
let app =
    application {
        // you can remove this line
        url "http://localhost:8085"
        (* ... *)
    }
```
- Run the app `dotnet run` and check that the server still runs in the port 8085 except that now the url is not hardcoded.
- This configuration only sets a profile for a self-hosted application, but if we would like to host the app in IIS or IIS express we would need some additional configuration. For completeness I'm going to add this configurations which are added by default in the ASP.Core Web API template.
```f#
{
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:8085",
      "sslPort": 44333
    }
  },
  "profiles": {
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": false,
      "launchUrl": "http://localhost:8080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "SelfHostedServer": {
      "commandName": "Project",
      "launchBrowser": false,
      "launchUrl": "http://localhost:8080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": " http://localhost:8085"
    }
  }
}
```
### application.json
By default ASP.Core with the default configuration will look for the appsettings.json file.
So how exactly that this works? Well it works in layers:

1. appsettings.json file is loaded.
2. appsettings.[ASPNETCORE_ENVIRONMENT].json is loaded. ASPNETCORE_ENVIRONMENT is read from the environment variables so you can create more files and chose which one to use by modifying this environment variable.
3. Environment variables are loaded.

If configuration values collide the last configuration loaded will win.

To see it in action we are going to follow these steps:
- Create the src/Server/appsettings.json file:
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=SafeFromScratch;Trusted_Connection=True"
  }
}
```
- Create the src/Server/appsettings.prod.json file:
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=SafeFromScratchProd;Trusted_Connection=True"
  }
}
```
- Now for the application to read the configuration we need this values to be present, so we need to make sure they are copied when we build the project.
  - You can either do it from an IDE: `right click -> properties -> copy if newer`
  - Or you can add the following ItemGroup to the Server.fsproj file:
```xml
  <ItemGroup>
    <Content Include="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Include="appsettings.prod.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
```
- That's it, now you can try it by adding an endpoint to read the configuration:
```f#
open Giraffe
let appRouter = router {
    get "/config" (fun next ctx ->
        let config = ctx.GetService<IConfiguration>()
        let config =
            sprintf "appsettings.json connection string: %s\nlaunchSettings environment: %s"
                config["ConnectionStrings:Default"]
                config["ASPNETCORE_ENVIRONMENT"]
        text config next ctx
        )
    forward "" todoApi
}
```
- now navigate to http://localhost:8085/config and you will see your configuration
- you can also try to update the launchSettings.json ASPNETCORE_ENVIRONMENT value to prod and you will see how it overrides the default value of appsettings.json: `"ASPNETCORE_ENVIRONMENT": "prod"`

### Webpack Refactoring 2
We are in good shape, however there is still one piece that we could improve. Currently our webpack.tests.config.js is configured only for development and although this is what we want in our day to day as developers, we may want to also have a production configuration that will allow us to run our unit tests with the same code that will be deployed to production. Usually this is done automatically by CI tool.

The approach we'll take is really simple, we realized we need the same configuration that we have for production inside webpack.config.js. We could easily just copy paste the code from webpack.config.js to webpack.tests.config.js but we'll take it one step further and we are going to centralize the configuration so we only have to update it in one place.

- Create a `webpack.common.js` file next to the other webpack config files. (you can also add it to the `Solution Items` virtual folder)
- The only real difference between `webpack.config.js` and `webpack.tests.config.js` is the configuration which we already moved to an object CONFIG, so we can easily create a function that will return the webpack configuration based on the CONFIG parameter. So you just need to create this function inside webpack.common.js with the contents of `webpack.config.js`:
```js

const getConfig = (CONFIG) => {
    return {
        mode: environment,
        entry: CONFIG.fsharpEntry,
        // etc.
        
        /* rest of the configuration ommited for brevity */
    };
}

module.exports.getConfig = getConfig; // we export the function
```
- Now we can use it in both `webpack.config.js` and `webpack.tests.config.js` so you should have something like this:
```f#
// webpack.tests.config.js
const common = require("./webpack.common");

const CONFIG = {
    // The tags to include the generated JS and CSS will be automatically injected in the HTML template
    // See https://github.com/jantimon/html-webpack-plugin
    indexHtmlTemplate: './tests/Client/index.html',
    fsharpEntry: "./tests/Client/output/App.Test.js",
    outputDir: "./tests/Client/dist",
    assetsDir: "./src/Client/public",
    devServerPort: 8080,
    // When using webpack-dev-server, you may need to redirect some calls
    // to a external API server. See https://webpack.js.org/configuration/dev-server/#devserver-proxy
    devServerProxy: {}
}

module.exports = common.getConfig(CONFIG);
```
- Note that I had to update assetsDir to `assetsDir: "./src/Client/public"`, previously we were not worried about copying our assets but since we want the test configuration to be as close as possible to the real implementation we are going to do it now.
- Use the getConfig function in `webpack.config.js` and you should be ready to try it our with `dotnet run RunTests` and `dotnet run`.

### Webpack Refactoring 3
I created a PR to include the previous changes in the official Safe-Template repository and some feedback came which I'm adding in this bonus.
The thing is we can simplify the webpack.config file even more and the steps are quite similar to the previous bonus, remember we created a `getConfig` method? well we are going to move it back to webpack.config.js

I know sorry for making you work double, but there are some tricks that we are going to use to make it work with just the webpack.config.js file and get rid of the webpack.test.config.js file and the webpack.common.js file. (if you have more complex workflows you can always add them back)

1. Move both CONFIG configurations to the webpack.config.js file:
```js
const CONFIG = {
    /* config content remains the same */
};

const TEST_CONFIG = {
    /* test config content remains the same */
};
```
2. We are going to export a [webpack function](https://webpack.js.org/configuration/configuration-types/#exporting-a-function) instead of an object, the content of the function will be practically or at least quite similar to the one that we created in the previous step, so you can just move it back. Now I'm going to focus in the main differences that you need to do:
```js
module.exports = function(env, arg) {
    // Mode is passed as a flag to npm run, we pass this flag in the package.json scripts. Ex. --mode development
    // see the docs for more details on flags https://webpack.js.org/api/cli/#flags
    const mode = arg.mode ?? 'development';
    // Environment variables can also be defined as args for package.json scripts. Ex. --env test
    // docs: https://webpack.js.org/api/cli/#environment-options
    const config = env.test ? TEST_CONFIG : CONFIG;
    const isProduction = mode === 'production';

    console.log(`Bundling for ${env.test ? 'test' : 'run'} - ${mode} ...`);

    return {
        mode: mode,
        /* The rest remains unchanged except that you need to replace CONFIG with config which will be CONIF or TEST_CONFIG depending on the environment */
    };
}
```
3. First let's focus on the mode `const mode = arg.mode ?? 'development';` now we are retrieving it from the arg passed to the function.
4. The config is chosen from the env variable passed to the function `const config = env.test ? TEST_CONFIG : CONFIG;`.
5. For this changes to work we also need to update our package.json scripts and pass this arguments accordingly:
```js
{
    "scripts": {
        "build": "webpack --mode development",
        "build:prod": "webpack --mode production",
        "build:tests": "webpack --mode development --env test",
        "start": "webpack-dev-server --mode development",
        "start:tests": "webpack-dev-server --mode development --env test"
    }
}
```
That's it! We have an even simpler webpack.config.js
If you are reviewing my commit you will notice an additional change to the plugins section of the config:
```js
{
    plugins: [
        // ONLY PRODUCTION
        // isProduction && SomePlugin returns false when it's not production and all false elements are filtered with .filter(Boolean)
        // MiniCssExtractPlugin: Extracts CSS from bundle to a different file
        // To minify CSS, see https://github.com/webpack-contrib/mini-css-extract-plugin#minimizing-for-production
        isProduction && new MiniCssExtractPlugin({filename: 'style.[name].[contenthash].css'}),
        // CopyWebpackPlugin: Copies static assets to output directory
        isProduction && new CopyWebpackPlugin({patterns: [{from: resolve(config.assetsDir)}]}),

        // PRODUCTION AND DEVELOPMENT
        // HtmlWebpackPlugin allows us to use a template for the index.html page
        // and automatically injects <script> or <link> tags for generated bundles.
        new HtmlWebpackPlugin({filename: 'index.html', template: resolve(config.indexHtmlTemplate)})
    ].filter(Boolean),
}
```
This trick came in the code review from the PR I opened. Basically we take advantage of JS syntax. 
When you have a boolean expression with `&&` if the left side is a truthy value it will be returned otherwise the left value will be returned.
Note that I'm referring to the left and right side of the expression as values, that's right JS won't return true or false but the values on each side of the expression.

For example `isProduction && new CopyWebpackPlugin({patterns: [{from: resolve(config.assetsDir)}]}),` if `isProduction` is false it will return `false`, but if it's true it will return the `CopyWebpackPlugin`.
Finally in the last line we filter the false items in the array with `.filter(Boolean)` leaving only the plugins.

### Build Soft Dependencies

I updated the build tasks to use the `==>` for dependencies that I want to run always and `?=>` for [soft dependencies](https://fake.build/legacy-core-targets.html#Soft-dependencies).
for example:
```f#
let dependencies = [
    "TaskB"
        ==> "TaskA"

    "TaskB"
        ==> "TaskA"
        ==> "TaskC"
]
```
In this case `TaskA` depends on `TaskB` so `TaskB` will always run before `TaskA` thus we can simplify the dependencies:
```f#
let dependencies = [
    "TaskB"
        ==> "TaskA"

    "TaskA" // TaskA already depends on TaskB so it will run before TaskA
        ==> "TaskC" 
]
```
Now there are cases were we want a `soft dependency` for example:
```f#
let dependencies = [
    // TaskC depends on both TaskA and TaskB
    "TaskB" ==> "TaskC"
    "TaskA" ==> "TaskC"
    // However we define that if TaskA and TaskB need to run TaskA will run before TaskB
    // Otherwise you can run TaskA and TaskB independently as they don't depend on each other.
    "TaskA" ?=> "TaskB"
        
    // this is a soft dependency defined with ?=>
    "TaskB" ?=> "TaskA"
    
]
```
The build dependencies on `./build.fs` ended up looking like this:
```f#
let dependencies = [
    // docs https://fake.build/legacy-core-targets.html#Visualising-target-dependencies
    // A ==> B Defines a dependency, so every time we run B, A will run first.
    //         Fake builds a graph will all the dependencies.
    // A ?=> B is a soft dependency which means, A is not required before B but if both need to run A will run first.

    "InstallClient" ==> "Run"

    "InstallClient" ==> "RunTests"

    "Clean" ==> "Bundle"
    "InstallClient" ==> "Bundle"
    "Clean" ?=> "InstallClient"

    "CleanAll" ==> "CleanAndRun"
    "InstallClient" ==> "CleanAndRun"
    "CleanAll" ?=> "InstallClient"

    "Bundle" ==> "Azure"
]
```
I also created `CleanAndRun` as I found that I don't need to clean the project very often and I use just `Run` most of the times. 

### Fantomas Settings
If you started using [fantomas](https://github.com/fsprojects/fantomas/blob/master/docs/Documentation.md#using-the-command-line-tool) to format your code you may have noticed that the client is formatted to the right. 
You may be Ok with this style of coding and that's fine but I don't like it very much, so I decided to add a `.editorconfig` file.

[editorconfig](https://editorconfig.org/) Is intended to be a standard way to define your coding style in the project so all developers use the same coding style. It supports f# files, c#, files, js files and others.
A nice feature of using a `.editorconfig` is that IDEs take advantage of it and will use it to format your code as well.  

These are the steps to integrate fantomas with your settings:
1. Create a `.editorconfig` file in the root folder of the repository. (This way the styles will be applied to the whole solution)
2. Copy the fantomas default `.editorconfig` settings from [here](https://github.com/fsprojects/fantomas/blob/master/docs/Documentation.md#configuration).
```gitignore
# It looks something like this:
[*.fs]
indent_size=4
max_line_length=120
# and many more configurations
```
3. Update the [fsharp_single_argument_web_mode](https://github.com/fsprojects/fantomas/blob/master/docs/Documentation.md#fsharp_single_argument_web_mode) to true, this is the one that makes the Client look weird.
```gitignore
fsharp_single_argument_web_mode=true
```
4. Since fantomas tries to format recursively all the .fs files, it also tries to format the .fs files that come from the nuget Client dependencies, for this reason we are going to ignore the `output` folder located in the Client and Client.Test projects. 
For this add a [.fantomasignore](https://github.com/fsprojects/fantomas/blob/master/docs/Documentation.md#ignore-files-fantomasignore) file in the root folder of the repo with the following content (It uses the same syntax as .gitignore):
```gitignore
# Ignore Fable output files
output/
```
5. Finally update the `Format` Fake task that we defined in `./Build.fs` to:
```f#
Target.create "Format" (fun _ ->
    [ "src", dotnet "fantomas ./src -r" "."
      "tests", dotnet "fantomas ./tests -r" "." ]
    |> runParallel
    )
```
The important thing to notice here is that I'm passing the folders I want fantomas to format as a parameter (`./src` and `./tests`) and I'm running the command from the root folder `.` otherwise fantomas won't pickup the files we added before.

Note that I found that fantomas was having some problems formatting the `src/Server/Server.fs` file, so I had to fix it manually, to save you a few minutes I had to update the `get "/config"` route to look like this:
```f#
get
    "/config"
    (fun next ctx ->
        let config = ctx.GetService<IConfiguration>()
        let conn = config.["ConnectionStrings:Default"]
        let env = config.["ASPNETCORE_ENVIRONMENT"]

        let config =
            $"appsettings.json connection string: {conn}\nlaunchSettings environment: {env}"

        text config next ctx)
```

<h1 id="final-thoughts">Final thoughts</h1>

Initially I thought on adding a step to make this project a template as well and show you how to do it, but I believe it goes out of the scope of this tutorial. Besides we already have the Official template so I encourage you to contribute to it.

I hope you liked the bonus parts, I feel they are very important in any project, but if you feel I missed something important that should be here let me know or create a PR :)

Congratulations now you know the purpose of every file in the template!!!
