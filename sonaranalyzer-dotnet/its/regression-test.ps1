[CmdletBinding(PositionalBinding = $false)]
param
(
    [ValidateSet("14.0", "15.0")]
    [string]
    $msbuildVersion = "14.0",

    [Parameter(HelpMessage = "The key of the rule to test, e.g. S1234. If omitted, all rule will be tested.")]
    [ValidatePattern("^S[0-9]+")]
    [string]
    $ruleId
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

function Initialize-ActualFolder() {
    $timer = [system.diagnostics.stopwatch]::StartNew()

    Write-Host "Initializing the actual issues folder with the expected  result"
    if (Test-Path .\actual) {
        Remove-Item -Recurse -Force actual
    }
    # this copies no files if ruleId is not set, and all but ending with ruleId if set

    Copy-FolderRecursively -From .\expected -To .\actual -Exclude "*${ruleId}.json"
    Write-Host "Initialize-ActualFolder:" $timer.Elapsed.TotalSeconds
}

function Initialize-OutputFolder() {
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
    Write-Host "Initialize-OutputFolder:" $timer.Elapsed.TotalSeconds
}

function Export-AnalyzerPerformancesFromLogs([string[]]$buildLogsPaths) {
    return $buildLogsPaths |
        Foreach-Object { Get-Content $_ } |
        Where-Object { $_ -match '^\s*<?([0-9.]+)\s*<?[0-9]+\s*(SonarAnalyzer\..*)$' -and ($matches[1] -ne '0.001') } |
        Foreach-Object {
            New-Object PSObject -Property @{
                Rule = $matches[2];
                Time = [decimal]$matches[1]}
        } |
        Group-Object Rule |
        Foreach-Object {
            New-Object PSObject -property @{
                Rule = $_.Name;
                Time = [math]::Round(($_.Group.Time | Measure-Object -sum).Sum, 3)}
        } |
        Sort-Object Time -Descending
}

function Get-FullPath($Folder) {
    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Folder))
}

function Copy-FolderRecursively($From, $To, $Include, $Exclude) {
    $fromPath = Get-FullPath -Folder $From
    $toPath   = Get-FullPath -Folder $To

    $files = if ($Include) {
        Get-ChildItem -Path $fromPath -Recurse -Include $Include
    } else {
        Get-ChildItem -Path $fromPath -Recurse -Exclude $Exclude
    }

    foreach ($file in $files) {
        $path = Join-Path $toPath $file.FullName.Substring($fromPath.length)
        $parent = split-path $path -parent
        if (-Not (Test-Path $parent)) {
            New-Item $parent -Type Directory -Force | out-null
        }
        Copy-Item $file -Destination $path
    }
}

function Measure-AnalyzerPerformance()
{
    # Process all build logs in the "output" folder
    $timings = Export-AnalyzerPerformancesFromLogs(Get-ChildItem output -filter *.log | Foreach-Object { $_.FullName })

    $timings | Where-Object { $_.Rule -match 'SonarAnalyzer\.CSharp.*' }                |
        Format-Table -Wrap -AutoSize | Out-String -Width 100
    $timings | Where-Object { $_.Rule -match 'SonarAnalyzer\.VisualBasic.*' }           |
        Format-Table -Wrap -AutoSize | Out-String -Width 100
    $timings | Where-Object { $_.Rule -match 'SonarAnalyzer\.Rules\.CSharp.*' }         |
        Format-Table -Wrap -AutoSize | Out-String -Width 100
    $timings | Where-Object { $_.Rule -match 'SonarAnalyzer\.Rules\.VisualBasic.*' }    |
        Format-Table -Wrap -AutoSize | Out-String -Width 100
}

function Show-DiffResults() {
    if (Test-Path .\diff) {
        Remove-Item -Recurse -Force .\diff
    }

    $errorMsg = "ERROR: There are differences between the actual and the expected issues."
    if (!$ruleId)
    {
        Exec { & git diff --no-index --quiet --exit-code ./expected ./actual } `
              -errorMessage $errorMsg
        return
    }

    New-Item ".\diff" -Type Directory | out-null
    New-Item ".\diff\actual" -Type Directory | out-null
    New-Item ".\diff\expected" -Type Directory | out-null

    Copy-FolderRecursively -From .\expected -To .\diff\expected -Include "*${ruleId}.json"
    Copy-FolderRecursively -From .\actual   -To .\diff\actual   -Include "*${ruleId}.json"

    Exec { & git diff --no-index --quiet --exit-code .\diff\expected .\diff\actual } `
          -errorMessage $errorMsg
}

try {
    $scriptTimer = [system.diagnostics.stopwatch]::StartNew()
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
    $sarifTimer = [system.diagnostics.stopwatch]::StartNew()
        Exec { & .\create-issue-reports.ps1 }
    Write-Host "Normalizing the SARIF reports:" $sarifTimer.Elapsed.TotalSeconds

    Write-Host "Computing analyzer performance"
    $measurePerfTimer = [system.diagnostics.stopwatch]::StartNew()
    Measure-AnalyzerPerformance
    Write-Host "Computing analyzer performance:" $measurePerfTimer.Elapsed.TotalSeconds

    Write-Host "Checking for differences..."
    $diffTimer = [system.diagnostics.stopwatch]::StartNew()
    Show-DiffResults
    Write-Host "Checking for differences..." $diffTimer.Elapsed.TotalSeconds

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

    $scriptTimer.Stop()
    $totalTimesec = [int]$scriptTimer.Elapsed.TotalSeconds
    if ($ruleId) {
        Write-Host "Analyzed ${ruleId} in ${totalTimesec}s"
    } else {
        Write-Host "Analyzed all rules in ${totalTimesec}s"
    }
}