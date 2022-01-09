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

Target.create "InstallClient" (fun _ -> run npm "install --from-lock-file" ".")

Target.create "Run" (fun _ ->
    run dotnet "build" sharedPath
    [ "server", dotnet "watch run" serverPath
      "client", dotnet "fable watch -o output -s --run npm run start --prefix ../.." clientPath ]
    |> runParallel
)

Target.create "CleanAndRun" (fun _ ->
    run dotnet "build" sharedPath
    [ "server", dotnet "watch run" serverPath
      "client", dotnet "fable watch -o output -s --run npm run start --prefix ../.." clientPath ]
    |> runParallel
)

Target.create "Format" (fun _ ->
    [ "src", dotnet "fantomas ./src -r" "."
      "tests", dotnet "fantomas ./tests -r" "." ]
    |> runParallel
    )

Target.create "RunTests" (fun _ ->
    run dotnet "build" sharedTestsPath
    [ "server", dotnet "watch run" serverTestsPath
      "client", dotnet "fable watch --outDir output --sourceMaps --run npm run start:tests --prefix ../.." clientTestsPath ]
    |> runParallel
)

Target.create "Bundle" (fun _ ->
    [ "server", dotnet $"publish -c Release -o \"{deployPath}\"" serverPath
      "client", dotnet "fable --outDir output --sourceMaps --run npm run build:prod --prefix ../.." clientPath ]
    |> runParallel
)

open Farmer
open Farmer.Builders
// In order to execute the Azure task you need to install and log into the azure cli https://docs.microsoft.com/en-us/cli/azure/get-started-with-azure-cli
// Find more about farmer here: https://compositionalit.github.io/farmer/quickstarts/quickstart-3/
Target.create "Azure" (fun _ ->
    let web = webApp {
        name "SafeHello"
        zip_deploy "deploy"
    }
    let deployment = arm {
        location Location.WestEurope
        add_resource web
    }

    deployment
    |> Deploy.execute "SafeHello" Deploy.NoParameters
    |> ignore
)

// Define dependencies
open Fake.Core.TargetOperators
let dependencies = [
    // docs https://fake.build/legacy-core-targets.html#Visualising-target-dependencies
    // A ==> B Defines a dependency, so every time we run B, A will run first.
    //         Fake builds a graph will all the dependencies.
    // A ?=> B is a soft dependency which means, A is not required before B but if both need to run A will run first.

    "InstallClient" ==> "Run"

    "InstallClient" ==> "RunTests"

    "Clean" ==> "Bundle"
    "InstallClient" ==> "Bundle"
    "Clean" ?=> "InstallClient" // make sure Clean runs before InstallClient

    "CleanAll" ==> "CleanAndRun"
    "InstallClient" ==> "CleanAndRun"
    "CleanAll" ?=> "InstallClient"

    "Bundle" ==> "Azure"
]

// The entry point allows us to run any task defined as a Target with the help of dotnet.
// Ex. dotnet run Clean
// By default it executes the Run Target.
[<EntryPoint>]
let main args = runOrDefault args
