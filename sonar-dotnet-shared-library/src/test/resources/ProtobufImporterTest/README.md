Test data generation
====================

The test files in this directory are generated with:

1. Copy `Program.cs` into `sonar-examples/projects/languages/csharp/ConsoleApplication1`

2. Open the `ConsoleApplication1` solution, add `Program.cs` to the project, save and build it

3. Install the latest C# plugin in local SonarQube

4. Launch an analysis locally

5. Copy `Program.cs` and `*.pb` files to *this* directory

6. Run `ProtobufFilterTool`, it will:
    1. Remove all other files from `.pb` files except `Program.cs`
    2. Replace the absolute path of `Program.cs` with simply `Program.cs`

7. Re-run all the unit tests, update the expected values accordingly

8. Commit, push, release

9. Copy (the relevant) files to C# project, re-run tests, update expected values, etc

### custom-log.pb

Log file was generated in C# unit test like this:

```
using var metricsStream = File.Create(@"<path>\custom-log.pb");
var messages = new[]
{
    new LogInfo {Severity = LogSeverity.Debug, Text = "First debug line" },
    new LogInfo {Severity = LogSeverity.Debug, Text = "Second debug line" },
    new LogInfo {Severity = LogSeverity.Info, Text = "Single info line" },
    new LogInfo {Severity = LogSeverity.Warning, Text = "Single warning line" }
};
foreach (var message in messages)
{
    message.WriteDelimitedTo(metricsStream);
}
```

### unknown-log.pb

Same as `custom-log.pb` with this message:

```
new LogInfo {Severity = LogSeverity.UnknownSeverity, Text = "Unknown severity for Coverage" }
```
