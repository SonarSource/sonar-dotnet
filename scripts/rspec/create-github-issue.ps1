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

$languageMap = @{
    "cs" = "c#";
    "vbnet" = "vb.net";
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
    java -jar $env:rule_api_path generate -directory $rspecFolder -language $convertedLanguage -rule "S${rspecKey}" | Out-Host
}

function Get-Labels() {
    switch ($action) {
        "new" { return @("rules", "Type: New Feature") }
        "fix" { return @("rules", "Type: Bug", "Type: False Positive") }
        "update" { return @("rules", "Type: Improvement") }
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

    Invoke-WebRequest `
        -Uri "https://api.github.com/repos/SonarSource/sonar-csharp/issues" `
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