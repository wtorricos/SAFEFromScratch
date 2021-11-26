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

// Define targets, we can run each target with dotnet run
// for example dotnet run Clean
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
       createProcess "dotnet" $"fable watch {clientPath} --sourceMaps --run webpack-dev-server" "." |]
    |> Array.Parallel.map runProcess
    |> ignore
    )

Target.create "Format" (fun _ ->
    createProcess "dotnet" "fantomas . -r" "./src" |> runProcess |> ignore
)

// Define dependencies
open Fake.Core.TargetOperators
let dependencies = [
    "Clean" // Clean has no dependencies at the moment
        ==> "Run" // Run depends on Clean so Clean will run first every time we call Run
]

// The entry point allows us to run any task defined as a Target with the help of dotnet.
// Ex. dotnet run Clean
// By default it executes the Run Target.
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
