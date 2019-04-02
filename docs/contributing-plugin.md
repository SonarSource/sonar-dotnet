# Building, Testing and Debugging the SonarQube plugin

## Working with the code

1. Clone [this repository](https://github.com/SonarSource/sonar-dotnet.git)
1. Download sub-modules `git submodule update --init --recursive`
1. Build the plugin
    * To build the plugin while embedding a local build of the analyzer you can either:
        * run `.\scripts\build\dev-build.ps1 -restore -build -test -buildJava`

            The flags `-restore -build -test` need to be run only when you have changed the analyzer. Otherwise you can run only `-buildJava`

        * or run the following commands:
            1. `msbuild /t:rebuild .\sonaranalyzer-dotnet\SonarAnalyzer.sln`
            1. `.\sonaranalyzer-dotnet\src\SonarAnalyzer.RuleDescriptorGenerator\bin\Debug\net46\SonarAnalyzer.RuleDescriptorGenerator.exe cs`
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

Please see [Contributing Code](../CONTRIBUTING.md) for details on contributing changes back to the code.
