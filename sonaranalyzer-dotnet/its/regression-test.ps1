[CmdletBinding(PositionalBinding = $false)]
param ([ValidateSet("14.0", "15.0")][string]$msbuildVersion = "14.0")

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

function Test-SonarAnalyzerDll {
    if (-Not (Test-Path ".\binaries\SonarAnalyzer.dll")) {
        throw "Could not find '.\binaries\SonarAnalyzer.dll'."
    }
}

function Build-Project([string]$ProjectName, [string]$SolutionRelativePath) {
    New-Item -ItemType directory -Path .\output\$ProjectName | out-null

    $solutionPath = Resolve-Path ".\sources\${ProjectName}\${SolutionRelativePath}"

    # The PROJECT env variable is used by 'SonarAnalyzer.Testing.ImportBefore.targets'
    $Env:PROJECT = $ProjectName

    Restore-Packages $solutionPath
    # Note: Summary doesn't work for MSBuild 14
    Invoke-MSBuild $msbuildVersion $solutionPath /m /t:rebuild /p:Configuration=Debug `
        /clp:"Summary;ErrorsOnly" `
        /fl /flp:"logFile=output\$ProjectName.log;verbosity=d"
}

function Initialize-ActualFolder {
    Write-Host "Initializing the actual issues Git repo with the expected ones"
    if (Test-Path .\actual) {
        Remove-Item -Recurse -Force actual
    }

    Copy-Item .\expected -Destination .\actual -Recurse
    Invoke-InLocation "actual" {
        Exec { git init }
        Exec { git config user.email "regression-test@example.com" }
        Exec { git config user.name "regression-test" }
        Exec { git add -A . }
        Exec { git commit -m "Initial commit with expected files" | out-null }
        Get-ChildItem * -Include *.json -Recurse | Remove-Item -Force
    }
}

function Initialize-OutputFolder {
    Write-Host "Initializing the output folder"
    if (Test-Path .\output) {
        Remove-Item -Recurse -Force output
    }
    New-Item -ItemType directory -Path .\output | out-null
    Copy-Item ".\AllSonarAnalyzerRules.ruleset" -Destination ".\output"
}

try {
    . (Join-Path $PSScriptRoot "..\..\scripts\build\build-utils.ps1")
    $msBuildImportBefore = Get-MSBuildImportBeforePath $msbuildVersion

    Push-Location $PSScriptRoot
    Test-SonarAnalyzerDll

    Write-Header "Initializing the environment"
    Initialize-ActualFolder
    Initialize-OutputFolder

    Write-Host "Installing the import before target file in '${msBuildImportBefore}'"
    Copy-Item .\SonarAnalyzer.Testing.ImportBefore.targets -Destination $msBuildImportBefore -Recurse -Container

    Build-Project "akka.net" "src\Akka.sln"
    Build-Project "Nancy" "src\Nancy.sln"
    Build-Project "Ember-MM" "Ember Media Manager.sln"

    Write-Header "Processing analyzer results"

    Write-Host "Normalizing the SARIF reports"
    Exec { & .\create-issue-reports.ps1 }

    Write-Host "Computing analyzer performance"
    Exec { & .\extract-analyzer-performances.ps1 }

    Write-Host "Checking for differences..."
    Invoke-InLocation "actual" {
        Exec { git add -A . }
        Exec { git diff --cached --exit-code | out-null } -errorMessage `
            "ERROR: There are differences between the actual and the expected issues."
    }

    Write-Host -ForegroundColor Green "SUCCESS: No differences were found!"
    exit 0
}
catch {
    Write-Host -ForegroundColor Red $_
    exit 1
}
finally {
    Pop-Location
    Write-Host "Removing the import before target file from '${msBuildImportBefore}'"
    Remove-Item -Force (Join-Path $msBuildImportBefore "\SonarAnalyzer.Testing.ImportBefore.targets") `
        -ErrorAction Ignore
}