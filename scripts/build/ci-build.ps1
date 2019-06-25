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
    # GitHub PR related parameters
    [string]$githubPRBaseBranch = $env:GITHUB_BASE_BRANCH,
    [string]$githubPRTargetBranch = $env:GITHUB_TARGET_BRANCH,

    # SonarCloud related parameters
    [string]$sonarCloudUrl = $env:SONARCLOUD_HOST_URL,
    [string]$sonarCloudToken = $env:SONARCLOUD_TOKEN,

    # Build related parameters
    [string]$buildNumber = $env:BUILD_NUMBER,
    [string]$certificatePath = $env:CERT_PATH,

    # Artifactory related parameters
    [string]$repoxUserName = $env:ARTIFACTORY_DEPLOY_USERNAME,
    [string]$repoxPassword = $env:ARTIFACTORY_DEPLOY_PASSWORD,

    # Others
    [string]$appDataPath = $env:APPDATA
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

if ($PSBoundParameters['Verbose'] -Or $PSBoundParameters['Debug']) {
    $global:DebugPreference = "Continue"
}

function Get-BranchName() {
    if ($githubBranch.StartsWith("refs/heads/")) {
        return $githubBranch.Substring(11)
    }

    return $githubBranch
}

function Clear-MSBuildImportBefore() {
    $importBeforePath = Get-MSBuildImportBeforePath "15.0"
    Get-ChildItem $importBeforePath -Recurse -Include "Sonar*.targets" `
        | ForEach-Object {
            Write-Debug "Removing $_"
            Remove-Item -Force $_
        }
}

function Get-DotNetVersion() {
    [xml]$versionProps = Get-Content "${PSScriptRoot}\..\version\Version.props"
    $fullVersion = $versionProps.Project.PropertyGroup.MainVersion + "." + $versionProps.Project.PropertyGroup.BuildNumber

    Write-Debug ".Net version is '${fullVersion}'"

    return $fullVersion
}

function Get-LeakPeriodVersion() {
    [xml]$versionProps = Get-Content "${PSScriptRoot}\..\version\Version.props"
    $mainVersion = $versionProps.Project.PropertyGroup.MainVersion

    Write-Debug "Leak period version is '${mainVersion}'"

    return $mainVersion
}

function Set-DotNetVersion() {
    Write-Header "Updating version in .Net files"

    $branchName = Get-BranchName
    Write-Debug "Setting build number ${buildNumber}, sha1 ${githubSha1} and branch ${branchName}"

    Invoke-InLocation (Join-Path $PSScriptRoot "..\version") {
        $versionProperties = "Version.props"
        (Get-Content $versionProperties) `
                -Replace '<Sha1>.*</Sha1>', "<Sha1>$githubSha1</Sha1>" `
                -Replace '<BuildNumber>\d+</BuildNumber>', "<BuildNumber>$buildNumber</BuildNumber>" `
                -Replace '<BranchName>.*</BranchName>', "<BranchName>$branchName</BranchName>" `
            | Set-Content $versionProperties

        Invoke-MSBuild "15.0" "ChangeVersion.proj"

        $version = Get-DotNetVersion
        Write-Host "Version successfully set to '${version}'"
    }
}

function Get-ScannerMsBuildPath() {
    $currentDir = (Resolve-Path .\).Path
    $scannerMsbuild = Join-Path $currentDir "SonarScanner.MSBuild.exe"

    if (-Not (Test-Path $scannerMsbuild)) {
        Write-Host "Scanner for MSBuild not found, downloading it"

        if ($env:ARTIFACTORY_URL)
        {
            Write-Host "Environment variable ARTIFACTORY_URL = $env:ARTIFACTORY_URL"
        }
        else
        {
            # We want this to be a terminating error so we throw
            Throw "Environment variable ARTIFACTORY_URL is not set"
        }

        # This links always redirect to the latest released scanner
        $downloadLink = "$env:ARTIFACTORY_URL/sonarsource-public-releases/org/sonarsource/scanner/msbuild/" +
            "sonar-scanner-msbuild/%5BRELEASE%5D/sonar-scanner-msbuild-%5BRELEASE%5D-net46.zip"
        $scannerMsbuildZip = Join-Path $currentDir "\SonarScanner.MSBuild.zip"

        Write-Host "Scanner for MSBuild not found, downloading it" "Downloading scanner from '${downloadLink}' at '${currentDir}'"

        # NB: the WebClient class defaults to TLS v1, which is no longer supported by GitHub/Artifactory online
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
        (New-Object System.Net.WebClient).DownloadFile($downloadLink, $scannerMsbuildZip)

        # perhaps we could use other folder, not the repository root
        Expand-ZIPFile $scannerMsbuildZip $currentDir

        Write-Host "Deleting downloaded zip"
        Remove-Item $scannerMsbuildZip -Force
    }

    Write-Host "Scanner for MSBuild found at '$scannerMsbuild'"
    return $scannerMsbuild
}

function Invoke-SonarBeginAnalysis([array][parameter(ValueFromRemainingArguments = $true)]$remainingArgs) {
    Write-Header "Running SonarCloud Analysis begin step"

    if (Test-Debug) {
        $remainingArgs += "/d:sonar.verbose=true"
    }

    Exec { & (Get-ScannerMsBuildPath) begin `
        /k:sonaranalyzer-dotnet `
        /n:"SonarAnalyzer .Net" `
        /d:sonar.host.url=${sonarCloudUrl} `
        /d:sonar.login=$sonarCloudToken `
        /o:sonarsource `
        /d:sonar.cs.vstest.reportsPaths="**\*.trx" `
        /d:sonar.cs.vscoveragexml.reportsPaths="**\*.coveragexml" `
        $remainingArgs `
    } -errorMessage "ERROR: SonarCloud Analysis begin step FAILED."
}

