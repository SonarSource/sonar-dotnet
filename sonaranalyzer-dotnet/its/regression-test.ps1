$importBeforePath = "$env:USERPROFILE\AppData\Local\Microsoft\MSBuild\14.0\Microsoft.Common.targets\ImportBefore"

. (Join-Path $PSScriptRoot "..\build\build-utils.ps1")

function Test-SonarAnalyzerDll {
    if (-Not (Test-Path ".\binaries\SonarAnalyzer.dll")) {
        throw "Could not find '.\binaries\SonarAnalyzer.dll'."
    }
}

function Build-Project([string]$ProjectName, [string]$SolutionRelativePath) {
    New-Item -ItemType directory -Path .\output\$ProjectName | out-null

    $solutionPath = (Resolve-Path ".\sources\${ProjectName}\${SolutionRelativePath}")

    # The PROJECT env variable is used by 'SonarAnalyzer.Testing.ImportBefore.targets'
    $Env:PROJECT = $ProjectName

    (Build-Solution $solutionPath /v:detailed /m /t:rebuild /p:Configuration=Debug) `
        > .\output\$ProjectName.log
}

function Initialize-ActualFolder {
    Write-Host "Initializing the actual issues Git repo with the expected ones..."
    if (Test-Path .\actual) {
        Remove-Item -Recurse -Force actual
    }

    Copy-Item .\expected -Destination .\actual -Recurse
    Exec-InLocation "actual" {
        Exec { git init }
        Exec { git config user.email "regression-test@example.com" }
        Exec { git config user.name "regression-test" }
        Exec { git add -A . }
        Exec { git commit -m "Initial commit with expected files" | out-null }
        Get-ChildItem * -Include *.json -Recurse | Remove-Item -Force
    }
}

function Initialize-OutputFolder {
    if (Test-Path .\output) {
        Remove-Item -Recurse -Force output
    }
    New-Item -ItemType directory -Path .\output | out-null
    Copy-Item ".\AllSonarAnalyzerRules.ruleset" -Destination ".\output"
}

try {
    Push-Location -Path $PSScriptRoot

    Test-SonarAnalyzerDll

    Initialize-ActualFolder
    Initialize-OutputFolder

    Write-Host "Installing the imports before targets file"
    Copy-Item .\SonarAnalyzer.Testing.ImportBefore.targets -Destination $importBeforePath -Recurse -Container

    Build-Project "akka.net" "src\Akka.sln"
    Build-Project "Nancy" "src\Nancy.sln"
    Build-Project "Ember-MM" "Ember Media Manager.sln"

    Write-Host "Normalizing the SARIF reports"
    Exec { & ./CreateIssueReports.ps1 | out-null }

    Write-Host "Computing analyzer performance"
    Exec { & ./ExtractAnalyzerPerformancesFromLogs.ps1 }

    Write-Host "Checking for differences..."
    Exec-InLocation "actual" {
        Exec { git add -A . }
        Exec { git diff --cached --exit-code | out-null } -errorMessage "ERROR: There are differences between the actual and the expected issues."
    }

    Write-Host -ForegroundColor Green "SUCCESS: No differences were found!"
    exit 0
}
catch {
    Write-Host -ForegroundColor Red $_.Exception.Message
    exit 1
}
finally {
    Pop-Location
    Remove-Item -Force "${importBeforePath}\SonarAnalyzer.Testing.ImportBefore.targets" -ErrorAction Ignore
}