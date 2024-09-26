# Save variables to files so they can be used by other tasks and stages
# -Force to suppress the error message when it exists on reused agent
# | Out-Null to suppress noisy output report
$Dir = "$env:AGENT_BUILDDIRECTORY\Azp-Variables"
New-Item -ItemType Directory -Path $Dir -Force | Out-Null

function WriteVariable($Name, $Value) {
    $Path = "${Dir}\${Name}"
    Write-Host "Writing '${Value}' to ${Path}"
    Set-Content -Path $Path -Value $Value
}

$VersionFilePath = "$env:BUILD_SOURCESDIRECTORY\analyzers\Version.targets"
[xml]$Xml = Get-Content "$VersionFilePath"
$ShortVersion = $Xml.Project.PropertyGroup.ShortVersion
$PatchVersion = if ($ShortVersion.IndexOf(".") -eq $ShortVersion.LastIndexOf(".")) { "${ShortVersion}.0" } else { $ShortVersion }

WriteVariable "SHORT_VERSION" $ShortVersion
WriteVariable "FULL_VERSION" "${PatchVersion}.${env:BUILD_BUILDID}"
