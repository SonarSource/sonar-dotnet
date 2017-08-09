Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\..\scripts\build\build-utils.ps1")

try {
    Push-Location $PSScriptRoot

    Remove-Item .\expected -Recurse -Force
    Rename-Item .\actual .\expected
    Remove-Item .\expected\.git -Recurse -Force
}
finally {
    Pop-Location
}