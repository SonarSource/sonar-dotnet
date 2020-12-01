[CmdletBinding(PositionalBinding = $false)]
param
(
    [Parameter(HelpMessage = "The name of single project to update issues for. If ommited, issues for all projects will be updated.")]
    [ValidateSet("AnalyzeGenerated", "AnalyzeGeneratedVb", "akka.net", "Automapper", "Ember-MM", "Nancy", "NetCore31", "Net5", "ManuallyAddedNoncompliantIssues", "ManuallyAddedNoncompliantIssuesVB", "SkipGenerated", "SkipGeneratedVb")]
    [string]
    $project
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\..\scripts\build\build-utils.ps1")

function Update-Single-Project([string]$project) {
    Write-Host "Will update issues only for: $project."

    $projectPath = ".\expected\${project}"
    if (Test-Path $projectPath) {
        Remove-Item $projectPath  -Recurse -Force
    }
    Copy-Item -Path ".\actual\${project}" -Destination  ".\expected" -Force -Recurse
    if (Test-Path .\diff) {
        Remove-Item .\diff -Recurse -Force
    }
}

function Update-All-Projects() {
    Write-Host "Will update issues for all projects."

    Remove-Item .\expected -Recurse -Force
    Rename-Item .\actual .\expected

    if (Test-Path .\diff) {
        Remove-Item .\diff -Recurse -Force
    }
}

try {
    Push-Location $PSScriptRoot

    if (-Not (Test-Path .\actual)) {
        Write-Error "no 'actual' folder. Have you run this script twice after tests? Have you run the test?"
    }

    if ($project) {
        Update-Single-Project $project
    } else {
        Update-All-Projects
    }
}
finally {
    Pop-Location
}