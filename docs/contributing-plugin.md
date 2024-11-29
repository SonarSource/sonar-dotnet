# Building, Testing and Debugging the SonarQube plugin

## Setup your Java environment

- Install the Java Development Kit (JDK) version 11 or later (make sure it's an LTS version) - you can install it from [here](https://www.azul.com/downloads/zulu-community/?version=java-11-lts&os=windows&architecture=x86-64-bit&package=jdk)
- Install IntelliJ IDEA Community Edition
- Install Apache Maven 
- Setup the Maven Settings ([internal link](https://xtranet-sonarsource.atlassian.net/wiki/spaces/DEV/pages/776711/Developer+Box))
- Setup the Orchestrator ([internal link](https://github.com/sonarsource/orchestrator#configuration))

## Working with the code

1. Clone [this repository](https://github.com/SonarSource/sonar-dotnet.git)
1. Build the plugin
    1. `dotnet build .\analyzers\SonarAnalyzer.sln`
    1. `mvn clean install`

## Developing with Eclipse or IntelliJ

When working with Eclipse or IntelliJ please follow the [sonar guidelines](https://github.com/SonarSource/sonar-developer-toolset)

## Running Tests

As for any maven project, the command `mvn clean install` automatically runs the unit tests.

## Contributing

Please see the [How to contribute](../README.md#how-to-contribute) section  for details on contributing changes back to the code.
