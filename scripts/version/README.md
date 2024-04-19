## How sonar-dotnet version is set in our pipeline

During the pipeline run, we promote the following artifacts:

1. `sonar-csharp-plugin-x.y.z.buildNumber.jar`
1. `sonar-vbnet-plugin-x.y.z.buildNumber.jar`
1. `SonarAnalyzer.CSharp-x.y.z.nupkg`
1. `SonarAnalyzer.VisualBasic-x.y.z.nupkg`
1. `SonarAnalyzer.CFG-x.y.z.nupkg` (promoted only on repox)

### How is the version set for the *.jar artifacts

The Java artifacts' major, minor and, patch parts of the version, are set in the [sonar-dotnet pom.xml](https://github.com/SonarSource/sonar-dotnet/blob/master/pom.xml#L14).
The `buildNumber` is set in the Azure pipeline [here](https://github.com/SonarSource/sonar-dotnet/blob/master/azure-pipelines.yml#L538) with the use of the `update-maven-version-steps.yml` and it's equal to the pipeline [`buildId`](https://learn.microsoft.com/en-us/azure/devops/pipelines/process/run-number?view=azure-devops&tabs=yaml#tokens).

You can find the `update-maven-version-steps.yml` [here](https://dev.azure.com/sonarsource/DotNetTeam%20Project/_git/pipelines-yaml-templates?path=/update-maven-version-steps.yml).
The `buildNumber` is set on line 16 where the `compute-build-version-step.yml` is called, which you can find [here](https://dev.azure.com/sonarsource/DotNetTeam%20Project/_git/pipelines-yaml-templates?path=/compute-build-version-step.yml).

### How is the version set for the for the NuGet and the *.dll artifacts

The major, minor and, patch parts of the version for these files is taken from the [`MainVersion` field in the Version.props](https://github.com/SonarSource/sonar-dotnet/blob/master/scripts/version/Version.props#L3) file. 
The `buildNumber` is set from our pipeline configuration [here](https://github.com/SonarSource/sonar-dotnet/blob/master/azure-pipelines.yml#L92).

Finally, any version references in the following files:
- [NuGet package metadata for `SonarAnalyzer.CSharp-x.y.z.nupkg`](https://github.com/SonarSource/sonar-dotnet/blob/master/analyzers/packaging/SonarAnalyzer.CSharp.nuspec)
- [NuGet package metadata for `SonarAnalyzer.VisualBasic-x.y.z.nupkg`](https://github.com/SonarSource/sonar-dotnet/blob/master/analyzers/packaging/SonarAnalyzer.VisualBasic.nuspec)
- [NuGet package metadata for `SonarAnalyzer.CFG-x.y.z.nupkg`](https://github.com/SonarSource/sonar-dotnet/blob/master/analyzers/src/SonarAnalyzer.CFG/SonarAnalyzer.CFG.cs.nuspec)

is automatically updated through the [`ChangeVersion.proj`](https://github.com/SonarSource/sonar-dotnet/blob/master/scripts/version/ChangeVersion.proj) that is executed [here](https://github.com/SonarSource/sonar-dotnet/blob/master/azure-pipelines.yml#L91).



What is important to keep is that the versions in the [sonar-dotnet pom.xml](https://github.com/SonarSource/sonar-dotnet/blob/master/pom.xml#L14) and the the [`MainVersion` field in the Version.props](https://github.com/SonarSource/sonar-dotnet/blob/master/scripts/version/Version.props#L3) files must
match. Otherwise we'll have version mismatch between the artifacts which will cause issues to how the versions appear in SonarQube/SonarCloud and the S4NET logs.