module Helpers

open Fake.Core

// initialize context for Fake to run
let initializeContext () =
    let execContext = Context.FakeExecutionContext.Create false "build.fsx" [ ]
    Context.setExecutionContext (Context.RuntimeContext.Fake execContext)

module Proc =
    module Output =
        open System
        let locker = obj()

        let colors =
            [| ConsoleColor.Blue
               ConsoleColor.Yellow
               ConsoleColor.Magenta
               ConsoleColor.Cyan
               ConsoleColor.DarkBlue
               ConsoleColor.DarkYellow
               ConsoleColor.DarkMagenta
               ConsoleColor.DarkCyan |]

        /// Helper function to print to the console.
        /// color is the ConsoleColor that is going to be used
        /// colored is going to be printed using the color from the first argument
        /// line is going to printed normally
        let print color (colored: string) (line: string) =
            lock locker
                (fun () ->
                    let currentColor = Console.ForegroundColor
                    Console.ForegroundColor <- color
                    Console.Write colored
                    Console.ForegroundColor <- currentColor
                    Console.WriteLine line)

        /// Helper function that will be used to capture the standard output
        /// It takes an index that will be used to choose a color
        /// name will be printed colored
        /// line will be printed normally
        let onStdout index name (line: string) =
            let color = colors.[index % colors.Length]
            if isNull line then
                print color $"{name}: --- END ---" ""
            else if String.isNotNullOrEmpty line then
                print color $"{name}: " line

        /// Helper function that will be used to capture the standard errors
        /// it prints the name and the error with red color always
        let onStderr name (line: string) =
            let color = ConsoleColor.Red
            if isNull line |> not then
                print color $"{name}: " line

        // Redirect redirects the input/output events to onStdout and onStderr
        // index will be used to choose a color from the Color list.
        // name will be used as a prefix to the process output
        let redirect (index, (name, createProcess)) =
            createProcess
            |> CreateProcess.redirectOutputIfNotRedirected
            |> CreateProcess.withOutputEvents (onStdout index name) (onStderr name)

        /// Helper function that prints what process is starting
        let printStarting indexed =
            for (index, (name, c: CreateProcess<_>)) in indexed do
                let color = colors.[index % colors.Length]
                let wd =
                    c.WorkingDirectory
                    |> Option.defaultValue ""
                let exe = c.Command.Executable
                let args = c.Command.Arguments.ToStartInfo
                print color $"{name}: {wd}> {exe} {args}" ""

        /// Helper function that as the name suggest will pretty print the output
        /// of the commands passed as arrays
        let withPrettyPrint commands =
            commands
            |> Array.indexed
            |> fun x -> printStarting x; x
            |> Array.map redirect

    module Parallel =
        open Output

        /// Helper function to run commands in Parallel
        let run cs =
            cs
            |> Seq.toArray
            |> withPrettyPrint
            |> Array.Parallel.map Proc.run

let createProcess exe arg dir =
    CreateProcess.fromRawCommandLine exe arg
    |> CreateProcess.withWorkingDirectory dir
    |> CreateProcess.ensureExitCode

let dotnet = createProcess "dotnet"
let npm =
    let npmPath =
        match ProcessUtils.tryFindFileOnPath "npm" with
        | Some path -> path
        | None ->
            "npm was not found in path. Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
            |> failwith

    createProcess npmPath

/// helper function to run terminal commands
let run proc arg dir =
    proc arg dir
    |> Proc.run
    |> ignore

/// helper function to run terminal commands in parallel
let runParallel processes =
    processes
    |> Proc.Parallel.run
    |> ignore

let runOrDefault args =
    try
        match args with
        | [| target |] -> Target.runOrDefault target
        | _ -> Target.runOrDefault "Run"
        0
    with e ->
        printfn "%A" e
        1
