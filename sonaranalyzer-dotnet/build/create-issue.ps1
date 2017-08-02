param
(
    [Parameter(Mandatory = $true, HelpMessage = "action: new, fix or update", Position = 0)]
    [ValidateSet("new", "fix", "update")]
    [string]$action,
    [Parameter(Mandatory = $true, HelpMessage = "language: cs or vbnet", Position = 1)]
    [ValidateSet("cs", "vbnet")]
    [string]$lang,
    [Parameter(Mandatory = $true, HelpMessage = "numeric rule key, e.g. without S in the beginning", Position = 2)]
    [string]$ruleKey,
    [Parameter(Mandatory = $true, HelpMessage = "your github auth token", Position = 3)]
    [string]$token,
    [int]$milestoneKey
)

$projectUri = "https://api.github.com/repos/SonarSource/sonar-csharp/issues"

$ruleapiLanguageMap =
@{
    "cs" = "c#";
    "vbnet" = "vb.net";
}

function Get-Description() {
    $language = $ruleapiLanguageMap.Get_Item($lang)

    $htmlPath = "${rspecFolder}\\S${ruleKey}_${language}.html"
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

    return "[RSPEC-${ruleKey}](https://jira.sonarsource.com/browse/RSPEC-${ruleKey})`n`n${description}"
}

function Get-Title() {
    $language = $ruleapiLanguageMap.Get_Item($lang)

    $jsonPath = "${rspecFolder}\\S${ruleKey}_${language}.json"
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

    return "${prefix} S${ruleKey}: $(${json}.title)"
}

function Get-RspecMetadata() {
    Write-Host "downloading RSPEC metadata to ${rspecFolder}"

    $language = $ruleapiLanguageMap.Get_Item($lang)
    java -jar $env:rule_api_path generate -directory $rspecFolder -language $language -rule "S${ruleKey}" | Out-Host
}

function Get-Labels() {
    switch ($action) {
        "new" { return @("rules", "new feature") }
        "fix" { return @("rules", "bug", "false-positive") }
        "update" { return @("rules", "improvement") }
        Default {
            throw "Not supported action: ${action}"
        }
    }
}

function New-Issue() {
    Write-Host "creating issue ${title}"

    $headers = @{"Authorization" = "token ${token}"}

    $payload = @{
        title = (Get-Title)
        body = (Get-Description)
        labels = (Get-Labels)
    }

    if ($milestoneKey) {
        $payload.milestone = $milestoneKey
    }

    Invoke-WebRequest `
        -Uri $projectUri `
        -Method "POST" `
        -Headers $headers `
        -Body ($payload | ConvertTo-Json) `
        -UseBasicParsing

    # we don't add issues to the project because the api is still experimental
}

Write-Host "starting..."

$rspecFolder = (Join-Path $PSScriptRoot "temp-rspec")
if (-Not (Test-Path $rspecFolder)) {
    New-Item $rspecFolder -ItemType Directory | Out-Null
}

New-Issue