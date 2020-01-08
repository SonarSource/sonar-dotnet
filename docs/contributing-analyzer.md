# Building, Testing and Debugging the .NET Analyzer

All C# and VB .NET analyzers present in SonarLint for Visual Studio, in the SonarQube SonarC# Plugin or in the SonarQube SonarVB plugin are being developed here. These analyzers rely on Roslyn 1.3.2 API.

## Working with the code

1. Clone [this repository](https://github.com/SonarSource/sonar-dotnet.git)
1. Download sub-modules `git submodule update --init --recursive`
1. Run `.\scripts\build\dev-build.ps1 -restore -build -test`

In general, it is best to run commands from the Visual Studio Developer Command Prompt (if you're using ConEmu, you can setup a console task like `-new_console:C:\Workspace\sonar-dotnet cmd /k "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\VsDevCmd.bat"` - it starts the Developer Console inside the folder `C:\Workspace\sonar-dotnet`)

## Developing with Visual Studio 2019

1. [Visual Studio 2019 Version 16.4.1](https://visualstudio.microsoft.com/vs/)
    - Ensure .Net Desktop development (C#, VB and F#), .NET Core, Visual Studio extension development and ASP.Net and web development are included in the selected work loads
1. [.NET Core SDK 2.1.402](https://dotnet.microsoft.com/download/dotnet-core/2.1)
1. Open `SonarAnalyzer.sln` in the `sonaranalyzer-dotnet` subfolder

The following environment variables must be set:
- **JAVA_HOME**
- **MAVEN_HOME** / **M2_HOME**
- **MSBUILD_PATH** - path to the MSBuild.exe executable from the Visual Studio installation folder - to MSBuild 16
- **NUGET_PATH** - path to the nuget.exe executable (related to the [plugin integration tests](./contributing-plugin.md#integration-tests))
- **ORCHESTRATOR_CONFIG_URL** - path to orchestrator.properties file (for integration tests)
- **rule_api_path** - path to folder containing the rule api jar
- **Path** - the **Path** must contain:
    - the path to the dotnet core installation folder
    - the path to the MSBuild bin folder
    - the path to the visual studo installer folder (for vswhere.exe)
    - the path to the nuget executable folder (e.g. C:\Program Files\nuget)
    - the path to the JDK bin folder
    - %M2_HOME%\bin
    - the path to the Scanner For MSBuild folder and to the Scanner CLI

## NuGet lock files update

NuGet lock file restore was enabled for all .Net projects in order to be able to use the Cache task on Azure pipeplines.

More details here: https://docs.microsoft.com/en-us/azure/devops/pipelines/caching/?view=azure-devops#netnuget

These files need to be updated on every dependency change by running: `dotnet restore --force-evaluate`

## Running Tests

### Unit Tests

You can run the Unit Tests via the Test Explorer of Visual Studio or using `.\scripts\build\dev-build.ps1 -test`

### Integration Tests
#### Running the tests
To run the ITs you will need to follow this pattern:

1. Make sure the project is built: Integration tests don't build the analyzer, but use the results of the latest build (debug or release)
1. Open the `Developer Command Prompt for VS2019` from the start menu
1. Go to `PATH_TO_CLONED_REPOSITORY/sonaranalyzer-dotnet/its`
1. Run `powershell`
1. Run `.\regression-test.ps1`

Notes: 

1. You can run a single rule using the `-ruleId` parameter (e.g. `.\regression-test.ps1 -ruleId S1234`)
1. You can run a single project using the `-project` parameter (e.g. `.\regression-test.ps1 -project Nancy`)

If the script ends with `SUCCESS: No differences were found!` (or exit code 0), this means the changes you have made haven't impacted any rule.

If the script ends with `ERROR: There are differences between the actual and the expected issues.` (or exit code 1),
the changes you have made have impacted one or many issues raised by the rules.

Note: if you are facing compilation errors on Windows 10 due to unknown characters, disable `beta use unicode utf-8 for worldwide language support` from your `Region Settings`.

#### Updating the references
You can run `.\update-expected.ps1` to update the list of expected issues. Please review all added/removed/updated issues to confirm they are wanted. Only after reviewing each difference do the commit.

_Note: Integration tests build the code to analyze. If you have an antivirus program on your computer, this may fail with some error messages about an executable file that cannot be open for writing. If this happens, look at the antivirus logs for confirmation, and contact IT team to add an rule to your antivirus program..._

#### Manual differences review
You can visualize the differences using:

1. `cd actual`
1. `git diff --cached`


#### Semi-automated differences review
If you run the project `ReviewDiffs` in debug mode from Visual Studio, it will print in the output windows all places where a difference has been found. From there you can easily navigate between differences (double-click, F4...).

*Disclaimer*: This program is still very new and the paint is still very wet. It may not work in all situations, but it's easy to update it ??.

*PS*: This program outputs message in a way that is compatible with the [VsColorOutput](https://marketplace.visualstudio.com/items?itemName=MikeWard-AnnArbor.VSColorOutput) extension, so that errors are colored in red, and differences are colored like warnings.

### Manual Tests

From Visual Studio, make sure `SonarAnalyzer.Vsix.csproj` is selected as startup project. And then do the following:

1. Make sure SonarLint for Visual Studio is uninstalled
2. Hit `F5` to launch the experimental instance of Visual Studio
3. Open one of the following solutions from the experimental instance:
    - [Akka.NET](akka.net/src/Akka.sln)
    - [Nancy](Nancy/src/Nancy.sln)
    - [Ember-MM](Ember-MM/Ember%20Media%20Manager.sln)
4. Turn on your new rule in [Validation Ruleset](ValidationRuleset.ruleset), review the results, improve, and setup the regression test once you are satisfied.
    - Note: the solutions have been pre-configured to use this ruleset on all their projects.

## Contributing

Please see [Contributing Code](../CONTRIBUTING.md) for details on
contributing changes back to the code.
