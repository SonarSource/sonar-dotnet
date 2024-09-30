[CmdletBinding()]
Param(
    [Parameter(Mandatory = $True, Position = 1)]
    [ValidatePattern("^\d{1,3}\.\d{1,3}(\.\d{1,3})?$")]
    [String]$Version,
    [Int]$BuildNumber=0,
    [String]$Branch,
    [string]$Sha1
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

function UpdatePomVersion($Path, $Version) {
    Write-Host "Updating version to ${Version} in ${Path}"
    # This is done manually, because mvn tries to resolve parent poms from jfrog and that requires heavy orchestration
    $Content = Get-Content $Path
    $Content = $Content -replace "<version>.*?-SNAPSHOT</version>", "<version>${Version}</version>"
    Set-Content $Path $Content
}

function Set-VersionForJava() {
    Write-Header "Updating version in Java files"
    $PatchVersion = If ($ShortVersion.IndexOf(".") -eq $ShortVersion.LastIndexOf(".")) { "${ShortVersion}.0" } else { $ShortVersion }
    $MavenVersion = If ($BuildNumber -eq 0) { "${ShortVersion}-SNAPSHOT" } else { "${PatchVersion}.${BuildNumber}" }
    $Files = Get-ChildItem -Path . -Filter "pom.xml" -Recurse
    foreach ($File in $Files) {
        UpdatePomVersion $File.FullName $MavenVersion
    }
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

    Write-Host "Updating Version.targets to ShortVersion=${ShortVersion}, BuildNumber=${BuildNumber}, Branch=${Branch}, Sha1=${Sha1}"
    $Path = Resolve-Path ".\analyzers\Version.targets"
    $Xml = [xml](Get-Content $Path)
    $Xml.Project.PropertyGroup.ShortVersion = $ShortVersion
    $Xml.Project.PropertyGroup.BuildNumber = $BuildNumber.ToString()
    $Xml.Project.PropertyGroup.Branch = $Branch
    $Xml.Project.PropertyGroup.Sha1 = $Sha1
    $Xml.Save($Path)
}

try {
    . (Join-Path $PSScriptRoot ".\utils.ps1")

    Push-Location "${PSScriptRoot}\.."

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