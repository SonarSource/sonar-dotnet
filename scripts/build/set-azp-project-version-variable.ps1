# Read the project version from the Version.props file
$version = "$env:BUILD_SOURCESDIRECTORY\scripts\version\Version.props"
Write-Host "Reading the Sonar project version from '${version}' ..."

[xml]$versionProps = Get-Content "$version"
$sonarProjectVersion = $versionProps.Project.PropertyGroup.MainVersion
Write-Host "Sonar project version is '${sonarProjectVersion}'"

# Set the azure build variable (SONAR_PROJECT_VERSION) so it can be used by other tasks
Write-Host "##vso[task.setvariable variable=SONAR_PROJECT_VERSION;]$sonarProjectVersion"