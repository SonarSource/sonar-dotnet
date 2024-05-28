# How sonar-dotnet version is set in our pipeline

## Introduction

During the pipeline run, we promote the following artifacts:

1. `sonar-csharp-plugin-x.y.z.buildNumber.jar`
1. `sonar-vbnet-plugin-x.y.z.buildNumber.jar`
1. `SonarAnalyzer.CSharp.x.y.z.buildNumber.nupkg`
1. `SonarAnalyzer.VisualBasic.x.y.z.buildNumber.nupkg`
1. `SonarAnalyzer.CSharp.Styling.x.y.z.buildNumber.nupkg`
1. `SonarAnalyzer.CFG.x.y.z.buildNumber.nupkg` (promoted only on repox)

There are two parts to the versioning of these artifacts (see also [Semantic Versioning 2.0.0](https://semver.org/)):
 - The semantic version `x.y.z`, that is set through the [`set-version.ps1`](./set-version.ps1) - see also [example PR](https://github.com/SonarSource/sonar-dotnet/pull/9194) and persisted in the repository.
 - The `buildNumber` which is updated in the [Azure pipeline](../../azure-pipelines.yml).

To set the version locally we run the [`set-version.ps1`](./set-version.ps1) script, which uses the `mvn set:version` command to update the java plugin's semantic version and the `ChangeVersion.proj` to update the analyzers' semantic version. Later, the `ChangeVersion.proj` and the `mvn set:version` (via the [`update-maven-version-steps.yml`](https://dev.azure.com/sonarsource/DotNetTeam%20Project/_git/pipelines-yaml-templates?path=/update-maven-version-steps.yml&version=GBmaster&line=20&lineEnd=21&lineStartColumn=1&lineEndColumn=1&lineStyle=plain&_a=contents)) are called in the pipeline to update the build version of the artifacts.

## How version is set in *.jar artifacts

### Semantic version
The java artifacts' semantic version is updated by the [`set-version.ps1`](./set-version.ps1). The script updates the following files:
1.  [root pom.xml](../../pom.xml)
2.  [sonar-csharp-plugin pom.xml](../../sonar-csharp-plugin/pom.xml)
3.  [sonar-vbnet-plugin pom.xml](../../sonar-vbnet-plugin//pom.xml)
4.  [sonar-dotnet-shared-library pom.xml](../../sonar-dotnet-shared-library/pom.xml)
5.  [its/pom.xml](../../its/pom.xml) (this project is not related to the artifacts, but the root pom is its parent pom and if there's no match the project build fails)

### Build metadata
The `buildNumber` is updated in the sonar-dotnet [Azure pipeline](https://github.com/SonarSource/sonar-dotnet/blob/a998f32eac72c7b6b4562935ffb8d423c6ebf936/azure-pipelines.yml#L504) is and equal to the pipeline [`buildId`](https://learn.microsoft.com/en-us/azure/devops/pipelines/process/run-number?view=azure-devops&tabs=yaml#tokens). More specifically the pipeline is calling the [`update-maven-version-steps.yml`](https://dev.azure.com/sonarsource/DotNetTeam%20Project/_git/pipelines-yaml-templates?path=/update-maven-version-steps.yml&version=GBmaster&line=16&lineEnd=17&lineStartColumn=1&lineEndColumn=1&lineStyle=plain&_a=contentsl), which in turn calls the [`compute-build-version-step.yml`](https://dev.azure.com/sonarsource/DotNetTeam%20Project/_git/pipelines-yaml-templates?path=/compute-build-version-step.yml&version=GBmaster&line=29&lineEnd=30&lineStartColumn=1&lineEndColumn=1&lineStyle=plain&_a=contents) that attaches the `buildNumber` to the semantic version.

## How is the version set for the for the NuGet and the *.dll artifacts

### Semantic version

The semantic version for these files is updated also by the [`set-version.ps1`](./set-version.ps1). The files updated are:
1. [Version.props](./Version.props)
1. [SonarAnalyzer.CSharp.nuspec](../../analyzers/packaging/SonarAnalyzer.CSharp.nuspec)
1. [SonarAnalyzer.VisualBasic.nuspec](../../analyzers/packaging/SonarAnalyzer.VisualBasic.nuspec)
1. [SonarAnalyzer.CFG.cs.nuspec](../../analyzers/src/SonarAnalyzer.CFG/SonarAnalyzer.CFG.cs.nuspec)
1. [SonarAnalyzer.CSharp.Styling.nuspec](../../analyzers/packaging/SonarAnalyzer.CSharp.Styling.nuspec)
1. [AssemblyInfo.Shared.cs](../../analyzers/src/AssemblyInfo.Shared.cs)

However, during the sonar-dotnet pipeline run, the files 2-6 above, have any reference to the semantic version replaced by the value in the [MainVersion](https://github.com/SonarSource/sonar-dotnet/blob/a998f32eac72c7b6b4562935ffb8d423c6ebf936/scripts/version/Version.props#L3) field of the `Version.props` file. This happens during the build of the [`ChangeVersion.proj`](./ChangeVersion.proj) that is [executed in the pipeline](https://github.com/SonarSource/sonar-dotnet/blob/d29aa1b43f6bf88f0e9448bb5ef104ecffd1e65d/azure-pipelines.yml#L91).
### Build metadata

The `builderNumber` is also updated by direct invocation of [ChangeVersion.proj](https://github.com/SonarSource/sonar-dotnet/blob/a998f32eac72c7b6b4562935ffb8d423c6ebf936/azure-pipelines.yml#L92).

## Notes

What is important to keep is that the versions in the root pom.xml, and the [MainVersion](https://github.com/SonarSource/sonar-dotnet/blob/master/scripts/version/Version.props#L3) field in the `Version.props` must
match. Otherwise, we'll have a version mismatch between the artifacts which will cause issues how the versions appear in SonarQube/SonarCloud and the S4NET logs.