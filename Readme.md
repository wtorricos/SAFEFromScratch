# SAFE template from scratch
In this guide we'll start from a blank project and start adding functionality so we can get a template similar to the [SAFE Template](https://safe-stack.github.io/docs/quickstart/), this way you will understand the role of every file and dependency.
You can follow this guide from top to bottom or you can review it with the git history as every step correspond to a commit that has the described changes.

Note that one difference with the SAFE template is that in this project we'll use .net6.0, along with the latest version of dotnet tools, npm packages and nuget packages while SAFE may not be in the latest version of some of them, for this reason some small differences may be seen.

# Create the solution and projects
First we are going to create the solution and the main projects.
  - Create the solution: $ `dotnet new sln --name SafeFromScratch`
  - src folder: $ `mkdir src`
  - Client Project: 
    - create project (by default the folder name is used): `dotnet new console --output src/Server -lang F#`
  - Server Project:
    - `dotnet new console --output src/Server -lang F#`
  - Shared Project:
    - `dotnet new console --output src/Shared -lang F#`
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
```
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
# Saturn
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

# Server Unit tests
First create the project:
  - At the root of the repo run: `dotnet new xunit --output tests/Server -lang F# --name Server.Test`
  - Add the project to the solution: `dotnet sln SafeFromScratch.sln add tests/Server/Server.Test.fsproj`
  - cd tests/Server
  - you can run tests with: `dotnet test`
  - Reference the server project: `dotnet add reference ..\..\src\Server\Server.fsproj`

# Server Integration Tests
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

# Fable
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

# Create a bundle with Webpack
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

# Webpack plugins
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

# Loading Styles
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

# Hot Reload
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

## Fake Build
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

# Fantomas
[Fantomas](https://github.com/fsprojects/fantomas) is a tool to format the src code.
  - To add the tool run the following command in the root folder: dotnet tool install fantomas-tool
  - Add a new target task on fake to easily run the tool and format your code
```f#
Target.create "Format" (fun _ ->
    createProcess "dotnet" "fantomas . -r" "./src" |> runProcess |> ignore
)
```
  - Try it with `dotnet run Format`

# Elmish
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

# Source maps and debugging
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

# Todos
- Add Client Unit tests.
- Modify the Server unit tests to use Expecto.
- Clean the project.
- Add Communication between the client and the server.
- Make Client and Server run Shared project tests.
- Add prod configuration to webpack and other missing steps.
- Add Targets in Fake to create a release version of the app.
- Refactor Fake (probably just copy the helpers from SAFE template)
- Add support to publish the project to Azure with Farmer.
- Add optional steps to migrate to paket instead of nuget. 
- Create a dotnet template based on this project. 
