Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\..\scripts\build\build-utils.ps1")

try {
    Push-Location $PSScriptRoot

    if (-Not (Test-Path .\actual)) {
        Write-Error "no 'actual' folder. Have you run this script twice after tests? Have you run the test?"
    }

    Remove-Item .\expected -Recurse -Force
    Rename-Item .\actual .\expected

    if (Test-Path .\diff) {
        Remove-Item .\diff -Recurse -Force
    }
}
finally {
    Pop-Location
}