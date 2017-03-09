Test data generation
====================

The test files in this directory are generated with:

1. Copy `Program.cs` into `sonar-examples/projects/languages/csharp/ConsoleApplication1`

2. Open the `ConsoleApplication1` solution, add `Program.cs` to the project, save and build it

3. Install the latest C# plugin in local SonarQube

4. Launch an analysis locally **using MSBuild 12**, otherwise `issues.pb` would be empty

5. Copy `Program.cs` and `*.pb` files to *this* directory

6. Run `ProtobufFilterTool`, it will:
    1. Remove all other files from `.pb` files except `Program.cs`
    2. Replace the absolute path of `Program.cs` with simply `Program.cs`

7. Re-run all the unit tests, update the expected values accordingly

8. Commit, push, release

9. Copy (the relevant) files to C# project, re-run tests, update expected values, etc
