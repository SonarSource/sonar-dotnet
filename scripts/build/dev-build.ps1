<#

.SYNOPSIS
This script controls the analyzer build process (from buildling to creating the NuGet packages).

.DESCRIPTION
Usage: build.ps1
    Steps
        -build              Build the analyzer solution
        -buildJava          Build the plugin (java)
        -coverage           Compute the code coverage of the analyzer
        -pack               Create the NuGet packages and extra metadata

    Test options
        -test               Run analyzer unit tests
        -its                Run analyzer ITs
        -itsJava            Run plugin ITs

    Special options
        -release            Perform release build (default is debug)

#>

[CmdletBinding(PositionalBinding = $false)]
param (
    # Steps to execute
    [switch]$build = $false,
    [switch]$buildJava = $false,
    [switch]$coverage = $false,
    [switch]$pack = $false,

    # Test options
    [switch]$test = $false,
    [switch]$its = $false,
    [switch]$itsJava = $false,

    # Build output options
    [switch]$release = $false
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

if ($PSBoundParameters['Verbose'] -Or $PSBoundParameters['Debug']) {
    $DebugPreference = "Continue"
}

try {
    . (Join-Path $PSScriptRoot "build-utils.ps1")
    Push-Location "${PSScriptRoot}\..\..\analyzers"

    Write-Header "Script configuration"
    $buildConfiguration = if ($release) { "Release" } else { "Debug" }
    $binPath = "bin\${buildConfiguration}"
    $solutionName = "SonarAnalyzer.sln"
    $msbuildVersion = "15.0"

    Write-Host "Solution to build: $solutionName"
    Write-Host "Build configuration: $buildConfiguration"
    Write-Host "Bin folder to use: $binPath"
    Write-Host "MSBuild: ${msbuildVersion}"

    $StartDate=(Get-Date)

    if ($build) {
        # Restore VSIX (special project)
        Push-Location "src\SonarAnalyzer.Vsix"
        nuget restore .\packages.SonarAnalyzer.Vsix.config -PackagesDirectory "../../packages"
        Pop-Location

        Invoke-MSBuild $msbuildVersion $solutionName /t:"Restore,Rebuild" `
            /consoleloggerparameters:Summary `
            /m `
            /p:configuration=$buildConfiguration `
            /p:DeployExtension=false `
    }

    if ($test) {
        # in ParseOptionsHelper.FilterByEnvironment, it does not run tests for all C# versions unless it detects it's on the CI/CD
        # It considers to run under CI/CD only if the following environment variable is set.
        # When running this script, we want to run all unit tests.
        # see analyzers\tests\SonarAnalyzer.UnitTest\TestFramework\ParseOptionsHelper.cs
        $env:SYSTEM_DEFINITIONID="set"
        Invoke-UnitTests $binPath $buildConfiguration
    }

    if ($its) {
        Invoke-IntegrationTests $msbuildVersion
    }

    if ($coverage) {
        Invoke-CodeCoverage
    }

    if ($pack) {
        New-NuGetPackages $binPath
    }

    if ($buildJava) {
        $skipTests = "-DskipTests"
        if ($test) {
            $skipTests = ""
        }
        Invoke-InLocation ".." {
            Exec { & mvn clean install -P local-analyzer -D analyzer.configuration=$buildConfiguration $skipTests }
        }
    }

    if ($itsJava) {
        Invoke-InLocation "..\its" {
            Exec { & mvn clean install }
        }
    }

    $EndDate=(Get-Date)
    $Time=(New-TimeSpan -Start $StartDate -End $EndDate)
    Write-Host "Time spent: $Time"

    Write-Host -ForegroundColor Green "SUCCESS: script was successful!"
    exit 0
}
catch {
    Write-Host -ForegroundColor Red $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
finally {
    Pop-Location
}
