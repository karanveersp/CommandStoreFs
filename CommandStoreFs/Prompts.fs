module Prompts

open Sharprompt

/// Prompts the user for a y/n response with the given default.
let ConfirmPrompt (message: string) (defaultVal: bool) : bool = Prompt.Confirm(message, defaultVal)

/// Prompts the user for non-empty text input.
let RequiredTextPrompt (msg: string) : string =
    Prompt.Input<string>(
        msg,
        validators =
            [| Validators.Required()
               Validators.MinLength(1) |]
    )

/// Prompts the user for a selection from the given choices.
let SelectionPrompt (message: string) (choices: seq<string>) : string = Prompt.Select(message, choices)
