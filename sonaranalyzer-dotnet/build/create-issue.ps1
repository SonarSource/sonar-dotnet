param
(
    [Parameter(Mandatory = $true, HelpMessage = "action: new, fix or update", Position = 0)]
    [ValidateSet("new", "fix", "update")]
    [string]$action,
    [Parameter(Mandatory = $true, HelpMessage = "numeric rule key, e.g. without S in the beginning", Position = 1)]
    [string]$ruleKey,
    [Parameter(Mandatory = $true, HelpMessage = "your github auth token", Position = 2)]
    [string]$token,
    [int]$milestoneKey
)

function Get-Description() {
    $htmlPath = "${rspecFolder}\\S${ruleKey}_c#.html"
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
    $jsonPath = "${rspecFolder}\\S${ruleKey}_c#.json"
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
    java -jar $env:rule_api_path generate -directory $rspecFolder -language "c#" -rule "S${ruleKey}" | Out-Host
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
        -Uri "https://api.github.com/repos/SonarSource/sonar-csharp/issues" `
        -Method "POST" `
        -Headers $headers `
        -Body ($payload | ConvertTo-Json)

    # we don't add issues to the project because the api is still experimental
}

Write-Host "starting..."

$rspecFolder = (Join-Path $PSScriptRoot "temp-rspec")
if (-Not (Test-Path $rspecFolder)) {
    New-Item $rspecFolder -ItemType Directory | Out-Null
}

New-Issue
