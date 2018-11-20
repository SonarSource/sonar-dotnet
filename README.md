# SonarC\#

[![Waffle.io - Columns and their card count](https://badge.waffle.io/SonarSource/sonar-dotnet.svg?columns=all)](https://waffle.io/SonarSource/sonar-dotnet?source=SonarSource%2Fsonar-dotnet)

[![Build Status](https://sonarsource.visualstudio.com/_apis/public/build/definitions/399fb241-ecc7-4802-8697-dcdd01fbb832/8/badge)](https://sonarsource.visualstudio.com/DotNetTeam%20Project/_build/index?definitionId=8)

|Product|Quality Gate|Coverage|
|:--:|:--:|:--:|
|Analyzer|[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=sonaranalyzer-dotnet&metric=alert_status)](https://sonarcloud.io/dashboard?id=sonaranalyzer-dotnet)|[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=sonaranalyzer-dotnet&metric=coverage)](https://sonarcloud.io/component_measures?id=sonaranalyzer-dotnet&metric=coverage)|
|Plugin|[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=org.sonarsource.dotnet%3Asonar-csharp&metric=alert_status)](https://sonarcloud.io/dashboard?id=org.sonarsource.dotnet%3Asonar-csharp)|[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=org.sonarsource.dotnet%3Asonar-csharp&metric=coverage)](https://sonarcloud.io/component_measures?id=org.sonarsource.dotnet%3Asonar-csharp&metric=coverage)|

|Product|Quality Gate|Coverage|
|:--:|:--:|:--:|
|Analyzer|[![Quality Gate](https://next.sonarqube.com/sonarqube/api/project_badges/measure?project=sonaranalyzer-csharp-vbnet&metric=alert_status)](https://next.sonarqube.com/sonarqube/dashboard?id=sonaranalyzer-csharp-vbnet)|[![Coverage](https://next.sonarqube.com/sonarqube/api/project_badges/measure?project=sonaranalyzer-csharp-vbnet&metric=coverage)](https://next.sonarqube.com/sonarqube/component_measures/domain/Coverage?id=sonaranalyzer-csharp-vbnet)|
|Plugin|[![Quality Gate](https://next.sonarqube.com/sonarqube/api/project_badges/measure?project=org.sonarsource.dotnet%3Asonar-csharp&metric=alert_status)](https://next.sonarqube.com/sonarqube/dashboard?id=org.sonarsource.dotnet%3Asonar-csharp)|[![Coverage](https://next.sonarqube.com/sonarqube/api/project_badges/measure?project=org.sonarsource.dotnet%3Asonar-csharp&metric=coverage)](https://next.sonarqube.com/sonarqube/component_measures/domain/Coverage?id=org.sonarsource.dotnet%3Asonar-csharp)|

SonarC# is a [static code analyser](https://en.wikipedia.org/wiki/Static_program_analysis) for C# language used as an
extension for the [SonarQube](http://www.sonarqube.org/) platform. It will allow you to produce stable and easily
supported code by helping you to find and to correct bugs, vulnerabilities and smells in your code.

## Features

* 340+ rules (including 60+ bug detection)
* Metrics (complexity, number of lines etc.)
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
