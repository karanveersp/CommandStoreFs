# CommandStoreFs CLI

To build the single file .exe for windows:

```
dotnet publish -r win-x64 -c Release /p:IncludeNativeLibrariesForSelfExtract=true --self-contained true /p:PublishSingleFile=true
```