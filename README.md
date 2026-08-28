<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://assets-eu-01.kc-usercontent.com/ef593040-b591-0198-9506-ed88b30bc023/a23fc7ba-23f0-489a-829d-ed88c0748521/Sonar_Logo_Dark%20Backgrounds.svg">
    <img src="https://assets-eu-01.kc-usercontent.com/ef593040-b591-0198-9506-ed88b30bc023/82c13eba-d95c-4bb8-8007-7ce77c14e043/Sonar_Logo_Light%20Backgrounds.svg" alt="Sonar logo" width="400">
  </picture>
</p>

# SonarQube analyzers for .NET

[![Build Status](https://dev.azure.com/sonarsource/DotNetTeam%20Project/_apis/build/status/Sonar.Net?branchName=master)](https://dev.azure.com/sonarsource/DotNetTeam%20Project/_build/latest?definitionId=77&branchName=master)

|Product|Quality Gate|Coverage|
|:--:|:--:|:--:|
|Analyzer|[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=sonaranalyzer-dotnet&metric=alert_status)](https://sonarcloud.io/dashboard?id=sonaranalyzer-dotnet)|[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=sonaranalyzer-dotnet&metric=coverage)](https://sonarcloud.io/component_measures?id=sonaranalyzer-dotnet&metric=coverage)|
|Plugin|[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=org.sonarsource.dotnet%3Asonar-dotnet&metric=alert_status)](https://sonarcloud.io/dashboard?id=org.sonarsource.dotnet%3Asonar-dotnet)|[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=org.sonarsource.dotnet%3Asonar-dotnet&metric=coverage)](https://sonarcloud.io/component_measures?id=org.sonarsource.dotnet%3Asonar-dotnet&metric=coverage)|

[![NuGet](https://img.shields.io/nuget/v/SonarAnalyzer.CSharp?label=NuGet)](https://www.nuget.org/packages/SonarAnalyzer.CSharp/)
[![GitHub stars](https://img.shields.io/github/stars/SonarSource/sonar-dotnet?style=flat)](https://github.com/SonarSource/sonar-dotnet)
[![License](https://img.shields.io/badge/license-SSALv1-blue)](#license)
[![Community forum](https://img.shields.io/badge/community-forum-blue)](https://community.sonarsource.com/)

SonarQube analyzers for .NET inspect C# and VB.NET code for bugs, vulnerabilities, and maintainability issues. They surface findings while developers work in their IDE and when teams analyze projects with SonarQube Server or SonarQube Cloud.

This repository contains the source for the Roslyn analyzers and the Java plugin that integrates .NET analysis with SonarQube. The analyzers apply the same rules to developer-written and AI-generated code, giving teams a consistent way to verify code before it reaches production.

## Features

* 480+ C# rules and 210+ VB.&#8203;NET rules
* Metrics (cognitive complexity, duplications, number of lines, etc.)
* Import of [test coverage reports](https://community.sonarsource.com/t/9871) from Visual Studio Code Coverage, dotCover, OpenCover, Coverlet, Altcover.
* Import of third-party Roslyn Analyzers results
* Support for [custom rules](https://github.com/SonarSource/sonarqube-roslyn-sdk)

The analyzers work with [SonarQube Server](https://www.sonarsource.com/products/sonarqube/server/), [SonarQube Cloud](https://www.sonarsource.com/products/sonarqube/cloud/), and [SonarQube for IDE](https://www.sonarsource.com/products/sonarqube/ide/).

## Useful public resources

* [Project homepage](https://redirect.sonarsource.com/plugins/csharp.html)
* [Issue tracking](./docs/issues.md)

### Nuget.org packages

* [SonarAnalyzer.CSharp](https://www.nuget.org/packages/SonarAnalyzer.CSharp/)
* [SonarAnalyzer.VisualBasic](https://www.nuget.org/packages/SonarAnalyzer.VisualBasic/)

### Integration with SonarQube

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

## How to contribute

There are many ways you can contribute to the `sonar-dotnet` project.
When contributing, please respect our [Code of Conduct](./docs/code-of-conduct.md).

### Join the discussions

One of the easiest ways to contribute is to share your feedback with us (see [give feedback](#do-you-have-a-question-or-feedback)) and also answer questions from [our community forum](https://community.sonarsource.com/).
You can also monitor the activity on this repository (opened issues, opened PRs) to get more acquainted with what we do.

### Pull Request (PR)

If you want to fix [an issue](https://github.com/SonarSource/sonar-dotnet/issues),
please read the [Get started](#get-started) pages first and make sure that you follow [our coding style](./docs/coding-style.md).
We suggest avoiding the implementation of new rules, as a specification process is required first.

Before submitting the PR, make sure [all tests](./docs/contributing-analyzer.md#running-unit-tests) are passing (all checks must be green).

If you did not sign the Contributor License Agreement in the past, please let us know in the PR your user handle from our [Community Forum](https://community.sonarsource.com/). We will arrange the signing via private message.

Note: Our CI does not get automatically triggered on the PRs from external contributors.
A member of our team will review the code and trigger the CI on demand by adding a comment on the PR (see [Azure Pipelines Comment triggers docs](https://docs.microsoft.com/en-us/azure/devops/pipelines/repos/github?view=azure-devops&tabs=yaml#comment-triggers)):
- `/azp run Sonar.Net` - It will run the full pipeline, including plugin tests and promotion

## Custom Rules

To request new rules, Contact us on [our Community Forum](https://community.sonarsource.com/c/suggestions/).

If you have an idea for a rule but you are not sure that everyone needs it, you can implement your own Roslyn analyzer.
- You can start with [this tutorial from Microsoft](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix) to write an analyzer.
- All Roslyn-based issues are picked up by the [SonarScanner for .NET](https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html)
and pushed to SonarQube as external issues.
- Also check out [SonarQube Roslyn SDK](https://github.com/SonarSource-VisualStudio/sonarqube-roslyn-sdk) to embed
your Roslyn analyzer in a SonarQube plugin, if you want to manage your rules from SonarQube.

## Configuring Rules

### SonarQube for IDE

The easiest way is to configure a Quality Profile in SonarQube. Use SonarQube for IDE Connected Mode to connect to SonarQube Server or Cloud.

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

## Security Issues

If you believe you have discovered a security vulnerability in Sonar's products, please check [this document](./SECURITY.md).

## License

Copyright SonarSource Sàrl.

Licensed under the [SONAR Source-Available License v1.0](https://www.sonarsource.com/license/ssal/)
