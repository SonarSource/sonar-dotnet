# Mend agent needs the following environment variables:
# - WS_APIKEY
# - WS_PRODUCTNAME
# - WS_PROJECTNAME

[xml]$Xml = [xml](Get-Content -Path analyzers/Version.targets)
$Version = $Xml.Project.PropertyGroup.ShortVersion
$env:WS_PROJECTNAME = "$env:WS_PRODUCTNAME $Version"

Write-Host "Running the Mend unified agent for $env:WS_PROJECTNAME..."
& "$env:JAVA_HOME\bin\java.exe" -jar $env:MEND_AGENT_PATH -c "$PSScriptRoot\wss-unified-agent.config" -scanComment "buildNumber:$env:BUILD_NUMBER;gitSha:$env:GIT_SHA"
exit $LASTEXITCODE
