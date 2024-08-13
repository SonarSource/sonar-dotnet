function Get-Version {
    $versionPropsXml = [xml](Get-Content -Path scripts/version/Version.props)
    return $versionPropsXml.Project.PropertyGroup.MilestoneVersion
}

# Mend agent needs the following environment variables:
# - WS_APIKEY
# - WS_PRODUCTNAME
# - WS_PROJECTNAME

$env:WS_PROJECTNAME = "$env:WS_PRODUCTNAME $(Get-Version)"

Write-Host "Running the Mend unified agent for $env:WS_PROJECTNAME..."
& "$env:JAVA_HOME\bin\java.exe" -jar $env:MEND_AGENT_PATH -c "$PSScriptRoot\wss-unified-agent.config" -scanComment "buildNumber:97113;gitSha:cc0ba76c09677fe094621d35ab0c408bd1215aaa"
