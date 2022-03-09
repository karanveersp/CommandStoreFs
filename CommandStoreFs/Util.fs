module Util

open System.IO
open Sharprompt
open System
open Chiron

open Model

let GetAppDataDir () =
    let dirPath =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolderOption.DoNotVerify
            ),
            "command-store-fs"
        )

    Directory.CreateDirectory(dirPath) |> ignore
    dirPath


let JsonToCommands (content: string) : Command list =
    content |> Json.parse |> Json.deserialize


let LoadCommandsIfExist (cmdsFilePath: string) : Map<string, Command> =
    if (File.Exists(cmdsFilePath)) then
        let jsonContent = File.ReadAllText(cmdsFilePath)
        let cmdList = JsonToCommands jsonContent

        cmdList
        |> List.map (fun cmd -> (cmd.Name, cmd))
        |> Map.ofList

    else
        list.Empty |> Map.ofList

let CommandsToJson (commands: Command List) =
    commands
    |> List.map Json.serialize
    |> Json.Array
    |> Json.formatWith JsonFormattingOptions.Pretty



let WriteCommands (cmdsFilePath: string) (commandsMap: Map<string, Command>) =
    let json =
        commandsMap.Values
        |> Seq.cast
        |> List.ofSeq
        |> CommandsToJson

    File.WriteAllText(cmdsFilePath, json)


let YesNoPrompt (message: string) (defaultVal: bool) : bool = Prompt.Confirm(message, defaultVal)

let RequiredTextPrompt (msg: string) : string =
    Prompt.Input<string>(
        msg,
        validators =
            [| Validators.Required()
               Validators.MinLength(1) |]
    )

let SelectionPrompt (message: string) (choices: seq<string>) : string = Prompt.Select(message, choices)


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

let ViewHandler (itemsMap: Map<string, Command>) =
    if (itemsMap.IsEmpty) then
        printfn $"No existing commands found."
    else
        let entries = itemsMap.Keys

        let selectedItem =
            SelectionPrompt "Select a command" entries

        printfn $"\n{string itemsMap.[selectedItem]}\n"

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
    let entries = itemsMap.Keys

    let selectedItem =
        SelectionPrompt "Select command to update" entries

    printfn $"{string itemsMap.[selectedItem]}"

    let cmd = CreateCmdWithName selectedItem
    let newMap = Map.add selectedItem cmd itemsMap
    WriteCommands cmdsFilePath newMap
    newMap


let DeleteHandler (cmdsFilePath: string) (itemsMap: Map<string, Command>) =
    let entries = itemsMap.Keys

    if (itemsMap.IsEmpty) then
        printfn "No commands to delete."
        itemsMap
    else

        let selectedItem =
            SelectionPrompt "Select command to delete" entries

        let confirm =
            YesNoPrompt $"Are you sure you want to delete entry ({selectedItem})?" false

        if confirm then
            let newMap = Map.remove selectedItem itemsMap
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
