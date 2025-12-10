### How to re-create the pb files

- Create a Project in SonarQube (manual setup).
- Open a console in this directory.
- For all [Roslyn subdirectories](https://github.com/SonarSource/sonar-dotnet-enterprise/tree/master/sonar-dotnet-core/src/test/resources/RazorProtobufImporter) do the following:
    - Copy the `global.json` file to this directory.
    - Run the "BEGIN" as given by SonarQube.
    - Run `dotnet build .\WebProject\BlazorWebAssembly.csproj`.
    - Copy the pb files from `.sonarqube\out\0\output-cs` to the Roslyn subdirectory.
    - Delete the `.sonarqube` and `global.json`.
- Update the ITs if needed.
