# Code Quality and Security for C\# and VB.NET

[![Build Status](https://dev.azure.com/sonarsource/DotNetTeam%20Project/_apis/build/status/Sonar.Net?branchName=master)](https://dev.azure.com/sonarsource/DotNetTeam%20Project/_build/latest?definitionId=77&branchName=master)

|Product|Quality Gate|Coverage|
|:--:|:--:|:--:|
|Analyzer|[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=sonaranalyzer-dotnet&metric=alert_status)](https://sonarcloud.io/dashboard?id=sonaranalyzer-dotnet)|[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=sonaranalyzer-dotnet&metric=coverage)](https://sonarcloud.io/component_measures?id=sonaranalyzer-dotnet&metric=coverage)|
|Plugin|[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=org.sonarsource.dotnet%3Asonar-dotnet&metric=alert_status)](https://sonarcloud.io/dashboard?id=org.sonarsource.dotnet%3Asonar-dotnet)|[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=org.sonarsource.dotnet%3Asonar-dotnet&metric=coverage)](https://sonarcloud.io/component_measures?id=org.sonarsource.dotnet%3Asonar-dotnet&metric=coverage)|

[Static analysis](https://en.wikipedia.org/wiki/Static_program_analysis) of C# and VB.NET
languages in [SonarQube](https://www.sonarsource.com/products/sonarqube), [SonarCloud](https://www.sonarsource.com/products/sonarcloud) and [SonarLint](https://www.sonarsource.com/products/sonarlint) code quality and security products. These Roslyn analyzers allow you to produce [Clean Code](https://www.sonarsource.com/solutions/clean-code/) that is safe, reliable, and maintainable by helping you find and correct bugs, vulnerabilities, and code smells in your codebase.

## Features

* [460+ C# rules](https://rules.sonarsource.com/csharp) and [210+ VB.&#8203;NET rules](https://rules.sonarsource.com/vbnet)
* Metrics (cognitive complexity, duplications, number of lines, etc.)
* Import of [test coverage reports](https://community.sonarsource.com/t/9871) from Visual Studio Code Coverage, dotCover, OpenCover, Coverlet, Altcover.
* Import of third-party Roslyn Analyzers results
* Support for [custom rules](https://github.com/SonarSource/sonarqube-roslyn-sdk)

## Useful public resources

* [Project homepage](https://redirect.sonarsource.com/plugins/csharp.html)
* [Issue tracking](./docs/issues.md)
* [C# rules](https://rules.sonarsource.com/csharp)
* [VB.Net rules](https://rules.sonarsource.com/vbnet)

### Nuget.org packages

* [SonarAnalyzer.CSharp](https://www.nuget.org/packages/SonarAnalyzer.CSharp/)
* [SonarAnalyzer.VisualBasic](https://www.nuget.org/packages/SonarAnalyzer.VisualBasic/)

### Integration with SonarQube and SonarCloud

* [Analyze projects with SonarScanner for .NET](https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html)
* [Importing code coverage](https://community.sonarsource.com/t/9871)
* [SonarQube and the code coverage](https://community.sonarsource.com/t/4725)

## Do you have a question or feedback?

* Contact us on [our Community Forum](https://community.sonarsource.com/) to provide feedback, ask for help, and request new rules or features.
* [Create a GitHub Issue](https://github.com/SonarSource/sonar-dotnet/issues/new/choose) if you've found a bug, False-Positive or False-Negative.

## Get started

* [Building, testing and debugging the .NET analyzer](./docs/contributing-analyzer.md)
* [Building, testing and debugging the Java plugin](./docs/contributing-plugin.md)
* [How to re-generate NuGet lock files](./docs/regenerate-lock-files.md)
* [Using the rspec.ps1 script](./scripts/rspec/README.md)

## How to contribute

There are many ways you can contribute to the `sonar-dotnet` project.
When contributing, please respect our [Code of Conduct](./docs/code-of-conduct.md).

### Join the discussions

One of the easiest ways to contribute is to share your feedback with us (see [give feedback](#do-you-have-a-question-or-feedback)) and also answer questions from [our community forum](https://community.sonarsource.com/).
You can also monitor the activity on this repository (opened issues, opened PRs) to get more acquainted with what we do.

### Pull Request (PR)

If you want to fix [an issue](https://github.com/SonarSource/sonar-dotnet/issues),
please read the [Get started](#get-started) pages first and make sure that you follow [our coding style](./docs/coding-style.md).

Before submitting the PR, make sure [all tests](./docs/contributing-analyzer.md#running-unit-tests) are passing (all checks must be green).

* We suggest you do not pick issues with the `Area: CFG` label
_(they are difficult, can have many side effects and are less likely to be accepted)_.
* We suggest you do not implement new rules unless they are already specified for C# and/or VB.NET on
our [rules repository](https://jira.sonarsource.com/projects/RSPEC).

Note: Our CI does not get automatically triggered on the PRs from external contributors.
A member of our team will review the code and trigger the CI on demand by adding a comment on the PR (see [Azure Pipelines Comment triggers docs](https://docs.microsoft.com/en-us/azure/devops/pipelines/repos/github?view=azure-devops&tabs=yaml#comment-triggers)):
- `/azp run Sonar.Net` - It will run the full pipeline, including plugin tests and promotion

### Join us

If you would like to work on this project full-time, [we are hiring!](https://www.sonarsource.com/company/jobs/static-code-analyzer-cs-developer)

## Custom Rules

To request new rules, Contact us on [our Community Forum](https://community.sonarsource.com/c/suggestions/).

If you have an idea for a rule but you are not sure that everyone needs it, you can implement your own Roslyn analyzer.
- You can start with [this tutorial from Microsoft](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix) to write an analyzer.
- All Roslyn-based issues are picked up by the [SonarScanner for .NET](https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html)
and pushed to SonarQube / SonarCloud as external issues.
- Also check out [SonarQube Roslyn SDK](https://github.com/SonarSource-VisualStudio/sonarqube-roslyn-sdk) to embed
your Roslyn analyzer in a SonarQube plugin, if you want to manage your rules from SonarQube.

## Configuring Rules

### SonarQube / SonarCloud and SonarLint in Connected Mode

Open the rule in SonarQube / SonarCloud, scroll down, and (in case the rule has parameters), you can configure the parameters for each Quality Profile the rule is part of.

Use SonarLint Connected Mode to connect to SonarQube and SonarCloud.

### SonarLint

The easiest way is to configure a Quality Profile in SonarCloud.

* Create a dummy repository and analyze it in SonarCloud (it's free for open-source).
* Configure the Quality Profile in SonarCloud for the project you created.
* Then connect SonarLint to that project, and it will download the configuration (ruleset and SonarLint.xml files) locally and update your project based on the Quality Profile.

### Standalone NuGet

The rules from standalone NuGet packages can be enabled or disabled in the same way as the other analyzers based on Roslyn, by using the `.globalconfig` or `.editorconfig` files.
See: https://learn.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=vs-2022#set-rule-severity-in-an-editorconfig-file

If the rules are parameterized, the parameter values can be changed using `SonarLint.xml` additional files.

The first step is to create a new file, named `SonarLint.xml`, that has the following structure:
```xml
<?xml version="1.0" encoding="utf-8"?>
<AnalysisInput xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Settings>
    <Setting>
      <Key>sonar.cs.analyzeGeneratedCode</Key>
      <Value>false</Value>
    </Setting>
  </Settings>
  <Rules>
    <Rule>
      <Key>S107</Key>
      <Parameters>
        <Parameter>
          <Key>max</Key>
          <Value>2</Value>
        </Parameter>
      </Parameters>
    </Rule>
  </Rules>
</AnalysisInput>
```
Then, update the projects to include this additional file:
```xml
<ItemGroup>
  <AdditionalFiles Include="SonarLint.xml" />
</ItemGroup>
```

## Internal resources

### Build configuration

* [VM Images repository](https://github.com/SonarSource/re-ci-images)
* [Provisioning repository](https://github.com/SonarSource/dotnet-infra/blob/main/src/dotnet_infra/constructs/asg.py)
* [Azure Pipelines](https://sonarsource.visualstudio.com/DotNetTeam%20Project/_build?definitionId=77&_a=summary)
* [Peachee configuration](https://github.com/SonarSource/peachee-languages/tree/dotnet)

## License

Copyright 2014-2024 SonarSource.

Licensed under the [GNU Lesser General Public License, Version 3.0](http://www.gnu.org/licenses/lgpl.txt)
