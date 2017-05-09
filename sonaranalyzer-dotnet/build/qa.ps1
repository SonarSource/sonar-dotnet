[CmdletBinding(PositionalBinding = $false)]
param (
    [string][Parameter(Mandatory = $true, Position = 0)]$version,
    [string]$artifactoryUser = $null,
    [string]$artifactoryPass = $null,
    [string]$artifactoryUrl = "https://repox.sonarsource.com/api/nuget",
    [parameter(ValueFromRemainingArguments = $true)] $badArgs)

$ErrorActionPreference = "Stop"

if ($badArgs -ne $null) {
    throw "Bad arguments: $badArgs"
}

. (Join-Path $PSScriptRoot "build-utils.ps1")

function Get-PackageUrl([string]$packageName) {
    return "${artifactoryUrl}/sonarsource-nuget-qa/${packageName}"
}

function Get-AuthHeaders() {
    if ((-Not $artifactoryUser) -Or (-Not $artifactoryPass)) {
        Write-Warning "Username and password not provided, the nuget download request will be not authenticated."
        return @{ }
    }

    $pair = "${artifactoryUser}:${artifactoryPass}"
    $encodedCredentials = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
    $basicAuthValue = "Basic $encodedCredentials"
    return @{Authorization = $basicAuthValue}
}

function Get-BuildArtifacts {
    Write-Header "Downloading build artifacts..."

    $authHeaders = (Get-AuthHeaders)

    $packageNames = "SonarAnalyzer.CSharp", "SonarAnalyzer.VisualBasic", "SonarAnalyzer.Scanner"

    try {
        $tempFolder = (Resolve-RepoPath "temp") #TODO: make random

        Remove-Item $tempFolder -Recurse -Force -ErrorAction Ignore
        New-Item $tempFolder -ItemType Directory

        foreach ($packageName in $packageNames) {
            $downloadUrl = (Get-PackageUrl "${packageName}.${version}.nupkg")
            $zipPath = (Join-Path $tempFolder "${packageName}.${version}.zip")

            Invoke-WebRequest -UseBasicParsing -Uri $downloadUrl -Headers $authHeaders -OutFile $zipPath

            if (-Not (Test-Path $zipPath)) {
                throw "Zip file was not downloaded correctly"
            }

            Expand-ZIPFile $zipPath $tempFolder
        }

        $tempAnalyzers = Join-Path $tempFolder "analyzers"
        $binaryFolder = Resolve-RepoPath ".\its\binaries"
        Copy-Item $tempAnalyzers $binaryFolder -Recurse -Force
    }
    finally {
        Remove-Item $tempFolder -Recurse -Force -ErrorAction Ignore
    }
}

function Invoke-IntegrationTests {
    Write-Header "Running integration tests..."

    Exec-InLocation (Resolve-RepoPath "") {
        Exec { git submodule update --init --recursive --depth 1 }

        & .\its\regression-test.ps1
        Test-ExitCode "ERROR: Integration tests FAILED."
    }
}

function Queue-ArtifactsBuild {
    Write-Header "Queueing Artifacts job..."

    $sha1propertiesPath = (Resolve-RepoPath "sha1.properties")

    $sha1 = Get-Sha1
    $branchName = Get-BranchName

    "SHA1=${sha1}", `
    "GITHUB_BRANCH=${branchName}" `
        | Out-File -Encoding utf8 $sha1propertiesPath

    To-UnixLineEndings $sha1propertiesPath

    $content = Get-Content $sha1propertiesPath
    Write-Host "sha1.properties content is '{$content}'"
}

Get-BuildArtifacts
Invoke-IntegrationTests
Queue-ArtifactsBuild