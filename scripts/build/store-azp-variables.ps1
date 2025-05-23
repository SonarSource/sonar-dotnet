# Read the project version from the Version.props file
$VersionFilePath = "$env:BUILD_SOURCESDIRECTORY\scripts\version\Version.props"
Write-Host "Reading the Sonar project version from '${VersionFilePath}' ..."

[xml]$VersionProps = Get-Content "$VersionFilePath"
$Version = $VersionProps.Project.PropertyGroup.MainVersion
Write-Host "Sonar project version is '${Version}'"

# Save variables to files so they can be used by other tasks and stages
# -Force to suppress the error message when it exists on reused agent
# | Out-Null to suppress noisy output report
$Dir = "$env:AGENT_BUILDDIRECTORY\Azp-Variables"
New-Item -ItemType Directory -Path $Dir -Force | Out-Null

$File = "${Dir}\SONAR_PROJECT_VERSION"
Write-Host "Writing '${Version}' to ${File}"
Set-Content -Path $File -Value $Version
