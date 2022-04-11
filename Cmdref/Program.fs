﻿open Util
open Prompts
open Model
open System.IO

[<Literal>]
let AppName = "cmdref"

[<Literal>]
let CommandsFileName = "cmdref.json"

let CommandsFile =
    CommandsFilePath AppName CommandsFileName

let dirpath = Path.GetDirectoryName(CommandsFile)
CreateDirectoryIfNotExist dirpath

let cmdsProvider = fun () -> ReadFileText CommandsFile

let mutable CommandsMap = ParseCommands cmdsProvider
let mutable finished = false

while not finished do
    if CommandsMap.IsEmpty then
        printfn "No commands saved"

        let createNewCmd =
            ConfirmPrompt "Create a new entry?" false

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
