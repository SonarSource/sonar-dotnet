[CmdletBinding()]
Param(
    [Parameter(Mandatory = $True, Position = 1)]
    [ValidatePattern("^\d{1,3}\.\d{1,3}\.\d{1,3}$")]
    [string]$version
)

function Write-Header([string]$message) {
    Write-Host "================================================"
    Write-Host $message
    Write-Host "================================================"
}

function Set-VersionForJava {
    Write-Header "Updating version in Java files"

    $fixedVersion = $version
    If ($fixedVersion.EndsWith(".0")) {
        $fixedVersion = $version.Substring(0, $version.Length - 2)
    }
    mvn org.codehaus.mojo:versions-maven-plugin:2.2:set "-DnewVersion=${fixedVersion}-SNAPSHOT" -DgenerateBackupPoms=false -B -e
}

function Set-VersionForDotNet {
    Write-Header "Updating version in .Net files"

    try {
        Push-Location ".\sonaranalyzer-dotnet\build"
        $versionPropsFile = Resolve-Path "Version.props"
        $xml = [xml](Get-Content $versionPropsFile)
        $xml.Project.PropertyGroup.MainVersion = ${version}
        $xml.Save($versionPropsFile)
        msbuild "ChangeVersion.proj"
    }
    finally {
        Pop-Location
    }
}

Set-VersionForJava
Write-Output ""
Set-VersionForDotNet