# Important

Projects are added by hand in this folder
Historically, these projects were in https://github.com/SonarSource/dotnet-test-sources.git and used as git submodule , but PR #2075 copied the projects directly.

# Changes since the the copy

- add more projects for testing the analysis of generated code
- remove the F# projects from the Akka solution so that the build won't need F# dependency

# Historical repo details

- Clone the repo 
- Copy the project into the root folder of the repo
- Make sure all csproj files have <ProjectGuid>
- Add <CodeAnalysisRuleSet>..\..\..\..\ValidationRuleset.ruleset</CodeAnalysisRuleSet>
    - NOTE: The number of .. depends on what level are the csproj files:
        - ..\..\.. are needed when <repo>/ProjectRoot/Project/Project.csproj
        - ..\..\..\.. are needed when <repo>/ProjectRoot/src/Project/Project.csproj