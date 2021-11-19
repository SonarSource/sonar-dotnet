function Get-Version {
    $versionPropsXml = [xml](Get-Content -Path scripts/version/Version.props)
    return $versionPropsXml.Project.PropertyGroup.MilestoneVersion
}

# Delete this step after the new image is finished and can be used
Write-host "Create tools directory"
$toolsPath = "C:\tools"
if (-Not (Test-Path $toolsPath)){
  New-Item -Path $toolsPath -ItemType "directory"
}

# Delete this step after the new image is finished and can be used
Write-host "Download WhiteSource tool"
$whiteSourceAgentPath = "$toolsPath\wss-unified-agent.jar"
Invoke-WebRequest -Uri https://unified-agent.s3.amazonaws.com/wss-unified-agent.jar -OutFile $whiteSourceAgentPath

Write-Host "Validating WhiteSource agent certificate signature..."
$cert = 'Signed by "CN=whitesource software inc, O=whitesource software inc, STREET=79 Madison Ave, L=New York, ST=New York, OID.2.5.4.17=10016, C=US"'
if (-Not (& "$env:JAVA\bin\jarsigner.exe" -verify -strict -verbose $whiteSourceAgentPath |  Select-String -Pattern $cert -CaseSensitive -Quiet)){
  Write-Host "wss-unified-agent.jar signature verification failed."
  exit 1
}

# WhiteSource agent needs the following environment variables:
# - WS_APIKEY
# - WS_PRODUCTNAME
# - WS_PROJECTNAME

$env:WS_PROJECTNAME = "$env:WS_PRODUCTNAME $(Get-Version)"

Write-Host "Running the WhiteSource unified agent for $env:WS_PROJECTNAME..."
& "$env:JAVA\bin\java.exe" -jar $env:WHITESOURCE_AGENT_PATH -c "$PSScriptRoot\wss-unified-agent.config"
