open Fake.Core
open Fake.IO

open Helpers

initializeContext ()

let sharedPath = Path.getFullName "src/Shared"
let serverPath = Path.getFullName "src/Server"
let clientPath = Path.getFullName "src/Client"
let deployPath = Path.getFullName "deploy"
let sharedTestsPath = Path.getFullName "tests/Shared"
let serverTestsPath = Path.getFullName "tests/Server"
let clientTestsPath = Path.getFullName "tests/Client"

Target.create "Clean" (fun _ ->
    // clean projects
    [ "server", dotnet "clean" serverPath
      "client", dotnet "clean" clientPath
      "shared", dotnet "clean" sharedPath]
    |> runParallel

    // Delete *.fs.js files created by Fable
    run dotnet "fable clean --yes" clientPath

    // clean deploy
    Shell.cleanDir deployPath
    )

Target.create "CleanAll" (fun _ ->
    // clean fs projects
    [ ("server", serverPath)
      ("client", clientPath)
      ("shared", sharedPath)
      ("server:test", serverTestsPath)
      ("client:test", clientTestsPath)
      ("shared:test", sharedTestsPath)
      ]
    |> List.map (fun (name, path) -> name, dotnet "clean" path)
    |> runParallel

    // Delete *.fs.js files created by Fable
    [ "client", dotnet "fable clean --yes" clientPath
      "client:test", dotnet "fable clean --yes" clientTestsPath ]
    |> runParallel

    // clean deploy
    Shell.cleanDir deployPath
)

Target.create "InstallClient" (fun _ -> run npm "install" ".")

Target.create "Run" (fun _ ->
    run dotnet "build" sharedPath
    [ "server", dotnet "watch run" serverPath
      "client", dotnet "fable watch -o output -s --run npm run start --prefix ../.." clientPath ]
    |> runParallel
)

Target.create "Format" (fun _ -> run dotnet "fantomas . -r" "src")

Target.create "RunTests" (fun _ ->
    run dotnet "build" sharedTestsPath
    [ "server", dotnet "watch run" serverTestsPath
      "client", dotnet "fable watch --outDir output --sourceMaps --run npm run start:tests --prefix ../.." clientTestsPath ]
    |> runParallel
)

// Define dependencies
open Fake.Core.TargetOperators
let dependencies = [
    "Clean"
        ==> "InstallClient"
        ==> "Run"

    "InstallClient"
        ==> "RunTests"
]

// The entry point allows us to run any task defined as a Target with the help of dotnet.
// Ex. dotnet run Clean
// By default it executes the Run Target.
[<EntryPoint>]
let main args = runOrDefault args
