module Model

open Chiron


type Action =
    | View
    | Create
    | Update
    | Delete
    | DisplayPath
    | Exit

let ActionFromString (s: string) : Action =
    match s with
    | "Create" -> Create
    | "View" -> View
    | "Update" -> Update
    | "Delete" -> Delete
    | "Show storage file path" -> DisplayPath
    | "Exit" -> Exit
    | _ -> failwith $"{s} is not a valid action"

type Command =
    { Name: string
      Command: string
      Platform: string
      Description: string }

    static member ToJson(c: Command) =
        json {
            do! Json.write "name" c.Name
            do! Json.write "command" c.Command
            do! Json.write "platform" c.Platform
            do! Json.write "description" c.Description
        }

    static member FromJson(_: Command) =
        json {
            let! name = Json.read "name"
            let! command = Json.read "command"
            let! platform = Json.read "platform"
            let! description = Json.read "description"

            return
                { Name = name
                  Command = command
                  Platform = platform
                  Description = description }
        }
