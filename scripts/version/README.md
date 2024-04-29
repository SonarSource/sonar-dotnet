## How sonar-dotnet version is set in our pipeline

### Introduction

During the pipeline run, we promote the following artifacts:

1. `sonar-csharp-plugin-x.y.z.buildNumber.jar`
1. `sonar-vbnet-plugin-x.y.z.buildNumber.jar`
1. `SonarAnalyzer.CSharp.x.y.z.buildNumber.nupkg`
1. `SonarAnalyzer.VisualBasic.x.y.z.buildNumber.nupkg`
1. `SonarAnalyzer.CSharp.Styling.x.y.z.buildNumber.nupkg`
1. `SonarAnalyzer.CFG.x.y.z.buildNumber.nupkg` (promoted only on repox)

There are two parts to the versioning of these artifacts (see also [Semantic Versioning 2.0.0](https://semver.org/)):
 - The semantic versioning `x.y.z`, that is set through the [`set-version.ps1`](./set-version.ps1) - see also [example PR](https://github.com/SonarSource/sonar-dotnet/pull/9194) to merge the changes done by the script.
 - The build metadata, `buildNumber` which is set through the [Azure pipeline](../../azure-pipelines.yml).

### 1. How version is set in *.jar artifacts

#### 1.1 Semantic version
The java artifacts' semantic version is updated by the [`set-version.ps1`](./set-version.ps1). The script updates the following files:
1.  [root pom.xml](../../pom.xml)
2.  [sonar-csharp-plugin pom.xml](../../sonar-csharp-plugin/pom.xml)
3.  [sonar-vbnet-plugin pom.xml](../../sonar-vbnet-plugin//pom.xml)
4.  [sonar-dotnet-shared-library pom.xml](../../sonar-dotnet-shared-library/pom.xml)
5.  [ITs pom.xml](../../its/pom.xml) (this project is not related to the artifacts, but the root pom is its parent pom and if there's no match the project build fails)

#### 1.2 Build metadata
The `buildNumber` is set in the sonar-dotnet [Azure pipeline](https://github.com/SonarSource/sonar-dotnet/blob/a998f32eac72c7b6b4562935ffb8d423c6ebf936/azure-pipelines.yml#L504) is and equal to the pipeline [`buildId`](https://learn.microsoft.com/en-us/azure/devops/pipelines/process/run-number?view=azure-devops&tabs=yaml#tokens). More specifically the pipeline is calling the [`update-maven-version-steps.yml`](https://dev.azure.com/sonarsource/DotNetTeam%20Project/_git/pipelines-yaml-templates?path=/update-maven-version-steps.yml&version=GBmaster&line=16&lineEnd=17&lineStartColumn=1&lineEndColumn=1&lineStyle=plain&_a=contentsl), which in turn calls the [`compute-build-version-step.yml`](https://dev.azure.com/sonarsource/DotNetTeam%20Project/_git/pipelines-yaml-templates?path=/compute-build-version-step.yml&version=GBmaster&line=29&lineEnd=30&lineStartColumn=1&lineEndColumn=1&lineStyle=plain&_a=contents) that attaches the `buildNumber` to the semantic version.

### 2. How is the version set for the for the NuGet and the *.dll artifacts

#### 2.1 Semantic version

The semantic version for these files is updated also by the [`set-version.ps1`](./set-version.ps1). The files updated are:
1. [Version.props](./Version.props)
1. [SonarAnalyzer.CSharp.nuspec](../../analyzers/packaging/SonarAnalyzer.CSharp.nuspec)
1. [SonarAnalyzer.VisualBasic.nuspec](../../analyzers/packaging/SonarAnalyzer.VisualBasic.nuspec)
1. [SonarAnalyzer.CFG.cs.nuspec](../../analyzers/src/SonarAnalyzer.CFG/SonarAnalyzer.CFG.cs.nuspec)
1. [SonarAnalyzer.CSharp.Styling.nuspec](../../analyzers/packaging/SonarAnalyzer.CSharp.Styling.nuspec)
1. [AssemblyInfo.Shared.cs](../../analyzers/src/AssemblyInfo.Shared.cs)

However, during artifact release, the files 2-6 above, have any reference to the semantic version replaced by the value in the [MainVersion field](https://github.com/SonarSource/sonar-dotnet/blob/a998f32eac72c7b6b4562935ffb8d423c6ebf936/scripts/version/Version.props#L3) of the `Version.props` file. This happens during the build of the [`ChangeVersion.proj`](./ChangeVersion.proj) that is [executed in the pipeline](https://github.com/SonarSource/sonar-dotnet/blob/master/azure-pipelines.yml#L91).

#### 2.2 Build metadata

The `builderNumber` is also set during the [build of the `ChangeVersion.proj`](https://github.com/SonarSource/sonar-dotnet/blob/a998f32eac72c7b6b4562935ffb8d423c6ebf936/azure-pipelines.yml#L92).


### 3. Notes

What is important to keep is that the versions in the pom files 1-4 mentioned in section 1.1, and the [MainVersion field](https://github.com/SonarSource/sonar-dotnet/blob/master/scripts/version/Version.props#L3) in the `Version.props` must
match. Otherwise, we'll have a version mismatch between the artifacts which will cause issues how the versions appear in SonarQube/SonarCloud and the S4NET logs.