# dotnet-test-sources
project sources for dotnet ITs


# To add a new project

- Clone the repo https://github.com/SonarSource/dotnet-test-sources.git
- Copy the project into the root folder of the repo
- Make sure all csproj files have <ProjectGuid>
- Add <CodeAnalysisRuleSet>..\..\..\..\ValidationRuleset.ruleset</CodeAnalysisRuleSet>
    - NOTE: The number of .. depends on what level are the csproj files:
        - ..\..\.. are needed when <repo>/ProjectRoot/Project/Project.csproj
        - ..\..\..\.. are needed when <repo>/ProjectRoot/src/Project/Project.csproj
