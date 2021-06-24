# Read variables shared between stages and tasks
$Dir = "$env:AGENT_BUILDDIRECTORY\azp-variables"

$Version = Get-Conent "${Dir}\SONAR_PROJECT_VERSION"
Write-Host "Sonar project version is '${Version}'"

# Set the azure build variable (SONAR_PROJECT_VERSION) so it can be used by other tasks
Write-Host "##vso[task.setvariable variable=SONAR_PROJECT_VERSION;]$Version"
