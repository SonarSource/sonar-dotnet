# SonarC\# and SonarVB

[![Build Status](https://sonarsource.visualstudio.com/DotNetTeam%20Project/_apis/build/status/Sonar.Net?branchName=master)](https://sonarsource.visualstudio.com/DotNetTeam%20Project/_build/latest?definitionId=77&branchName=master)

|Product|Quality Gate|Coverage|
|:--:|:--:|:--:|
|Analyzer|[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=sonaranalyzer-dotnet&metric=alert_status)](https://sonarcloud.io/dashboard?id=sonaranalyzer-dotnet)|[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=sonaranalyzer-dotnet&metric=coverage)](https://sonarcloud.io/component_measures?id=sonaranalyzer-dotnet&metric=coverage)|
|Plugin|[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=org.sonarsource.dotnet%3Asonar-csharp&metric=alert_status)](https://sonarcloud.io/dashboard?id=org.sonarsource.dotnet%3Asonar-csharp)|[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=org.sonarsource.dotnet%3Asonar-csharp&metric=coverage)](https://sonarcloud.io/component_measures?id=org.sonarsource.dotnet%3Asonar-csharp&metric=coverage)|

SonarC# and SonarVB are [static code analyzers](https://en.wikipedia.org/wiki/Static_program_analysis) for the C# and VB.&#8203;NET
languages and are part of the [SonarQube](http://www.sonarqube.org/), [SonarCloud](https://sonarcloud.io) and [SonarLint](https://www.sonarlint.org/) code quality and security products. These analyzers allow you to produce safe, reliable and maintainable code by helping you find and correct bugs, vulnerabilities and code smells in your codebase.

## Features

* 380+ C# rules and 130+ VB.&#8203;NET rules
* Metrics (complexity, duplications, number of lines etc.)
* Import of [unit test results](https://docs.sonarqube.org/x/CoBh) from VSTest, MSTest, NUnit and xUnit
* Import of [test coverage reports](https://docs.sonarqube.org/x/CoBh) from Visual Studio Code Coverage, dotCover, OpenCover and NCover 3.
* Support for [custom rules](https://github.com/SonarSource/sonarqube-roslyn-sdk)

## Useful links

* [Project homepage](https://redirect.sonarsource.com/plugins/csharp.html)
* [Issue tracking](./docs/issues.md)
* [C# rules](https://rules.sonarsource.com/csharp)
* [VB.Net rules](https://rules.sonarsource.com/vbnet)

### Nuget.org packages

* [SonarAnalyzer.CSharp](https://www.nuget.org/packages/SonarAnalyzer.CSharp/)
* [SonarAnalyzer.VisualBasic](https://www.nuget.org/packages/SonarAnalyzer.VisualBasic/)

### Integration with SonarQube and SonarCloud

* [Analyze projects with Scanner for MSBuild](https://docs.sonarqube.org/latest/analysis/scan/sonarscanner-for-msbuild/)
* [Importing code coverage](https://community.sonarsource.com/t/coverage-test-data-generate-reports-for-c-vb-net/9871)
* [SonarQube and the code coverage](https://community.sonarsource.com/t/sonarqube-and-the-code-coverage/4725)

## Do you have a question or feedback?

* [Contact us on Community Forum](https://community.sonarsource.com/) to provide feedback, ask for help, request new rules or features.
* [Create a GitHub Issue](https://github.com/SonarSource/sonar-dotnet/issues/new/choose) if you've found a bug, False-Positive or False-Negative. 

## Get started

* [Building, testing and debugging the Java plugin](./docs/contributing-plugin.md)
* [Building, testing and debugging the .NET analyzer](./docs/contributing-analyzer.md)
* [Using the rspec.ps1 script](./scripts/rspec/README.md)

## How to contribute

Check out the [contributing](CONTRIBUTING.md) page to see the best places to log issues and start discussions.

## License

Copyright 2014-2020 SonarSource.

Licensed under the [GNU Lesser General Public License, Version 3.0](http://www.gnu.org/licenses/lgpl.txt)
