[CmdletBinding(PositionalBinding = $false)]
param (
    [switch]$analyze = $false,
    [switch]$test = $false,
    [switch]$package = $false,
    [switch]$debugBuild = $false,

    [string]$githubRepo,
    [string]$githubToken,
    [string]$githubPullRequest,

    [string]$isPullRequest,

    [string]$sonarQubeProjectName = "SonarAnalyzer for C#",
    [string]$sonarQubeProjectKey = "sonaranalyzer-csharp-vbnet",
    [string]$sonarQubeUrl = "http://localhost:9000",
    [string]$sonarQubeToken = $null,

    [string]$certificatePath,

    [string]$artifactoryUrl = "https://repox.sonarsource.com/api/nuget",
    [string]$artifactoryUser = $env:ARTIFACTORY_DEPLOY_USERNAME,
    [string]$artifactoryPass = $env:ARTIFACTORY_DEPLOY_PASSWORD,
    [parameter(ValueFromRemainingArguments = $true)] $badArgs)

$ErrorActionPreference = "Stop"

if ($badArgs -Ne $null) {
    throw "Bad arguments: $badArgs"
}

. (Join-Path $PSScriptRoot "build-utils.ps1")

function Queue-QaBuild() {
    Write-Header "Queueing QA job..."

    $versionPropertiesPath = (Resolve-RepoPath "version.properties")

    $version = Get-Version

    "VERSION=${version}" `
        | Out-File -Encoding utf8 -Append $versionPropertiesPath

    To-UnixLineEndings $versionPropertiesPath

    $content = Get-Content $versionPropertiesPath
    Write-Host "version.properties content is '{$content}'"
}

Set-Version

Setup-NugetConfig $artifactoryUrl $artifactoryUser $artifactoryPass

$branchName = Get-BranchName

Write-Header "Temporary info Analyze=${analyze} Branch=${branchName} PR=${isPullRequest}"

$isMaster = $branchName -Eq "master"
$skippedAnalysis = $false

if ($analyze -And $isPullRequest -Eq "true") {
    Write-Host "Pull request '${githubPullRequest}'"

    Begin-Analysis $sonarQubeUrl $sonarQubeToken $sonarQubeProjectKey $sonarQubeProjectName `
            /d:sonar.github.pullRequest=$githubPullRequest `
            /d:sonar.github.repository=$githubRepo `
            /d:sonar.github.oauth=$githubToken `
            /d:sonar.analysis.mode="issues" `
            /d:sonar.scanAllFiles="true" `
            /v:"latest"
}
elseif ($analyze -And $isMaster) {
    Write-Host "Is master '${isMaster}'"

    Begin-Analysis $sonarQubeUrl $sonarQubeToken $sonarQubeProjectKey $sonarQubeProjectName `
            /v:"master"
}
else {
    $skippedAnalysis = $true
}

if ($debugBuild) {
    Build-Solution (Resolve-RepoPath ".\SonarAnalyzer.sln")
}
else {
    Build-ReleaseSolution (Resolve-RepoPath ".\SonarAnalyzer.sln") $certificatePath
}

if ($test) {
    Run-Tests
}

if (-Not $skippedAnalysis) {
    End-Analysis $sonarQubeToken
}

if ($package) {
    Generate-Metadata
    Pack-Nugets
    Publish-Packages
    Update-MavenArtifacts (Get-Version)
}

if ($isPullRequest -Eq "true" -Or $isMaster) {
    Queue-QaBuild
}
