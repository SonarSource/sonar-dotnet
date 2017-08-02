# Building, Testing and Debugging the Analyzer

All C# and VB.NET analyzers present in SonarLint for Visual Studio, in the SonarQube SonarC# Plugin or in the
SonarQube SonarVB plugin are being developed here.
These analyzers rely on Roslyn 1.0 API.

## Working with the code

1. Clone SonarC# repository: https://github.com/SonarSource/sonar-csharp.git
2. Go to `sonaranalyzer-dotnet` sub-folder
3. Open SonarAnalyzer.sln

Note: SonarAnalyzer.VS2015.sln and SonarAnalyzer.VS2017.sln are not ready for use yet and are part of our plan to support
.Net Core and C# 7 features.

## Developing with Visual Studio

We currently rely on Visual Studio 2015 for the development on the Analyzer. Any change to the solution or the projects
linked to development with Visual Studio 2017 should not be included in PR.

## Running Tests

### Unit Tests

You can run the Unit Tests via Test Explorer of Visual Studio.

### Integration Tests

If this is the first time you are running the ITs you will need to run the following command:

`git submodule update --init --recursive`

To run the ITs you will need to follow this pattern:

1. Open the `Developer Command Prompt for VS2015`
2. Go to `PATH_TO_CLONED_REPOSITORY/sonar-csharp/sonaranalyzer-dotnet/its`
3. Run `powershell`
4. Run `.\regression-test.ps1`

If the script ends with `SUCCESS: No differences were found!` (or exit code 0), this means the changes you have made
haven't impacted any rule.

If the script ends with `ERROR: There are differences between the actual and the expected issues.` (or exit code 1),
the changes you have made have impacted one or many issues raised by the rules.
You can visualize the differences using:

1. `cd actual`
2. `git diff --cached`

Please review all added/removed/updated issue to confirm they are wanted. When this is done run `.\UpdateExpected.ps1`
to update the list of expected issues.

### Manual Tests

From Visual Studio, make sure SonarAnalyzer.Vsix.csproj is selected as startup project. And then do the following:

1. Make sure SonarLint for Visual Studio is uninstalled
2. Hit `F5` to launch the experimental instance of Visual Studio
3. Open one of the following solutions from the experimental instance:
  * [Akka.NET](akka.net/src/Akka.sln)
  * [Nancy](Nancy/src/Nancy.sln)
  * [Ember-MM](Ember-MM/Ember Media Manager.sln)
4. Turn on your new rule in [Validation Ruleset](ValidationRuleset.ruleset), review the results, improve, and setup the
regression test once you are satisfied.
Note: the solutions have been pre-configured to use this ruleset on all their projects.

## Contributing

Please see [Contributing Code](../CONTRIBUTING.md) for details on
contributing changes back to the code.
