# Read the project version from the Version.props file
$versionFilePath = "$env:BUILD_SOURCESDIRECTORY\scripts\version\Version.props"
Write-Host "Reading the Sonar project version from '${versionFilePath}' ..."

[xml]$versionProps = Get-Content "$versionFilePath"
$version = $versionProps.Project.PropertyGroup.MainVersion
Write-Host "Sonar project version is '${version}'"

# Set the azure build variable (SONAR_PROJECT_VERSION) so it can be used by other tasks
Write-Host "##vso[task.setvariable variable=SONAR_PROJECT_VERSION;]$version"
