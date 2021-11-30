function Get-Version {
    $versionPropsXml = [xml](Get-Content -Path scripts/version/Version.props)
    return $versionPropsXml.Project.PropertyGroup.MilestoneVersion
}

# WhiteSource agent needs the following environment variables:
# - WS_APIKEY
# - WS_PRODUCTNAME
# - WS_PROJECTNAME

$env:WS_PROJECTNAME = "$env:WS_PRODUCTNAME $(Get-Version)"

Write-Host "Running the WhiteSource unified agent for $env:WS_PROJECTNAME..."
& "$env:JAVA\bin\java.exe" -jar $env:WHITESOURCE_AGENT_PATH -c "$PSScriptRoot\wss-unified-agent.config"