function Invoke-SonarEndAnalysis() {
    Write-Header "Running SonarCloud Analysis end step"

    Exec { & (Get-ScannerMsBuildPath) end `
        /d:sonar.login=$sonarCloudToken `
    } -errorMessage "ERROR: SonarCloud Analysis end step FAILED."
}

function Initialize-NuGetConfig() {
    Write-Header "Setting up nuget.config"

    $nugetFile = "${appDataPath}\NuGet\NuGet.Config"
    Write-Debug "Deleting '${nugetFile}'"
    Remove-Item $nugetFile

    $nugetExe = Get-NuGetPath
    Write-Debug "Adding repox source to NuGet config"
    Write-Debug "ARTIFACTORY_URL: $env:ARTIFACTORY_URL"
    Exec { & $nugetExe Sources Add -Name "repox" -Source "$env:ARTIFACTORY_URL/api/nuget/sonarsource-nuget-qa" }

    Write-Debug "Adding repox API key to NuGet config"
    Exec { & $nugetExe SetApiKey "${repoxUserName}:${repoxPassword}" -Source "repox" }
}

function Publish-NuGetPackages {
    Write-Header "Publishing NuGet packages"

    $nugetExe = Get-NuGetPath
    $extraArgs = if (Test-Debug) { "-Verbosity detailed" } else { }

    foreach ($file in (Get-ChildItem "src" -Recurse "*.nupkg")) {
        Write-Debug "Pushing NuGet package '${file}' to repox"
        Exec { & $nugetExe push $file.FullName -Source repox $extraArgs }
    }
}

function Update-AnalyzerMavenArtifacts() {
    $version = Get-DotNetVersion
    Write-Header "Updating analyzer maven sub-modules"

    Get-ChildItem "src" -Recurse "*.nupkg" | ForEach-Object {
        $packageId = ($_.Name -Replace $_.Extension, "") -Replace ".$version", ""
        $pomPath = ".\sonaranalyzer-maven-artifacts\${packageId}\pom.xml"

        Write-Debug "Updating ${pomPath} artifact file with $_"
        (Get-Content $pomPath) -Replace "file-${packageId}", $_.FullName | Set-Content $pomPath
    }
}

