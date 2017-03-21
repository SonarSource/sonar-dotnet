function testExitCode(){
    If($LASTEXITCODE -ne 0) {
        write-host -f green "lastexitcode: $LASTEXITCODE"
        exit $LASTEXITCODE
    }
}

# Possible outcomes
function sucess(){
    Write-Host "SUCCESS: No differences were found!"    
    cleanup "0"
}

function build_error(){
    param ([string]$PROJECT)
    Write-Host "ERROR: The project $PROJECT could not be built!"
    cleanup "1"
}

function diff_error(){
    Write-Host "ERROR: There are differences between the actual and the expected issues!"
    cleanup "2"
}

function ps_error(){
    Write-Host "ERROR: Error while executing an internal PowerShell script"
    cleanup "3"
}

# Cleanup
function cleanup(){
    param ([string]$exit)
    # Uninstall the imports before targets file
    rm -force "$env:USERPROFILE\AppData\Local\Microsoft\MSBuild\14.0\Microsoft.Common.targets\ImportBefore\SonarAnalyzer.Testing.ImportBefore.targets"
    # Restore current working directory
    Pop-Location -StackName "regression"
    # Exit
    exit $exit
}

# Building projects
function buildProject(){
    param ([string]$PROJECT)
    Write-Host "Building: $PROJECT"
    New-Item -ItemType directory -Path .\output\$PROJECT | out-null
    Push-Location $PROJECT
    & .\sonarlint-build.bat > ..\output\$PROJECT.txt
    If($LASTEXITCODE -ne 0) { build_error }
    Pop-Location
}


#generate rulesest file
function rulesetGenerator(){
    $ErrorActionPreference = "Stop"

    $max = 10000
$s=@"
<?xml version="1.0" encoding="utf-8"?>
<RuleSet Name="AllSonarAnalyzerRules" Description="Ruleset used to test for rule regressions." ToolsVersion="14.0">
<!-- AD0001, the analyzer catching other analyzer exceptions, should be enabled at Error level -->
  <Rules AnalyzerId="Roslyn.Core" RuleNamespace="Roslyn.Core">
    <Rule Id="AD0001" Action="Error" />
  </Rules>

<!-- This list is just hardcoded for now with plausible existing & upcoming rule IDs -->
<!-- It would be better to generate the actual list from the analyzer assemblies -->
  <Rules AnalyzerId="SonarAnalyzer.CSharp" RuleNamespace="SonarAnalyzer.CSharp">
"@
    for ($i = 0; $i -le $max; $i++) { 
        $s+="    <Rule Id=""S$i"" Action=""Warning"" />`r`n" 
    }
$s+=@"
  </Rules>
  <Rules AnalyzerId="SonarAnalyzer.VisualBasic" RuleNamespace="SonarAnalyzer.VisualBasic">
"@
    for ($i = 0; $i -le $max; $i++) { $s+="    <Rule Id=""S$i"" Action=""Warning"" />`r`n" }
$s+=@"
  </Rules>
</RuleSet>
"@
    $s | out-file -encoding utf8 -append ".\output\AllSonarAnalyzerRules.ruleset"
}

# Set the current working directory to the script's folder
Push-Location -Path $PSScriptRoot -StackName "regression"

# Setup the actual folder with the expected files
Write-Host "Initializing the actual issues Git repo with the expected ones..."
if (Test-Path .\actual) { rm -Recurse -force actual }
Copy-Item .\expected -Destination .\actual -Recurse
Push-Location actual
git init
testExitCode
git config user.email "regression-test@example.com"
testExitCode
git config user.name "regression-test"
testExitCode
git add -A .
testExitCode
git commit -m "Initial commit with expected files" | out-null
testExitCode
rm -Recurse -force *.json 
Pop-Location

# Setup output folder
if (Test-Path .\output) { rm -Recurse -force output }
New-Item -ItemType directory -Path .\output | out-null

# Generate the SonarAnalyzer all rules ruleset
rulesetGenerator
# Install the imports before targets file
Copy-Item .\SonarAnalyzer.Testing.ImportBefore.targets -Destination "$env:USERPROFILE\AppData\Local\Microsoft\MSBuild\14.0\Microsoft.Common.targets\ImportBefore" -Recurse

buildProject "akka.net"
buildProject "Nancy"
buildProject "Ember-MM"

# Normalize SARIF reports
Write-Host "Normalizing the SARIF reports"
& ./CreateIssueReports.ps1
If($LASTEXITCODE -ne 0) { ps_error }

# Compute analyzer performances
Write-Host "Computing analyzer performances"
& ./ExtractAnalyzerPerformancesFromLogs.ps1
If($LASTEXITCODE -ne 0) { ps_error }

# Assert no differences
Write-Host "Checking for differences..."
Push-Location actual
git add -A .
testExitCode
git diff --cached --exit-code | out-null
If($LASTEXITCODE -ne 0) { diff_error }
Pop-Location


