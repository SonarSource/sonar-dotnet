# SonarC\# and SonarVB

[![Waffle.io - Columns and their card count](https://badge.waffle.io/SonarSource/sonar-dotnet.svg?columns=all)](https://waffle.io/SonarSource/sonar-dotnet?source=SonarSource%2Fsonar-dotnet)

|Product|Quality Gate|Coverage|
|:--:|:--:|:--:|
|Analyzer|[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=sonaranalyzer-dotnet&metric=alert_status)](https://sonarcloud.io/dashboard?id=sonaranalyzer-dotnet)|[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=sonaranalyzer-dotnet&metric=coverage)](https://sonarcloud.io/component_measures?id=sonaranalyzer-dotnet&metric=coverage)|
|Plugin|[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=org.sonarsource.dotnet%3Asonar-csharp&metric=alert_status)](https://sonarcloud.io/dashboard?id=org.sonarsource.dotnet%3Asonar-csharp)|[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=org.sonarsource.dotnet%3Asonar-csharp&metric=coverage)](https://sonarcloud.io/component_measures?id=org.sonarsource.dotnet%3Asonar-csharp&metric=coverage)|

SonarC# and SonarVB are [static code analyser](https://en.wikipedia.org/wiki/Static_program_analysis) for C# and VB.&#8203;NET
languages used as an extension for the [SonarQube](http://www.sonarqube.org/) and [SonarCloud](https://sonarcloud.io)
platforms. It will allow you to produce stable and easily supported code by helping you to find and to correct bugs,
vulnerabilities and smells in your code.

## Features

* 350+ C# rules and 100+ VB.&#8203;NET rules
* Metrics (complexity, number of lines etc.)
* Import of [unit test results](https://docs.sonarqube.org/x/CoBh) from VSTest, MSTest, NUnit and xUnit
* Import of [test coverage reports](https://docs.sonarqube.org/x/CoBh) from Visual Studio Code Coverage, dotCover, OpenCover and NCover 3.
* Support for [custom rules](https://github.com/SonarSource/sonarqube-roslyn-sdk)

## Useful links

* [Project homepage](https://redirect.sonarsource.com/plugins/csharp.html)
* [Issue tracking](https://github.com/SonarSource/sonar-dotnet/issues)
* [C# rules](https://rules.sonarsource.com/csharp)
* [VB.Net rules](https://rules.sonarsource.com/vbnet)

## Have question or feedback?

To provide feedback (request a feature, report a bug etc.), simply
[create a GitHub Issue](https://github.com/SonarSource/sonar-dotnet/issues/new).

## Get started

* [Building, testing and debugging the plugin](./docs/contributing-plugin.md)
* [Building, testing and debugging the analyzer](./docs/contributing-analyzer.md)

## How to contribute

Check out the [contributing](CONTRIBUTING.md) page to see the best places to log issues and start discussions.

## License

Copyright 2014-2018 SonarSource.

Licensed under the [GNU Lesser General Public License, Version 3.0](http://www.gnu.org/licenses/lgpl.txt)
