[CmdletBinding(PositionalBinding = $false)]
param
(
    [Parameter(HelpMessage = "Version of MS Build: 14.0, 15.0, 16.0 or Current")]
    [ValidateSet("14.0", "15.0", "16.0", "Current")]
    [string]
    $msbuildVersion = "Current",

    [Parameter(HelpMessage = "The key of the rule to test, e.g. S1234. If omitted, all rules will be tested.")]
    [ValidatePattern("^S[0-9]+")]
    [string]
    $ruleId,

    [Parameter(HelpMessage = "The name of single project to build. If ommited, all projects will be build.")]
    [ValidateSet("AnalyzeGenerated", "AnalyzeGeneratedVb", "akka.net", "Automapper", "Ember-MM", "Nancy", "ManuallyAddedNoncompliantIssues", "ManuallyAddedNoncompliantIssuesVB", "SkipGenerated", "SkipGeneratedVb")]
    [string]
    $project
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

if ($PSBoundParameters['Verbose'] -Or $PSBoundParameters['Debug']) {
    $global:DebugPreference = "Continue"
}

function Build-Project([string]$ProjectName, [string]$SolutionRelativePath, [int]$CpuCount = 4) {
    if ($project -And -Not ($ProjectName -eq $project)) {
        Write-Host "Build skipped: $ProjectName"
        return
    }

    New-Item -ItemType directory -Path .\output\$ProjectName | out-null

    $solutionPath = Resolve-Path ".\sources\${ProjectName}\${SolutionRelativePath}"

    # The PROJECT env variable is used by 'SonarAnalyzer.Testing.ImportBefore.targets'
    Write-Debug "Setting PROJECT environment variable to '${ProjectName}'"
    $Env:PROJECT = $ProjectName

    Restore-Packages $msbuildVersion $solutionPath
    # Note: Summary doesn't work for MSBuild 14
    Invoke-MSBuild $msbuildVersion $solutionPath `
        /m:$CpuCount `
        /t:rebuild `
        /p:Configuration=Debug `
        /clp:"Summary;ErrorsOnly" `
        /fl `
        /flp:"logFile=output\${ProjectName}.log;verbosity=d"
}

function Initialize-ActualFolder() {
    $methodTimer = [system.diagnostics.stopwatch]::StartNew()

    Write-Host "Initializing the actual issues folder with the expected result"
    if (Test-Path .\actual) {
        Write-Host "Removing existing folder 'actual'"
        Remove-Item -Recurse -Force actual
    }

    # this copies no files if ruleId is not set, and all but ending with ruleId if set
    Copy-FolderRecursively -From .\expected -To .\actual -Exclude "*${ruleId}.json"
    $methodTimerElapsed = $methodTimer.Elapsed.TotalSeconds
    Write-Debug "Initialized actual folder in '${methodTimerElapsed}'"
}

# FIXME: this is a hacky way of avoiding diffing problems with temporary generated files in the %TEMP% folder
function ReplaceGeneratedTempFileName([string]$folder){

    Write-Host "Will replace generated temporary file names from the jsons in ${folder}:"
    $files = Get-ChildItem -Path $folder -Recurse -File
    $tempFileNameLineRegexCs = '"uri":.*\.NETFramework,Version=.*\.cs"'
    $tempFileNameLineRegexVb = '"uri":.*\.NETFramework,Version=.*\.vb"'

    foreach ($file in $files){

        $fullName = $file.FullName
        Write-Host "Will replace the generated temporary file names inside $fullName..."

        $data = [System.IO.File]::ReadAllText(${fullName})
        $data = [System.Text.RegularExpressions.Regex]::Replace($data, $tempFileNameLineRegexCs, '"uri": "replaced"')
        $data = [System.Text.RegularExpressions.Regex]::Replace($data, $tempFileNameLineRegexVb, '"uri": "replaced"')
        [System.IO.File]::WriteAllText(${fullName}, $data)
    }
}

function Initialize-OutputFolder() {
    $methodTimer = [system.diagnostics.stopwatch]::StartNew()

    Write-Host "Initializing the output folder"
    if (Test-Path .\output) {
        Write-Host "Removing existing folder 'output'"
        Remove-Item -Recurse -Force output
    }

    Write-Debug "Creating folder 'output'"
    New-Item -ItemType directory -Path .\output | out-null

    if ($ruleId) {
        Write-Host "Running ITs with only rule ${ruleId} turned on"
        $template = Get-Content -Path ".\SingleRule.ruleset.template" -Raw
        $rulesetWithOneRule = $template.Replace("<Rule Id=`"$ruleId`" Action=`"None`" />", `
                                                "<Rule Id=`"$ruleId`" Action=`"Warning`" />")
        Set-Content -Path ".\output\AllSonarAnalyzerRules.ruleset" -Value $rulesetWithOneRule
    }
    else {
        Write-Host "Running ITs with all rules turned on"
        Copy-Item ".\AllSonarAnalyzerRules.ruleset" -Destination ".\output"
    }

    $methodTimerElapsed = $methodTimer.Elapsed.TotalSeconds
    Write-Debug "Initialized output folder in '${methodTimerElapsed}'"
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

function Measure-AnalyzerPerformance(){
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
        Write-Host "Removing existing folder 'diff'"
        Remove-Item -Recurse -Force .\diff
    }

    $errorMsg = "ERROR: There are differences between the actual and the expected issues."
    if (!$ruleId)
    {
        Write-Debug "Running 'git diff' between 'actual' and 'expected'"
        Exec { & git diff --no-index --quiet --exit-code ./expected ./actual `
        } -errorMessage $errorMsg
        return
    }

    New-Item ".\diff" -Type Directory | out-null
    New-Item ".\diff\actual" -Type Directory | out-null
    New-Item ".\diff\expected" -Type Directory | out-null

    Copy-FolderRecursively -From .\expected -To .\diff\expected -Include "*${ruleId}.json"
    Copy-FolderRecursively -From .\actual   -To .\diff\actual   -Include "*${ruleId}.json"

    Exec { & git diff --no-index --quiet --exit-code .\diff\expected .\diff\actual `
    } -errorMessage $errorMsg
}

try {
    $scriptTimer = [system.diagnostics.stopwatch]::StartNew()
    . (Join-Path $PSScriptRoot "..\..\scripts\build\build-utils.ps1")
    $msBuildImportBefore14 = Get-MSBuildImportBeforePath "14.0"
    $msBuildImportBefore15 = Get-MSBuildImportBeforePath "15.0"
    $msBuildImportBefore16 = Get-MSBuildImportBeforePath "16.0"
    $msBuildImportBeforeCurrent = Get-MSBuildImportBeforePath "Current"

    Push-Location $PSScriptRoot
    Test-FileExists ".\binaries\SonarAnalyzer.dll"
    Test-FileExists ".\binaries\SonarAnalyzer.CFG.dll"

    Write-Header "Initializing the environment"
    Initialize-ActualFolder
    Initialize-OutputFolder

    Write-Debug "Installing the import before target file at '${msBuildImportBefore14}'"
    Copy-Item .\SonarAnalyzer.Testing.ImportBefore.targets -Destination (New-Item $msBuildImportBefore14 -Type container -Force) -Force -Recurse
    Write-Debug "Installing the import before target file at '${msBuildImportBefore15}'"
    Copy-Item .\SonarAnalyzer.Testing.ImportBefore.targets -Destination (New-Item $msBuildImportBefore15 -Type container -Force) -Force -Recurse
    Write-Debug "Installing the import before target file at '${msBuildImportBefore16}'"
    Copy-Item .\SonarAnalyzer.Testing.ImportBefore.targets -Destination (New-Item $msBuildImportBefore16 -Type container -Force) -Force -Recurse
    Write-Debug "Installing the import before target file at '${msBuildImportBeforeCurrent}'"
    Copy-Item .\SonarAnalyzer.Testing.ImportBefore.targets -Destination (New-Item $msBuildImportBeforeCurrent -Type container -Force) -Force -Recurse

    # Note: Automapper has multiple configurations that are built simultaneously and sometimes
    # it happens that a the same project is built in parallel in different configurations. The
    # protobuf-generating rules try to write their output in the same folder and fail, even
    # though there is a basic lock, because it is process-wide and not machine-wide.
    # Parallel builds are not a problem when run through the Scanner for MSBuild because it
    # redirects the outputs of the different configurations in separate folders.

    # Do not forget to update ValidateSet of -project parameter when new project is added.
    Build-Project "AnalyzeGenerated" "AnalyzeGeneratedFiles.sln"
    Build-Project "AnalyzeGeneratedVb" "AnalyzeGeneratedVb.sln"
    Build-Project "akka.net" "src\Akka.sln"
    Build-Project "Automapper" "Automapper.sln" -CpuCount 1
    Build-Project "Ember-MM" "Ember Media Manager.sln"
    Build-Project "ManuallyAddedNoncompliantIssues" "ManuallyAddedNoncompliantIssues.sln"
    Build-Project "ManuallyAddedNoncompliantIssuesVB" "ManuallyAddedNoncompliantIssuesVB.sln"
    Build-Project "Nancy" "src\Nancy.sln"
    Build-Project "SkipGenerated" "SkipGeneratedFiles.sln"
    Build-Project "SkipGeneratedVb" "SkipGeneratedVb.sln"

    Write-Header "Processing analyzer results"

    Write-Host "Normalizing the SARIF reports"
    $sarifTimer = [system.diagnostics.stopwatch]::StartNew()
    Exec { & .\create-issue-reports.ps1 }
    $sarifTimerElapsed = $sarifTimer.Elapsed.TotalSeconds
    Write-Debug "Normalized the SARIF reports in '${sarifTimerElapsed}'"

    Write-Host "Computing analyzer performance"
    $measurePerfTimer = [system.diagnostics.stopwatch]::StartNew()
    Measure-AnalyzerPerformance
    $measurePerfTimerElapsed = $measurePerfTimer.Elapsed.TotalSeconds
    Write-Debug "Computed analyzer performance in '${measurePerfTimerElapsed}'"

    # FIXME: this is a hacky way of diffing the issues found on the temporary generated files during build
    ReplaceGeneratedTempFileName(".\expected\AnalyzeGenerated")
    ReplaceGeneratedTempFileName(".\actual\AnalyzeGenerated")
    ReplaceGeneratedTempFileName(".\expected\AnalyzeGeneratedVb")
    ReplaceGeneratedTempFileName(".\actual\AnalyzeGeneratedVb")

    Write-Host "Checking for differences..."
    $diffTimer = [system.diagnostics.stopwatch]::StartNew()
    Show-DiffResults
    $diffTimerElapsed = $diffTimer.Elapsed.TotalSeconds
    Write-Debug "Checked for differences in '${diffTimerElapsed}'"

    Write-Host -ForegroundColor Green "SUCCESS: ITs were successful! No differences were found!"
    exit 0
}
catch {
    Write-Host -ForegroundColor Red $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
finally {
    Pop-Location
    Write-Debug "Removing the import before target file from '${msBuildImportBefore14}'"
    Remove-Item -Force (Join-Path $msBuildImportBefore14 "\SonarAnalyzer.Testing.ImportBefore.targets") `
        -ErrorAction Ignore

    Write-Debug "Removing the import before target file from '${msBuildImportBefore15}'"
    Remove-Item -Force (Join-Path $msBuildImportBefore15 "\SonarAnalyzer.Testing.ImportBefore.targets") `
        -ErrorAction Ignore

    Write-Debug "Removing the import before target file from '${msBuildImportBefore16}'"
    Remove-Item -Force (Join-Path $msBuildImportBefore16 "\SonarAnalyzer.Testing.ImportBefore.targets") `
        -ErrorAction Ignore

    Write-Debug "Removing the import before target file from '${msBuildImportBeforeCurrent}'"
    Remove-Item -Force (Join-Path $msBuildImportBeforeCurrent "\SonarAnalyzer.Testing.ImportBefore.targets") `
        -ErrorAction Ignore

    $scriptTimer.Stop()
    $totalTimeInSeconds = [int]$scriptTimer.Elapsed.TotalSeconds
    if ($ruleId) {
        Write-Debug "Analyzed ${ruleId} in ${totalTimeInSeconds}s"
    } else {
        Write-Debug "Analyzed all rules in ${totalTimeInSeconds}s"
    }
}
