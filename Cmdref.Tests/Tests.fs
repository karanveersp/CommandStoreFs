module Tests

open Xunit

open Util
open Model


[<Fact>]
let ``Parse commands from json works`` () =
    let cmdsProvider =
        fun () ->
            "[{\"name\":\"run go program\",\n
            \"command\":\"go run <.go file>\",\n
            \"platform\":\"go cli\",\n
            \"description\":\"run a go program\"}]"

    let expected =
        { Name = "run go program"
          Command = "go run <.go file>"
          Platform = "go cli"
          Description = "run a go program" }

    let actual = ParseCommands cmdsProvider

    Assert.Equal(expected, actual.["run go program"])


[<Fact>]
let ``Parse commands returns empty map when provider string is empty`` () =
    let cmdsProvider = fun () -> ""

    let expected = Map.empty
    let actual = ParseCommands cmdsProvider

    Assert.Equal<Map<string, Command>>(expected, actual)

[<Fact>]
let ``Json to commands returns empty list when given empty string`` () =
    let expected = list.Empty
    let actual = JsonToCommands ""
    Assert.Equal<Command list>(expected, actual)


[<Fact>]
let ``Json to commands works`` () =
    let expected =
        [ { Name = "run dotnet tests"
            Command = "dotnet test"
            Platform = "dotnet cli"
            Description = "Runs dotnet unit tests" } ]

    let actual =
        JsonToCommands
            "[{\"name\": \"run dotnet tests\",\n
            \"command\": \"dotnet test\",\n
            \"platform\": \"dotnet cli\",\n
            \"description\": \"Runs dotnet unit tests\"}]"

    Assert.Equal<Command list>(expected, actual)
