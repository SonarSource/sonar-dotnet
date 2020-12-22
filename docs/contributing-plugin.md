# Building, Testing and Debugging the SonarQube plugin

## Setup your Java environment

- Install the Java Development Kit (JDK) version 11 or later (make sure it's an LTS version) - you can install it from [here](https://www.azul.com/downloads/zulu-community/?version=java-11-lts&os=windows&architecture=x86-64-bit&package=jdk)
- Install IntelliJ IDEA Community Edition
- Install Apache Maven 
- Setup the Maven Settings ([internal link](https://xtranet-sonarsource.atlassian.net/wiki/spaces/DEV/pages/776711/Developer+Box))

## Working with the code

1. Clone [this repository](https://github.com/SonarSource/sonar-dotnet.git)
1. Download sub-modules `git submodule update --init --recursive`
1. Build the plugin
    * To build the plugin while embedding a local build of the analyzer you can either:
        * run `.\scripts\build\dev-build.ps1 -build -test -buildJava`

            The flags `-restore -build -test` need to be run only when you have changed the analyzer. Otherwise you can run only `-buildJava`

        * or run the following commands:
            1. `msbuild /t:rebuild .\analyzers\SonarAnalyzer.sln`
            1. `mvn clean install -P local-analyzer -D analyzer.configuration=Debug`

    * To build the plugin while relying on a released analyzer run `mvn clean install -P download-analyzer -D analyzer.version=<VERSION>`
        * SonarSource developers can reference any built version
        * External users can rely only on public versions hosted in maven [org.sonarsource.dotnet:SonarAnalyzer.CSharp](https://mvnrepository.com/artifact/org.sonarsource.dotnet/SonarAnalyzer.CSharp)

## Developing with Eclipse or IntelliJ

When working with Eclipse or IntelliJ please follow the [sonar guidelines](https://github.com/SonarSource/sonar-developer-toolset)

## Running Tests

### Unit Tests

As for any maven project, the command `mvn clean install` automatically runs the unit tests.

### Integration Tests

Before running ITs, you need to setup the NUGET_PATH environment variable to point to the *nuget.exe* executable.

To run the ITs, from your command prompt, you can either:

* go to the `its` folder and run `mvn clean install`
* or run `.\scripts\build\dev-build.ps1 -itsJava`

## Contributing

Please see the [How to contribute](../README.md#how-to-contribute) section  for details on contributing changes back to the code.
