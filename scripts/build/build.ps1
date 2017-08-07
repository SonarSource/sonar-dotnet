<#

.SYNOPSIS
This script controls the build process on the CI server.

#>

[CmdletBinding(PositionalBinding = $false)]
param (
    # GitHub related parameters
    [string]$githubRepo = $env:GITHUB_REPO,
    [string]$githubToken = $env:GITHUB_TOKEN,
    [string]$githubPullRequest = $env:PULL_REQUEST,
    [string]$githubIsPullRequest = $env:IS_PULLREQUEST,
    [string]$githubBranch = $env:GITHUB_BRANCH,
    [string]$githubSha1 = $env:GIT_SHA1,

    # SonarQube related parameters
    [string]$sonarQubeUrl = $env:SONAR_HOST_URL,
    [string]$sonarQubeToken = $env:SONAR_TOKEN,

    # Build related parameters
    [string]$buildNumber = $env:BUILD_NUMBER,
    [string]$certificatePath = $env:CERT_PATH,

    # Artifactory related parameters
    [string]$repoxUserName = $env:ARTIFACTORY_DEPLOY_USERNAME,
    [string]$repoxPassword = $env:ARTIFACTORY_DEPLOY_PASSWORD,

    [string]$appDataPath = $env:APPDATA
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

function Get-BranchName() {
    if ($githubBranch.StartsWith("refs/heads/")) {
        return $githubBranch.Substring(11)
    }

    return $githubBranch
}

function Clear-MSBuildImportBefore() {
    Get-ChildItem (Get-MSBuildImportBeforePath) -Recurse -Include "Sonar*.targets" `
        | ForEach-Object { Remove-Item -Force $_ }
}

function Get-DotNetVersion() {
    [xml]$versionProps = Get-Content "${PSScriptRoot}\..\version\Version.props"
    return $versionProps.Project.PropertyGroup.MainVersion + "." + $versionProps.Project.PropertyGroup.BuildNumber
}

function Set-DotNetVersion() {
    Write-Header "Updating version in .Net files..."

    $branchName = Get-BranchName

    Write-Host "Setting build number ${buildNumber}, sha1 ${githubSha1} and branch ${branchName}"

    $versionPropsPath = "${PSScriptRoot}\..\version\Version.props"

    (Get-Content $versionPropsPath) `
            -Replace '<Sha1>.*</Sha1>', "<Sha1>$githubSha1</Sha1>" `
            -Replace '<BuildNumber>\d+</BuildNumber>', "<BuildNumber>$buildNumber</BuildNumber>" `
            -Replace '<BranchName>.*</BranchName>', "<BranchName>$branchName</BranchName>" `
        | Set-Content $versionPropsPath

    Invoke-MSBuild "${PSScriptRoot}\..\version\ChangeVersion.proj"

    $version = Get-DotNetVersion
    Write-Host "Version successfully set to '${version}'"
}

function Get-ScannerMsBuildPath() {
    $currentDir = (Resolve-Path .\).Path
    $scannerMsbuild = Join-Path $currentDir "SonarQube.Scanner.MSBuild.exe"

    if (-Not (Test-Path $scannerMsbuild)) {
        # This links always redirect to the latest release scanner
        $downloadLink = "https://repox.sonarsource.com/sonarsource-public-releases/org/sonarsource/scanner/msbuild/" +
            "sonar-scanner-msbuild/%5BRELEASE%5D/sonar-scanner-msbuild-%5BRELEASE%5D.zip"
        $scannerMsbuildZip = Join-Path $currentDir "\MSBuild.SonarQube.Runner.zip"

        (New-Object System.Net.WebClient).DownloadFile($downloadLink, $scannerMsbuildZip)

        # perhaps we could use other folder, not the repository root
        Expand-ZIPFile $scannerMsbuildZip $currentDir

        Remove-Item $scannerMsbuildZip -Force
    }

    return $scannerMsbuild
}

function Invoke-SonarBeginAnalysis([array][parameter(ValueFromRemainingArguments = $true)]$remainingArgs) {
    Write-Header "Running SonarQube Analysis begin step..."

    $scannerMsbuildExe = Get-ScannerMsBuildPath
    & $scannerMsbuildExe begin `
        /k:"sonaranalyzer-csharp-vbnet" `
        /n:"SonarAnalyzer for C#" `
        /d:sonar.host.url=$sonarQubeUrl `
        /d:sonar.login=$sonarQubeToken `
        $remainingArgs
    Test-ExitCode "ERROR: SonarQube Analysis begin step FAILED."
}

function Invoke-SonarEndAnalysis() {
    Write-Header "Running SonarQube Analysis end step..."

    $scannerMsbuildExe = Get-ScannerMsBuildPath
    & $scannerMsbuildExe end /d:sonar.login=$sonarQubeToken
    Test-ExitCode "ERROR: SonarQube Analysis end step FAILED."
}

function Initialize-NuGetConfig() {
    Write-Header "Setting up nuget.config..."

    Remove-Item $appDataPath\NuGet\NuGet.Config

    $nuget_exe = Get-NuGetPath
    Exec { & $nuget_exe Sources Add -Name "repox" -Source "https://repox.sonarsource.com/api/nuget/sonarsource-nuget-qa" }
    Exec { & $nuget_exe SetApiKey "${repoxUserName}:${repoxPassword}" -Source repox }
}

function Publish-NuGetPackages {
    Write-Header "Publishing NuGet packages..."

    $nuget_exe = Get-NuGetPath
    foreach ($file in (Get-ChildItem "src" -Recurse "*.nupkg")) {
        Exec { & $nuget_exe push $file.FullName -Source "repox" }
    }
}

function Update-AnalyzerMavenArtifacts() {
    $version = Get-DotNetVersion
    Write-Header "Updating analyzer maven sub-modules..."

    Get-ChildItem "src" -Recurse "*.nupkg" | ForEach-Object {
        $packageId = ($_.Name -Replace $_.Extension, "") -Replace ".$version", ""
        $pomPath = ".\sonaranalyzer-maven-artifacts\${packageId}\pom.xml"

        Write-Host "Upating ${pomPath} artifact file with $_.FullName"
        (Get-Content $pomPath) -Replace "file-${packageId}", $_.FullName | Set-Content $pomPath
    }
}

function Initialize-QaStep() {
    Write-Header "Queueing QA job..."

    $versionPropertiesPath = ".\version.properties"
    $version = Get-DotNetVersion

    "VERSION=${version}" | Out-File -Encoding utf8 -Append $versionPropertiesPath
    ConvertTo-UnixLineEndings $versionPropertiesPath

    $content = Get-Content $versionPropertiesPath
    Write-Host "Successfully created version.properties with content '${content}'"
}

function Invoke-DotNetBuild() {
    Set-DotNetVersion

    $skippedAnalysis = $false
    if ($isPullRequest) {
        Invoke-SonarBeginAnalysis `
            /d:sonar.github.pullRequest=$githubPullRequest `
            /d:sonar.github.repository=$githubRepo `
            /d:sonar.github.oauth=$githubToken `
            /d:sonar.analysis.mode="issues" `
            /d:sonar.scanAllFiles="true" `
            /v:"latest"
    }
    elseif ($isMaster) {
        Invoke-SonarBeginAnalysis `
            /v:"master" `
            /d:sonar.cs.vstest.reportsPaths="**\*.trx" `
            /d:sonar.cs.vscoveragexml.reportsPaths="**\*.coveragexml"
    }
    else {
        $skippedAnalysis = $true
    }

    Restore-Packages $solutionName
    Invoke-MSBuild $solutionName `
        /v:q `
        /consoleloggerparameters:Summary `
        /m `
        /p:configuration=$buildConfiguration `
        /p:DeployExtension=false `
        /p:ZipPackageCompressionLevel=normal `
        /p:defineConstants=SignAssembly `
        /p:SignAssembly=true `
        /p:AssemblyOriginatorKeyFile=$certificatePath

    Invoke-UnitTests $binPath
    Invoke-CodeCoverage

    if (-Not $skippedAnalysis) {
        Invoke-SonarEndAnalysis
    }

    New-Metadata $binPath
    New-NuGetPackages $binPath

    Initialize-NuGetConfig
    Publish-NuGetPackages
    Update-AnalyzerMavenArtifacts
}

function Get-MavenExpression([string]$exp) {
    $out = mvn help:evaluate -B -Dexpression="${exp}"
    Test-ExitCode "ERROR: Evaluation of expression ${exp} FAILED."

    return ($out | Select-String -NotMatch -Pattern '^\[|Download\w\+\:').Line
}

function Set-MavenBuildVersion() {
    $currentVersion = Get-MavenExpression "project.version"
    $releaseVersion = $currentVersion.Split("-").Get(0)

    # In case of 2 digits, we need to add the 3rd digit (0 obviously)
    # Mandatory in order to compare versions (patch VS non patch)
    $digitCount = $releaseVersion.Split(".").Count
    if ($digitCount -lt 3) {
        $releaseVersion = "$releaseVersion.0"
    }
    $newVersion = "$releaseVersion.$buildNumber"

    Write-Host "Replacing version $currentVersion with $newVersion"

    mvn org.codehaus.mojo:versions-maven-plugin:2.2:set "-DnewVersion=$newVersion" -DgenerateBackupPoms=false -B -e
    Test-ExitCode "ERROR: Maven set version FAILED."

    # Set the version used by Jenkins to associate artifacts to the right version
    $env:PROJECT_VERSION = $newVersion
}

function Invoke-JavaBuild() {
    # Remove specific env variables so qgate is not displayed for java (we only want qgate for sonaranalyzer as long
    # as only one qgate can be shown in burgr)
    if (Test-Path Env:\CI_BUILD_NUMBER) {
        Remove-Item Env:\CI_BUILD_NUMBER
    }
    if (Test-Path Env:\CI_PRODUCT) {
        Remove-Item Env:\CI_PRODUCT
    }

    if ($isMaster -And -Not $isPullRequest) {
        Write-Header "Building, deploying and analyzing SonarC#..."

        $currentVersion = Get-MavenExpression "project.version"
        Set-MavenBuildVersion

        $env:MAVEN_OPTS = "-Xmx1536m -Xms128m"

        & mvn org.jacoco:jacoco-maven-plugin:prepare-agent deploy sonar:sonar `
            "-Pcoverage-per-test,deploy-sonarsource,release,sonaranalyzer" `
            "-Dmaven.test.redirectTestOutputToFile=false" `
            "-Dsonar.host.url=${sonarQubeUrl}" `
            "-Dsonar.login=${sonarQubeToken}" `
            "-Dsonar.projectVersion=${currentVersion}" `
            -B -e -V
        Test-ExitCode "ERROR: Maven build deploy sonar FAILED."
    }
    elseif ($env:IS_PULLREQUEST -eq "true" -and $githubToken -ne $null) {
        Write-Header "Building and analyzing SonarC#..."

        # Do not deploy a SNAPSHOT version but the release version related to this build and PR
        Set-MavenBuildVersion

        $env:MAVEN_OPTS = "-Xmx1G -Xms128m"

        # No need for Maven phase "install" as the generated JAR files do not need to be installed
        # in Maven local repository. Phase "verify" is enough.
        Write-Host "SonarC# will be deployed"

        & mvn org.jacoco:jacoco-maven-plugin:prepare-agent deploy sonar:sonar `
            "-Pdeploy-sonarsource,sonaranalyzer" `
            "-Dmaven.test.redirectTestOutputToFile=false" `
            "-Dsonar.analysis.mode=issues" `
            "-Dsonar.github.pullRequest=$githubPullRequest" `
            "-Dsonar.github.repository=$githubRepo" `
            "-Dsonar.github.oauth=$githubToken" `
            "-Dsonar.host.url=$sonarQubeUrl" `
            "-Dsonar.login=$sonarQubeToken" `
            -B -e -V
        Test-ExitCode "ERROR: Maven build deploy sonar FAILED."
    }
    else {
        Write-Header "Building SonarC#..."

        Set-MavenBuildVersion

        # No need for Maven phase "install" as the generated JAR files do not need to be installed
        # in Maven local repository. Phase "verify" is enough.
        & mvn verify "-Dmaven.test.redirectTestOutputToFile=false" -B -e -V
        Test-ExitCode "ERROR: Maven verify FAILED."
    }
}

try {
    . (Join-Path $PSScriptRoot "build-utils.ps1")

    $buildConfiguration = "Release"
    $binPath = "bin\${buildConfiguration}"
    $solutionName = "SonarAnalyzer.sln"
    $branchName = Get-BranchName
    $isMaster = $branchName -Eq "master"
    $isPullRequest = $githubIsPullRequest -Eq "true"

    Write-Host "Solution to build: $solutionName"
    Write-Host "Build configuration: $buildConfiguration"
    Write-Host "Bin folder to use: $binPath"
    Write-Host "Branch: $branchName"
    if ($isPullRequest) {
        Write-Host "PR: $githubPullRequest"
    }

    # Ensure the ImportBefore folder does not contain our targets
    Clear-MSBuildImportBefore

    Invoke-InLocation "${PSScriptRoot}\..\..\sonaranalyzer-dotnet" {
        Invoke-DotNetBuild
    }

    Invoke-InLocation "${PSScriptRoot}\..\.." {
        Invoke-JavaBuild
    }

    if ($isPullRequest -Or $isMaster) {
        Invoke-InLocation "${PSScriptRoot}\..\..\sonaranalyzer-dotnet" { Initialize-QaStep }
    }

    exit 0
}
catch {
    Write-Host -ForegroundColor Red $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}