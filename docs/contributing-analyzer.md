# Building, Testing and Debugging the .NET Analyzer

All C# and VB.NET code analyzers present in SonarLint for Visual Studio, SonarQube and SonarCloud are being developed here. These analyzers rely on Roslyn 1.3.2 API.

## Working with the code

1. Clone [this repository](https://github.com/SonarSource/sonar-dotnet.git)
1. Download sub-modules `git submodule update --init --recursive`
1. Run `.\scripts\build\dev-build.ps1 -build -test`

In general, it is best to run commands from the Visual Studio Developer Command Prompt (if you're using ConEmu, you can setup a console task like `-new_console:C:\Workspace\sonar-dotnet cmd /k ""c:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat" "` - it starts the Developer Console inside the folder `C:\Workspace\sonar-dotnet`)

## Developing with Visual Studio 2022 or Rider

1. [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Rider](https://www.jetbrains.com/rider/download)
1. When using Visual Studio, ensure to install the following Workloads:
    - ASP.NET and web development
    - .NET desktop development
    - Visual Studio extension development
1. Ensure to install *Individual components*:
    - .NET Framework 4.8 SDK
    - .NET Framework 4.8 Targeting pack
    - .NET Framework 4.7.2 Targeting pack
    - .NET Framework 4.6 Targeting pack
    - .NET SDK
    - .NET Compiler Platform SDK
1. Install also:
    - [.NET Core 3.1 SDK](https://dotnet.microsoft.com/en-us/download/dotnet)
    - .NET 3.5 SDK (SP1) from [Microsoft download center](https://www.microsoft.com/en-us/download/details.aspx?id=21)
    - Install Visual Studio 2019 and check these SDKs in the *individual components* tab
        - .NET framework 4 targeting pack
        - .NET framework 4.5 targeting pack
1. The following environment variables must be set:
    - **JAVA_HOME** (e.g. `C:\Program Files\Java\jdk-11.0.2`)
    - **MSBUILD_PATH** - path to the MSBuild.exe executable (MSBuild 16 e.g. `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe`)
    - **NUGET_PATH** - path to the nuget.exe executable (related to the [plugin integration tests](./contributing-plugin.md#integration-tests))
    - **Sonarsource internal only** These two steps require access to SonarSource internal resources and are not possible for external contributers
        - **ORCHESTRATOR_CONFIG_URL** - url to orchestrator.properties file (for integration tests) in uri form (i.e. file:///c:/something/orchestrator.properties). See also: [Documentation in the orchestrator repository](https://github.com/sonarsource/orchestrator#configuration)
        - **RULE_API_PATH** - path to folder containing the rule api jar. The rule api jar can be found at [repox.jfrog search](https://repox.jfrog.io/ui/artifactSearchResults?name=rule-api&type=artifacts) or [repox.jfrog private releases](https://repox.jfrog.io/ui/native/sonarsource-private-releases/com/sonarsource/rule-api/rule-api/)
    - **PATH** - the **PATH** variable must contain (either *system* or *user* scope):
        - [*System*] the path to the dotnet core installation folder (`C:\Program Files\dotnet\`)
        - [*System*] the path to the MSBuild bin folder (`C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin`)
        - [*System*] the path to the visual studo installer folder (for vswhere.exe) (`C:\Program Files (x86)\Microsoft Visual Studio\Installer`)
        - [*System*] the path to the nuget executable folder (`C:\Program Files\nuget`)
        - [*System*] the path to the JDK bin folder (`C:\Program Files\Java\jdk-11.0.2\bin`)
        - [*System*] %M2_HOME%\bin (`C:\Program Files\JetBrains\IntelliJ IDEA\plugins\maven\lib\maven3\bin` [Maven cli](https://maven.apache.org/install.html). Here installed via IntelliJ IDEA.)
        - [*System*] the path to the SonarScanner for .NET folder and to the Scanner CLI ([SonarScanner download](https://github.com/SonarSource/sonar-scanner-msbuild/releases))
1. Open `analyzers/SonarAnalyzer.sln`

## Running Tests

### Unit Tests

You can run the Unit Tests via the Test Explorer of Visual Studio or using `.\scripts\build\dev-build.ps1 -test`

### Integration Tests

#### Types of tests

For most projects, there are JSON files in the [expected](../analyzers/its/expected) folder with expected issues. One JSON file per rule.

For the [ManuallyAddedNoncompliantIssues](../analyzers/its/sources/ManuallyAddedNoncompliantIssues) project, we verify for each file the issues for one specific rule - like we do for Unit Tests. The first occurrence must specify the rule ID (`// Noncompliant (S9999)`), and the next occurrences can only have `// Noncompliant`. If multiple rules are raising issues in that file, they will be ignored. The framework can only verify one rule per file. Look at some files inside the **ManuallyAddedNoncompliantIssues** project.

The same applies for **ManuallyAddedNoncompliantIssuesVB**.

For details on how the parsing works, read the [regression-test.ps1](../analyzers/its/regression-test.ps1) script.

#### Running the tests
To run the ITs you will need to follow this pattern:

1. Make sure the project is built: Integration tests don't build the analyzer, but use the results of the latest build (debug or release)
1. Open the `Developer Command Prompt for VS2022` from the start menu
1. Go to `PATH_TO_CLONED_REPOSITORY/analyzers/its`
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
If you run the project `ReviewDiffs` in debug mode, it will print in the output windows all places where a difference has been found. From there you can easily navigate between differences (double-click, F4...).

*Disclaimer*: This program is still very new and the paint is still very wet. It may not work in all situations, but it's easy to update it ??.

*PS*: This program outputs message in a way that is compatible with the [VsColorOutput](https://marketplace.visualstudio.com/items?itemName=MikeWard-AnnArbor.VSColorOutput) extension, so that errors are colored in red, and differences are colored like warnings.

### Debug an analysis started from the command line / Java ITs

If you want to debug the analysis of a project, you can add a [`Debugger.Launch()`](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debugger.launch?view=net-6.0) breakpoint in the class you want to debug. Rebuild `SonarAnalyzer.sln` and link the analyzer debug binaries to the project you want to debug the analysis for.

- If you are analyzing the project with the Scanner for .NET, after the begin step you can replace the binaries in the local cache (`%TEMP%\.sonarqube\resources\` - the `0` folder for the C# Analyzer, the `1` folder for the VB .NET analyzer), and then run the build.
- If you don't want to use the Scanner for .NET, you can manually reference the binaries in `analyzers/packaging/binaries/` in the {cs,vb}proj file with `<Analyzer Include=... />` items (see [SonarAnalyzer.Testing.ImportBefore.targets](../analyzers/its/SonarAnalyzer.Testing.ImportBefore.targets#L46) as an example)

Please note that if the rule is not in SonarWay, you will also need to enable it in a RuleSet file and link it in the {cs,vb}proj file with the `<CodeAnalysisRuleSet>` property (see [example](../analyzers/src/Directory.Build.targets#L8)).

This also works with the Java ITs, as long as the debug assemblies are in the folder which is used by the Java ITs.

When running the build and doing the Roslyn analysis, when hitting the `Debugger.Launch()` line, a UI window will prompt you to choose a debugger, where your IDE should show up - you will be able to open the solution and debug.

After the debug session, remove the `Debugger.Launch()` line.

### Java ITs

**Internal only**

* Use IntelliJ IDEA
  * Follow the [Code Style Configuration for Intellij](https://github.com/SonarSource/sonar-developer-toolset#code-style-configuration-for-intellij) instructions
  * Open the root folder of the repo
  * Make sure the `its`, `sonar-csharp-plugin`, `sonar-dotnet-shared-library`, and `sonar-vbnet-plugin` folders are are imported as Maven modules (indicated by a blue square). Search for `pom.xml` in the folders and make it a maven project if not.
* Add the following environment variables (*user* scope)
  * **ARTIFACTORY_URL** https://repox.jfrog.io/repox
  * **ARTIFACTORY_USER** your repox.jfrog username (see e.g. orchestrator.properties)
  * **ARTIFACTORY_PASSWORD** the api key for repox.jfrog (see e.g. orchestrator.properties)
* Create `settings.xml` in the `%USERPROFILE%\.m2` directory. A template can be found in the [Developer box section in the extranet](https://xtranet-sonarsource.atlassian.net/wiki/spaces/DEV/pages/776711/Developer+Box#Maven-Settings). Change the username and password settings with the values from the environment variables above.
* Run `mvn install clean -DskipTests=true` in the respective directories (pom.xml). To build all artefacts run `.\scripts\build\dev-build.ps1 -buildJava`
* Use the IDE to run unit tests in the projects.

## Contributing

Please see the [How to contribute](../README.md#how-to-contribute) section for details on
contributing changes back to the code.
