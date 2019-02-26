<#

.SYNOPSIS
This script allows to automatically create a GitHub issue from RSPEC.

#>

[CmdletBinding(PositionalBinding = $false)]
param (
    [Parameter(Mandatory = $true, HelpMessage = "action: new, fix or update", Position = 0)]
    [ValidateSet("new", "fix", "update")]
    [string]$action,
    [Parameter(Mandatory = $true, HelpMessage = "language: cs or vbnet", Position = 1)]
    [ValidateSet("cs", "vbnet")]
    [string]$language,
    [Parameter(Mandatory = $true, HelpMessage = "numeric rule key, e.g. without S in the beginning", Position = 2)]
    [string]$rspecKey,
    [Parameter(Mandatory = $true, HelpMessage = "your github auth token", Position = 3)]
    [string]$githubToken,
    [int]$milestoneKey
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

if ($language -eq "vbnet") {
    throw "At the moment, script only supports C#. Create a ticket manually in JIRA: https://jira.sonarsource.com/browse/SONARVB"
}

$languageMap = @{
    "cs" = "c#";
    "vbnet" = "vb.net";
}

# Update the following variable when a new version of rule-api has to be used.
$rule_api_version = "1.22.2.1215"
$rule_api_error = "Download Rule-api from " + `
    "'https://repox.jfrog.io/repox/sonarsource-private-releases/com/sonarsource/rule-api/rule-api/${rule_api_version}' " +`
    "to a folder and set the %rule_api_path% environment variable with the full path of that folder. For example 'c:\\work\\tools'."
if (-Not $Env:rule_api_path) {
    throw $rule_api_error
}
$rule_api_jar = "${Env:rule_api_path}\\rule-api-${rule_api_version}.jar"
if (-Not (Test-Path $rule_api_jar)) {
    throw $rule_api_error
}

function Get-Description() {
    $convertedLanguage = $languageMap.Get_Item($language)

    $htmlPath = "${rspecFolder}\\S${rspecKey}_${convertedLanguage}.html"
    if (-Not (Test-Path $htmlPath)) {
        Get-RspecMetadata
    }

    $html = Get-Content -Raw $htmlPath

    # take the first paragraph of the HTML file
    if ($html -Match "<p>((.|\n)*?)</p>") {
        # strip HTML tags and new lines
        $description = $Matches[1] -replace '<[^>]*>', ''
        $description = $description -replace '\n|( +)', ' '
    }

    return "[RSPEC-${rspecKey}](https://jira.sonarsource.com/browse/RSPEC-${rspecKey})`n`n${description}"
}

function Get-Title() {
    $convertedLanguage = $languageMap.Get_Item($language)

    $jsonPath = "${rspecFolder}\\S${rspecKey}_${convertedLanguage}.json"
    if (-Not (Test-Path $jsonPath)) {
        Get-RspecMetadata
    }

    $json = Get-Content -Raw $jsonPath | ConvertFrom-Json

    switch ($action) {
        "new" { $prefix = "Rule" }
        "fix" { $prefix = "Fix" }
        "update" { $prefix = "Update" }
        Default {
            throw "Not supported action: ${action}"
        }
    }

    return "${prefix} S${rspecKey}: $(${json}.title)"
}

function Get-RspecMetadata() {
    Write-Host "Downloading RSPEC metadata to ${rspecFolder}"

    $convertedLanguage = $languageMap.Get_Item($language)
    java -jar $rule_api_jar generate -directory $rspecFolder -language $convertedLanguage -rule "S${rspecKey}" | Out-Host
}

function Get-Labels() {
    switch ($action) {
        "new" { return @("Area: Rules", "Type: New Feature") }
        "fix" { return @("Area: Rules", "Type: Bug") }
        "update" { return @("Area: Rules", "Type: Improvement") }
        Default {
            throw "Not supported action: ${action}"
        }
    }
}

function New-Issue() {
    $title = Get-Title
    Write-Host "Creating issue ${title}"

    $headers = @{"Authorization" = "token ${githubToken}"}

    $payload = @{
        title = $title
        body = Get-Description
        labels = Get-Labels
    }

    if ($milestoneKey) {
        $payload.milestone = $milestoneKey
    }

    # NB: the WebClient class defaults to TLS v1, which is no longer supported by GitHub/Artifactory online
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest `
        -Uri "https://api.github.com/repos/SonarSource/sonar-dotnet/issues" `
        -Method "POST" `
        -Headers $headers `
        -Body ($payload | ConvertTo-Json) `
        -UseBasicParsing

    # we don't add issues to the project because the api is still experimental
}


try {
    $rspecFolder = (Join-Path $PSScriptRoot "temp-rspec")
    if (-Not (Test-Path $rspecFolder)) {
        New-Item $rspecFolder -ItemType Directory | Out-Null
    }

    New-Issue

    exit 0
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}