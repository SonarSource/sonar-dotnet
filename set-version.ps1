[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True, Position=1)]
    [ValidatePattern("\d{1,3}\.\d{1,3}(\.\d{1,3})?")]
    [string]$version
)

function UpdateJavaFiles {
    Write-Output "Update version in Java files"

    $fixedVersion = $version
    If ($fixedVersion.Split('.').Count -eq 3 -and $fixedVersion.EndsWith(".0")) {
        $fixedVersion = $version.Substring(0, $version.Length - 2)
    }
    mvn org.codehaus.mojo:versions-maven-plugin:2.2:set "-DnewVersion=${fixedVersion}-SNAPSHOT" -DgenerateBackupPoms=false -B -e
}

function UpdateDotNetFiles {
    Write-Output "Update version in .Net files"

    Push-Location ".\sonaranalyzer-dotnet\build"
    $versionPropsFile = "Version.props"
    $xml = [xml](Get-Content $versionPropsFile)
    $xml.Project.PropertyGroup.MainVersion = ${version}
    $xml.Save($versionPropsFile)

    msbuild "ChangeVersion.proj"
    Pop-Location
}

UpdateJavaFiles
UpdateDotNetFiles