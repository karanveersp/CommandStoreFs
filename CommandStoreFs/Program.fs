open Util
open Model
open System.IO

let appDataDir = GetAppDataDir()

let CommandsFileName = "commands.json"

let CommandsFile =
    Path.Combine(
        List.toArray [ appDataDir
                       CommandsFileName ]
    )

let mutable CommandsMap = LoadCommandsIfExist CommandsFile
let mutable finished = false

printfn "Commands file: %s" CommandsFile

while not finished do
    if CommandsMap.IsEmpty then
        printfn "No commands stored"

        let createNewCmd = YesNoPrompt "Create a new entry?" false

        if createNewCmd then
            CommandsMap <- CreateHandler CommandsFile CommandsMap
        else
            finished <- true
    else
        let selectedAction = GetUserAction()

        match selectedAction with
        | Exit ->
            printfn "Bye!"
            finished <- true
        | _ -> CommandsMap <- ProcessAction CommandsFile CommandsMap selectedAction
