# NuGet packaging for CBDE

In order to create a new version (manually):
- update the version in *.nuspec file
- copy the new `cbde-dialect-checker.exe` and `dotnet-symbolic-execution.exe` to this folder
- run `nuget pack SonarSource.CBDE.nuspec`
- ask your favorite repox administrator to publish the package (to `sonarsource-nuget-public`)

## Additional Notes

- `cbde-dialect-checker.exe` is used in unit tests to validate the generated intermediate representation
- `dotnet-symbolic-execution.exe` is the bug detection engine

### SonarSource.CBDE.nuspec

XML manifest that contains the package metadata. Used to generate `SonarSource.CBDE.<version>.nupkg`.

### SonarSource.CBDE.targets

Adds build variables for paths to ease build configuration:
- `cbde_tools`: root folder for CBDE tools
- `cbde_dialect_checker_windows`: windows version of `cbde-dialect-checker.exe`
- `dotnet_symbolic_execution_windows`: windows version of `dotnet-symbolic-execution.exe` engine
