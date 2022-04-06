# dotnet-cmdref

A cli app to store your commands for future reference.
Use it to
* recall long commands you tend to forget
* build a reference library of commands

## Install

The app can be installed as a dotnet-tool and executed with `cmdref`.

1. Clone the repo using `git clone https://github.com/karanveersp/dotnet-cmdref.git`
2. `dotnet build Cmdref`
3. `dotnet pack Cmdref`
4. `dotnet tool install -g dotnet-cmdref --add-source ./Cmdref/nupkg/`
5. Run the program with `cmdref`