function Initialize-QaStep() {
    Write-Header "Queueing QA job"

    $versionPropertiesPath = ".\version.properties"
    $version = Get-DotNetVersion

    "VERSION=${version}" | Out-File -Encoding utf8 -Append $versionPropertiesPath
    ConvertTo-UnixLineEndings $versionPropertiesPath

    $content = Get-Content $versionPropertiesPath
    Write-Debug "Successfully created version.properties with content '${content}'"
    Write-Host "Triggering QA job"
}

function Invoke-DotNetBuild() {
    Set-DotNetVersion

    $skippedAnalysis = $false
    $leakPeriodVersion = Get-LeakPeriodVersion

    if ($isPullRequest) {
        Invoke-SonarBeginAnalysis `
            /d:sonar.analysis.prNumber=$githubPullRequest `
            /d:sonar.analysis.sha1=$githubSha1 `
            /d:sonar.pullrequest.key=$githubPullRequest `
            /d:sonar.pullrequest.branch=$githubPRBaseBranch `
            /d:sonar.pullrequest.base=$githubPRTargetBranch `
            /d:sonar.pullrequest.provider=github `
            /d:sonar.pullrequest.github.repository=$githubRepo `
            /v:$leakPeriodVersion
    }
    elseif ($isMaster) {
        Invoke-SonarBeginAnalysis `
            /v:$leakPeriodVersion `
            /d:sonar.analysis.buildNumber=$buildNumber `
            /d:sonar.analysis.pipeline=$buildNumber `
            /d:sonar.analysis.sha1=$githubSha1 `
            /d:sonar.analysis.repository=$githubRepo
    }
    elseif ($isMaintenanceBranch -or $isFeatureBranch) {
        Invoke-SonarBeginAnalysis `
            /v:$leakPeriodVersion `
            /d:sonar.analysis.buildNumber=$buildNumber `
            /d:sonar.analysis.pipeline=$buildNumber `
            /d:sonar.analysis.sha1=$githubSha1 `
            /d:sonar.analysis.repository=$githubRepo `
            /d:sonar.branch.name=$branchName
    }
    else {
        $skippedAnalysis = $true
    }

    Restore-Packages "15.0" $solutionName
    Invoke-MSBuild "15.0" $solutionName `
        /bl:"${binPath}\msbuild.binlog" `
        /consoleloggerparameters:Summary `
        /m `
        /p:configuration=$buildConfiguration `
        /p:DeployExtension=false `
        /p:ZipPackageCompressionLevel=normal `
        /p:defineConstants="SignAssembly" `
        /p:SignAssembly=true `
        /p:AssemblyOriginatorKeyFile=$certificatePath

    Invoke-UnitTests $binPath $true
    Invoke-CodeCoverage

    if (-Not $skippedAnalysis) {
        Invoke-SonarEndAnalysis
    }

    New-NuGetPackages $binPath

    Initialize-NuGetConfig
    Publish-NuGetPackages
    Update-AnalyzerMavenArtifacts
}

function Get-MavenExpression([string]$exp) {
    $out = Exec { mvn help:evaluate -B -Dexpression="${exp}" }
    Test-ExitCode "ERROR: Evaluation of expression ${exp} FAILED."

    return $out | Select-String -NotMatch -Pattern '^\[|Download\w\+\:' | Select-Object -First 1 -ExpandProperty Line
}

function Set-MavenBuildVersion() {
    $currentVersion = Get-MavenExpression "project.version"
    $releaseVersion = $currentVersion.Split("-").Get(0)

    # In case of 2 digits, we need to add the 3rd digit (0 obviously)
    # Mandatory in order to compare versions (patch VS non patch)
    $digitCount = $releaseVersion.Split(".").Count
    if ($digitCount -lt 3) {
        $releaseVersion = "${releaseVersion}.0"
    }
    $newVersion = "${releaseVersion}.${buildNumber}"

    Write-Host "Replacing version ${currentVersion} with ${newVersion}"

    Exec { & mvn org.codehaus.mojo:versions-maven-plugin:2.2:set "-DnewVersion=${newVersion}" `
        -DgenerateBackupPoms=false -B -e `
    } -errorMessage "ERROR: Maven set version FAILED."

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

    $sonarqubePluginName = "SonarC# and SonarVB plugins"

    if ($isPullRequest) {
        Write-Header "Building and analyzing SonarC# and SonarVB for PR" $githubPullRequest

        # Do not deploy a SNAPSHOT version but the release version related to this build and PR
        Set-MavenBuildVersion

        $env:MAVEN_OPTS = "-Xmx1G -Xms128m"

        # No need for Maven phase "install" as the generated JAR files do not need to be installed
        # in Maven local repository. Phase "verify" is enough.
        Write-Host "SonarC# and SonarVB will be deployed"

        # hardcode a PR number so that the PR decoration doesn't find the right PR
        Exec { & mvn org.jacoco:jacoco-maven-plugin:prepare-agent deploy sonar:sonar `
            "-Pcoverage,deploy-sonarsource,release,sonaranalyzer" `
            "-Dmaven.test.redirectTestOutputToFile=false" `
            "-Dsonar.projectName=${sonarqubePluginName}" `
            "-Dsonar.analysis.buildNumber=${buildNumber}" `
            "-Dsonar.analysis.pipeline=${buildNumber}" `
            "-Dsonar.analysis.sha1=${githubSha1}" `
            "-Dsonar.analysis.repository=${githubRepo}" `
            "-Dsonar.analysis.prNumber=${githubPullRequest}" `
            "-Dsonar.host.url=${sonarCloudUrl}" `
            "-Dsonar.login=${sonarCloudToken}" `
            "-Dsonar.organization=sonarsource" `
            "-Dsonar.pullrequest.key=0" `
            "-Dsonar.pullrequest.branch=${githubPRBaseBranch}" `
            "-Dsonar.pullrequest.base=${githubPRTargetBranch}" `
            "-Dsonar.pullrequest.provider=github" `
            "-Dsonar.pullrequest.github.repository=${githubRepo}" `
            -B -e -V `
        } -errorMessage "ERROR: Maven build deploy sonar FAILED."
    }
    elseif ($isMaster) {
        Write-Header "Building, deploying and analyzing SonarC# and SonarVB for master"

        $currentVersion = Get-MavenExpression "project.version"
        Set-MavenBuildVersion

        $env:MAVEN_OPTS = "-Xmx1536m -Xms128m"

        Exec { & mvn org.jacoco:jacoco-maven-plugin:prepare-agent deploy sonar:sonar `
            "-Pcoverage,deploy-sonarsource,release,sonaranalyzer" `
            "-Dmaven.test.redirectTestOutputToFile=false" `
            "-Dsonar.projectName=${sonarqubePluginName}" `
            "-Dsonar.analysis.buildNumber=${buildNumber}" `
            "-Dsonar.analysis.pipeline=${buildNumber}" `
            "-Dsonar.analysis.sha1=${githubSha1}" `
            "-Dsonar.analysis.repository=${githubRepo}" `
            "-Dsonar.host.url=${sonarCloudUrl}" `
            "-Dsonar.login=${sonarCloudToken}" `
            "-Dsonar.organization=sonarsource" `
            "-Dsonar.projectVersion=${currentVersion}" `
            -B -e -V `
        } -errorMessage "ERROR: Maven build deploy sonar FAILED."
    }
    elseif ($isMaintenanceBranch -or $isFeatureBranch) {
        Write-Header "Building and analyzing SonarC# and SonarVB for maitenance/feature branch" $branchName

        # Do not deploy a SNAPSHOT version but the release version related to this build and PR
        $currentVersion = Get-MavenExpression "project.version"
        Set-MavenBuildVersion

        $env:MAVEN_OPTS = "-Xmx1G -Xms128m"

        # No need for Maven phase "install" as the generated JAR files do not need to be installed
        # in Maven local repository. Phase "verify" is enough.
        Write-Host "SonarC# and SonarVB will be deployed"

        Exec { & mvn org.jacoco:jacoco-maven-plugin:prepare-agent deploy sonar:sonar `
            "-Pcoverage,deploy-sonarsource,release,sonaranalyzer" `
            "-Dmaven.test.redirectTestOutputToFile=false" `
            "-Dsonar.projectName=${sonarqubePluginName}" `
            "-Dsonar.analysis.buildNumber=${buildNumber}" `
            "-Dsonar.analysis.pipeline=${buildNumber}" `
            "-Dsonar.analysis.sha1=${githubSha1}" `
            "-Dsonar.analysis.repository=${githubRepo}" `
            "-Dsonar.branch.name=${branchName}" `
            "-Dsonar.host.url=${sonarCloudUrl}" `
            "-Dsonar.login=${sonarCloudToken}" `
            "-Dsonar.organization=sonarsource" `
            "-Dsonar.projectVersion=${currentVersion}" `
            -B -e -V `
        } -errorMessage "ERROR: Maven build deploy sonar FAILED."
    }
    else {
        Write-Header "Building SonarC# and SonarVB for branch" $branchName

        Set-MavenBuildVersion

        # No need for Maven phase "install" as the generated JAR files do not need to be installed
        # in Maven local repository. Phase "verify" is enough.
        Exec { & mvn verify "-Dmaven.test.redirectTestOutputToFile=false" `
            -B -e -V `
        } -errorMessage "ERROR: Maven verify FAILED."
    }
}

try {
    . (Join-Path $PSScriptRoot "build-utils.ps1")

    $buildConfiguration = "Release"
    $binPath = "bin\${buildConfiguration}"
    $solutionName = "SonarAnalyzer.sln"
    $branchName = Get-BranchName
    $isMaster = $branchName -eq "master"
    # See https://xtranet.sonarsource.com/display/DEV/Release+Procedures for info about maintenance branches
    $isMaintenanceBranch = $branchName -like 'branch-*'
    $isFeatureBranch = $branchName -like 'feature/*'
    $isPullRequest = $githubIsPullRequest -eq "true"

    Write-Debug "Solution to build: ${solutionName}"
    Write-Debug "Build configuration: ${buildConfiguration}"
    Write-Debug "Bin folder to use: ${binPath}"
    Write-Debug "Branch: ${branchName}"
    if ($isMaster) {
        Write-Debug "Build kind: master"
    }
    elseif ($isPullRequest) {
        Write-Debug "Build kind: PR"
        Write-Debug "PR: ${githubPullRequest}"
        Write-Debug "PR source: ${githubPRBaseBranch}"
        Write-Debug "PR target: ${githubPRTargetBranch}"
    }
    elseif ($isMaintenanceBranch) {
        Write-Debug "Build kind: maintenance branch"
    }
    elseif ($isFeatureBranch) {
        Write-Debug "Build kind: feature branch"
    }
    else {
        Write-Debug "Build kind: branch"
    }

    # Ensure the ImportBefore folder does not contain our targets
    Clear-MSBuildImportBefore

    Invoke-InLocation "${PSScriptRoot}\..\..\sonaranalyzer-dotnet" {
        Invoke-DotNetBuild
    }

    Invoke-InLocation "${PSScriptRoot}\..\.." {
        Invoke-JavaBuild
    }

    if ($isPullRequest -or $isMaster -or $isMaintenanceBranch) {
        Invoke-InLocation "${PSScriptRoot}\..\..\sonaranalyzer-dotnet" { Initialize-QaStep }
    }

    Write-Host -ForegroundColor Green "SUCCESS: BUILD job was successful!"
    exit 0
}
catch {
    Write-Host -ForegroundColor Red $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}