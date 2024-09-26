<#

.SYNOPSIS
This script allows to set the specified version in all required files.

See in .\scripts\version\README.md how version is set during the pipeline run

#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory = $True, Position = 1)]
    [ValidatePattern("^\d{1,3}\.\d{1,3}\.\d{1,3}$")]
    [String]$Version,
    [Int]$BuildNumber=0,
    [String]$Branch,
    [string]$Sha1
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

function Set-VersionForJava() {
    Write-Header "Updating version in Java files"

    mvn org.codehaus.mojo:versions-maven-plugin:2.2:set "-DnewVersion=${ShortVersion}-SNAPSHOT" -DgenerateBackupPoms=false -B -e -P its
    Test-ExitCode "ERROR: Maven set version FAILED."
}

function Set-VersionForDotNet() {
    Write-Header "Updating version in .Net files"

    Invoke-InLocation ".\scripts\version" {
        $Path = Resolve-Path "Version.props"
        $Xml = [xml](Get-Content $Path)
        $Xml.Project.PropertyGroup.MainVersion = $Version
        $Xml.Project.PropertyGroup.MilestoneVersion = $ShortVersion
        $Xml.Save($Path)
    }

    $Path = Resolve-Path ".\analyzers\Version.targets"
    $Xml = [xml](Get-Content $Path)
    $Xml.Project.PropertyGroup.ShortVersion = $ShortVersion
    $Xml.Project.PropertyGroup.BuildNumber = $BuildNumber.ToString()
    $Xml.Project.PropertyGroup.Branch = $Branch
    $Xml.Project.PropertyGroup.Sha1 = $Sha1
    $Xml.Save($Path)
}

try {
    . (Join-Path $PSScriptRoot "..\utils.ps1")

    Push-Location "${PSScriptRoot}\..\.."

    $ShortVersion = $Version
    if ($ShortVersion.EndsWith(".0")) {
        $ShortVersion = $ShortVersion.Substring(0, $Version.Length - 2)
    }

    Set-VersionForJava
    Set-VersionForDotNet

    exit 0
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
finally {
    Pop-Location
}