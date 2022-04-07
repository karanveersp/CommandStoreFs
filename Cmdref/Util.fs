module Util

open System.IO
open System
open Chiron

open Model
open Prompts

/// Gets the path to the user's application data folder
/// which can be used to store application artifacts.
let AppDataDir =
    Environment.GetFolderPath(
        Environment.SpecialFolder.LocalApplicationData,
        Environment.SpecialFolderOption.DoNotVerify
    )

/// Gets the path of the file used to store commands
let CommandsFilePath (appName: string) (fname: string) =
    Path.Combine(
        List.toArray [ AppDataDir
                       appName
                       fname ]
    )

let CreateDirectoryIfNotExist (dirpath: string) =
    if not (Directory.Exists dirpath) then
        Directory.CreateDirectory(dirpath) |> ignore


/// Converts json string to list of commands.
let JsonToCommands (content: string) : Command list =
    match content with
    | "" -> list.Empty
    | _ -> content |> Json.parse |> Json.deserialize


/// Parses commands from the result of the provider into a map
/// where command name is the key.
let ParseCommands (cmdsProvider: unit -> string) : Map<string, Command> =
    cmdsProvider ()
    |> JsonToCommands
    |> List.map (fun cmd -> (cmd.Name, cmd))
    |> Map.ofList

/// Reads the file contents and returns them.
/// Returns empty string if file does not exist.
let ReadFileText (fpath: string) : string =
    if (File.Exists(fpath)) then
        File.ReadAllText(fpath)
    else
        ""

/// Converts command list into indented json string
let CommandsToJson (commands: Command List) =
    commands
    |> List.map Json.serialize
    |> Json.Array
    |> Json.formatWith JsonFormattingOptions.Pretty


/// Writes list of commands into the file as json
let WriteCommands (cmdsFilePath: string) (commandsMap: Map<string, Command>) =
    let json =
        commandsMap.Values
        |> Seq.cast
        |> List.ofSeq
        |> CommandsToJson

    File.WriteAllText(cmdsFilePath, json)


let CommandSelection (cmdMap: Map<string, Command>) : string seq =
    cmdMap.Values
    |> Seq.map (fun cmd -> sprintf "%s - %s" cmd.Platform cmd.Name)
    |> Seq.sort

let NameFromSelection (selection: string) : string =
    selection.Split '-'
    |> Seq.map (fun s -> s.Trim())
    |> Seq.last

let CreateCmdWithName (name: string) =
    let name = name
    let command = RequiredTextPrompt "Command"
    let platform = RequiredTextPrompt "Platform"
    let description = RequiredTextPrompt "Description"

    { Name = name
      Command = command
      Platform = platform
      Description = description }

let CreateCmd () =
    let name = RequiredTextPrompt "Command name"
    CreateCmdWithName name


let CreateHandler (cmdFilePath: string) (cmdMap: Map<string, Command>) =
    let cmd = CreateCmd()
    let newMap = Map.add cmd.Name cmd cmdMap
    WriteCommands cmdFilePath newMap
    newMap

let PrintCommand (cmd: Command) : unit =
    printfn ""
    printfn $"Name: {cmd.Name}\nPlatform: {cmd.Platform}\nDescription: {cmd.Description}"
    printfn $"Command:\n{cmd.Command}\n"

let ViewHandler (itemsMap: Map<string, Command>) =
    if (itemsMap.IsEmpty) then
        printfn $"No existing commands found."
    else
        let entries = CommandSelection itemsMap

        let selectedItem =
            SelectionPrompt "Select a command" entries

        let cmdName = NameFromSelection selectedItem
        let cmd = itemsMap.[cmdName]

        PrintCommand cmd

    itemsMap


let GetUserAction () : Action =
    let action =
        SelectionPrompt
            "Select action"
            [| "Create"
               "View"
               "Update"
               "Delete"
               "Exit" |]

    ActionFromString action

let UpdateHandler (cmdsFilePath: string) (itemsMap: Map<string, Command>) =
    let entries = CommandSelection itemsMap

    let selectedItem =
        SelectionPrompt "Select command to update" entries

    let cmdName = NameFromSelection selectedItem
    let cmd = itemsMap.[cmdName]
    PrintCommand cmd

    let cmd = CreateCmdWithName cmdName
    let newMap = Map.add cmdName cmd itemsMap
    WriteCommands cmdsFilePath newMap
    newMap


let DeleteHandler (cmdsFilePath: string) (itemsMap: Map<string, Command>) =
    let entries = CommandSelection itemsMap

    if (itemsMap.IsEmpty) then
        printfn "No commands to delete."
        itemsMap
    else

        let selectedItem =
            SelectionPrompt "Select command to delete" entries

        let cmdName = NameFromSelection selectedItem
        let cmd = itemsMap.[cmdName]
        PrintCommand cmd

        let confirm =
            ConfirmPrompt $"Are you sure you want to delete entry ({cmdName})?" false

        if confirm then
            let newMap = Map.remove cmdName itemsMap
            WriteCommands cmdsFilePath newMap
            newMap
        else
            itemsMap

let ProcessAction (cmdsFilePath: string) (itemsMap: Map<string, Command>) (action: Action) =
    match action with
    | Create -> CreateHandler cmdsFilePath itemsMap
    | View -> ViewHandler itemsMap
    | Update -> UpdateHandler cmdsFilePath itemsMap
    | Delete -> DeleteHandler cmdsFilePath itemsMap
    | Exit -> itemsMap
