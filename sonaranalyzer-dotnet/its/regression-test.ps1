[CmdletBinding(PositionalBinding = $false)]
param
(
    [ValidateSet("14.0", "15.0")]
    [string]
    $msbuildVersion = "14.0",

    [Parameter(HelpMessage = "The key of the rule to test, e.g. S1234. If omitted, all rule will be tested.")]
    [ValidatePattern("^S[0-9]+$")]
    [string]
    $singleRuleIdToTest = "S101" # TODO: REMOVE!
)

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

function Initialize-ActualFolder($ruleId) {
    $timer = [system.diagnostics.stopwatch]::StartNew()
    
    Write-Host "Initializing the actual issues Git repo with the expected ones"
    if (Test-Path .\actual) {
        Remove-Item -Recurse -Force actual
    }

    Copy-Item .\expected -Destination .\actual -Recurse
    Invoke-InLocation "actual" {

        # if ($ruleId) {
        #     Copy-Item * -Exclude "*${ruleId}.json" -Recurse | Remove-Item -Force
        # }
        # else {
        #     Copy-Item * -Include "*${ruleId}.json" -Recurse | Remove-Item -Force
        # }




        if ($ruleId) {
            Get-ChildItem * -Include "*${ruleId}.json" -Recurse | Remove-Item -Force
        }
        else {
            Get-ChildItem * -Include *.json -Recurse | Remove-Item -Force
        }
        Write-Host "Initialize-ActualFolder:" + $timer.Elapsed.TotalSeconds
    }
}

function Initialize-OutputFolder($ruleId) {
    $timer = [system.diagnostics.stopwatch]::StartNew()
    
    Write-Host "Initializing the output folder"
    if (Test-Path .\output) {
        Remove-Item -Recurse -Force output
    }
    New-Item -ItemType directory -Path .\output | out-null

    if ($ruleId) {
        $template = Get-Content -Path ".\SingleRule.ruleset.template" -Raw
        $rulesetWithOneRule = $template.Replace("<Rule Id=`"$ruleId`" Action=`"None`" />", `
                                                "<Rule Id=`"$ruleId`" Action=`"Warning`" />")
        Set-Content -Path ".\output\AllSonarAnalyzerRules.ruleset" -Value $rulesetWithOneRule
    }
    else {
        Copy-Item ".\AllSonarAnalyzerRules.ruleset" -Destination ".\output"
    }
    Write-Host "Initialize-OutputFolder:" + $timer.Elapsed.TotalSeconds
    
}

try {
    $sw = [system.diagnostics.stopwatch]::StartNew()
    . (Join-Path $PSScriptRoot "..\..\scripts\build\build-utils.ps1")
    $msBuildImportBefore = Get-MSBuildImportBeforePath $msbuildVersion

    Push-Location $PSScriptRoot
    Test-SonarAnalyzerDll

    Write-Header "Initializing the environment"
    Initialize-ActualFolder -ruleId $singleRuleIdToTest
    Initialize-OutputFolder -ruleId $singleRuleIdToTest

    Write-Host "Installing the import before target file in '${msBuildImportBefore}'"
    Copy-Item .\SonarAnalyzer.Testing.ImportBefore.targets -Destination $msBuildImportBefore -Recurse -Container

    Build-Project "akka.net" "src\Akka.sln"
    Build-Project "Nancy" "src\Nancy.sln"
    Build-Project "Ember-MM" "Ember Media Manager.sln"

    Write-Header "Processing analyzer results"

    Write-Host "Normalizing the SARIF reports"
    $timer2 = [system.diagnostics.stopwatch]::StartNew()
        Exec { & .\create-issue-reports.ps1 }
    Write-Host "Normalizing the SARIF reports:" + $timer2.Elapsed.TotalSeconds
        
    Write-Host "Computing analyzer performance"
    $timer3 = [system.diagnostics.stopwatch]::StartNew()
        Exec { & .\extract-analyzer-performances.ps1 }
    Write-Host "Computing analyzer performance:" + $timer3.Elapsed.TotalSeconds
        
    Write-Host "Checking for differences..."
    $timer4 = [system.diagnostics.stopwatch]::StartNew()
    Exec { git diff --no-index --quiet --exit-code ./expected ./actual } -errorMessage `
        "ERROR: There are differences between the actual and the expected issues."
    Write-Host "Checking for differences..." + $timer4.Elapsed.TotalSeconds
        

    # Invoke-InLocation "actual" {
    #     Exec { git add -A . }
    #     Exec { git diff --cached --exit-code | out-null } -errorMessage `
    #         "ERROR: There are differences between the actual and the expected issues."
    # }

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

    $sw.Stop()
    $totalTimesec = [int]$sw.Elapsed.TotalSeconds
    Write-Host "Total time: ${totalTimesec}s"
}