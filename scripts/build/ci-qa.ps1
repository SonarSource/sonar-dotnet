<#

.SYNOPSIS
This script controls the analyzer QA process.

#>

[CmdletBinding(PositionalBinding = $false)]
param (
    [string]$version = $env:VERSION,

    # GitHub related parameters
    [string]$githubBranch = $env:GITHUB_BRANCH,
    [string]$githubSha1 = $env:GIT_SHA1,

    # repox related parameters
    [string]$repoxUserName = $env:REPOX_QAPUBLICADMIN_USERNAME,
    [string]$repoxPassword = $env:REPOX_QAPUBLICADMIN_PASSWORD
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

function Get-PackageUrl([string]$packageName) {
    return "$env:ARTIFACTORY_URL/api/nuget/sonarsource-nuget-qa/${packageName}"
}

function Get-AuthHeaders() {
    if ((-Not $repoxUserName) -Or (-Not $repoxPassword)) {
        Write-Host "Username and password not provided, the NuGet download request will be not authenticated."
        return @{ }
    }

    $pair = "${repoxUserName}:${repoxPassword}"
    $encodedCredentials = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
    $basicAuthValue = "Basic $encodedCredentials"
    return @{Authorization = $basicAuthValue}
}

function Get-BuildArtifacts() {
    Write-Header "Downloading build artifacts from repox..."

    $authHeaders = (Get-AuthHeaders)
    $packagesToDownload = "SonarAnalyzer.CSharp", "SonarAnalyzer.VisualBasic"

    $currentDir = (Resolve-Path .\).Path
    $tempFolder = Join-Path $currentDir "temp" #TODO: make path random
    try {

        Remove-Item $tempFolder -Recurse -Force -ErrorAction Ignore
        New-Item $tempFolder -ItemType Directory

        # NB: the WebClient class defaults to TLS v1, which is no longer supported by GitHub/Artifactory online
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

        foreach ($packageName in $packagesToDownload) {
            $downloadUrl = Get-PackageUrl "${packageName}.${version}.nupkg"
            $zipPath = Join-Path $tempFolder "${packageName}.${version}.zip"

            Write-Host "Downloading '${downloadUrl}' at '${zipPath}'"
            Invoke-WebRequest -UseBasicParsing -Uri $downloadUrl -Headers $authHeaders -OutFile $zipPath

            if (-Not (Test-Path $zipPath)) {
                throw "Zip file was not downloaded correctly"
            }

            Expand-ZIPFile $zipPath $tempFolder
        }

        $tempAnalyzers = Join-Path $tempFolder "analyzers"
        $binaryFolder = ".\its\binaries"
        Write-Debug "Copying unzipped items to '${tempAnalyzers}'"
        Copy-Item $tempAnalyzers $binaryFolder -Recurse -Force
    }
    finally {
        Write-Debug "Removing temp folder '${tempFolder}'"
        Remove-Item $tempFolder -Recurse -Force -ErrorAction Ignore
    }
}

function Initialize-ArtifactsStep() {
    Write-Header "Queueing Artifacts job..."

    $sha1propertiesPath = "sha1.properties"

    $branchName = Get-BranchName

    "SHA1=${githubSha1}", `
    "GITHUB_BRANCH=${branchName}" | Out-File -Encoding utf8 $sha1propertiesPath

    ConvertTo-UnixLineEndings $sha1propertiesPath

    $content = Get-Content $sha1propertiesPath
    Write-Debug "Successfully created sha1.properties with '${content}'"
    Write-Host "Triggering Artifacts job"
}

try {
    . (Join-Path $PSScriptRoot "build-utils.ps1")

    Invoke-InLocation "${PSScriptRoot}\..\..\sonaranalyzer-dotnet" {
        Get-BuildArtifacts
        Invoke-IntegrationTests "15.0"
        Initialize-ArtifactsStep
    }

    Write-Host -ForegroundColor Green "SUCCESS: QA job was successful!"
    exit 0
}
catch {
    Write-Host -ForegroundColor Red $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
