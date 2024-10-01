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

function UpdatePom($Path, $Version) {
    Write-Host "Updating version to $Version in $Path"
    # This is done manually, because mvn tries to resolve parent poms from jfrog and that requires heavy orchestration
    $Pattern = "<version>.*?-SNAPSHOT</version>"
    $Content = Get-Content $Path
    If ($Content -match $Pattern) {
        $Content = $Content -replace $Pattern, "<version>$Version</version>"
        Set-Content $Path $Content
    }
    Else {
        throw "Could not find $Pattern in $Path. This script is not supposed to be run multiple times. Revert your git changes first."
    }
}

function UpdateJava() {
    Write-Host "Updating version in Java files"
    $MavenVersion = If ($BuildNumber -eq 0) { "$ShortVersion-SNAPSHOT" } else { "${PatchVersion}.${BuildNumber}" }
    $Files = Get-ChildItem -Path . -Filter "pom.xml" -Recurse
    foreach ($File in $Files) {
        UpdatePom $File.FullName $MavenVersion
    }
}

function UpdateDotNet() {
    $Path = Resolve-Path ".\analyzers\Version.targets"
    Write-Host "Updating $Path to ShortVersion=$ShortVersion, BuildNumber=$BuildNumber, Branch=$Branch, Sha1=$Sha1"
    $Xml = [xml](Get-Content $Path)
    $Xml.Project.PropertyGroup.ShortVersion = $ShortVersion
    $Xml.Project.PropertyGroup.BuildNumber = $BuildNumber.ToString()
    $Xml.Project.PropertyGroup.Branch = $Branch
    $Xml.Project.PropertyGroup.Sha1 = $Sha1
    $Xml.Save($Path)
}

Push-Location "${PSScriptRoot}\.."  # Run everything from the root of the repository
try {
    $ShortVersion = If ($Version.EndsWith(".0") -and $Version.IndexOf(".") -ne $Version.LastIndexOf(".")) { $Version.Substring(0, $Version.Length - 2) } else { $Version } # x.y   or x.y.z
    $PatchVersion = If ($ShortVersion.IndexOf(".") -eq $ShortVersion.LastIndexOf(".")) { "$ShortVersion.0" } else { $ShortVersion }                                       # x.y.0 or x.y.z

    UpdateDotNet
    UpdateJava

    If ($BuildNumber -eq 0) {
        Write-Host "Creating Git commit"
        git commit -a -m "Bump version to $ShortVersion"
    }

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