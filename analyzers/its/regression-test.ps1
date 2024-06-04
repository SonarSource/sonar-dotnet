[CmdletBinding(PositionalBinding = $false)]
param
(
    [Parameter(HelpMessage = "The key of the rule to test, e.g. S1234. If omitted, all rules will be tested.")]
    [ValidatePattern("^S[0-9]+")]
    [string]
    $ruleId,

    [Parameter(HelpMessage = "The name of single project to build. If ommited, all projects will be build.")]
    [ValidateSet("AnalyzeGenerated.CS", "AnalyzeGenerated.VB", "akka.net", "AutoMapper", "BlazorSample", "CSharpLatest", "Ember-MM", "Nancy", "ManuallyAddedNoncompliantIssues.CS", "ManuallyAddedNoncompliantIssues.VB", "RazorSample", "Roslyn.1.3.1", "SkipGenerated.CS", "SkipGenerated.VB", "SonarLintExclusions", "WebConfig")]
    [string]
    $project
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"
$DifferencesMsg = "ERROR: There are differences between the actual and the expected issues."

if ($PSBoundParameters['Verbose'] -Or $PSBoundParameters['Debug']) {
    $global:DebugPreference = "Continue"
}

function Prepare-Project([string]$ProjectName){
    $Output = ".\output\$ProjectName"
    New-Item -ItemType directory -Path $Output | out-null

    $SourcePath = ".\config\$ProjectName\SonarLint.xml"
    if(-Not (Test-Path $SourcePath)){
        $SourcePath = ".\config\SonarLint.xml"
    }
    $Content = Get-Content -Path $SourcePath -Raw

    if($ruleId){
        $RuleFragment = "    <Rule><Key>$ruleId</Key></Rule>"
    } else {
        $HotspotFiles = Get-ChildItem ..\rspec -Filter *.json -Recurse | Select-String "SECURITY_HOTSPOT" | Select-Object -ExpandProperty FileName
        $HotspotIDs = $HotspotFiles -Replace ".json", "" | Select-Object -Unique
        $RuleFragment = ""
        foreach($HotspotID in $HotspotIDs){
            $RuleFragment = $RuleFragment + "    <Rule><Key>$HotspotID</Key></Rule>`n"
        }
    }

    $Content = $Content -Replace "<Rules>", "<Rules>`n$RuleFragment"
    Set-Content -Path "$Output\SonarLint.xml" -Value $Content

    Write-Host "Using $Output\SonarLint.xml"
}

function Build-Project-MSBuild([string]$ProjectName, [string]$SolutionRelativePath, [int]$CpuCount = 4) {
    if ($project -And -Not ($ProjectName -eq $project)) {
        Write-Host "Build skipped: $ProjectName"
        return
    }

    Prepare-Project($ProjectName)

    $solutionPath = Resolve-Path ".\Projects\${ProjectName}\${SolutionRelativePath}"

    # The PROJECT env variable is used by 'Directory.Build.targets'
    Write-Debug "Setting PROJECT environment variable to '${ProjectName}'"
    $Env:PROJECT = $ProjectName

    Restore-Packages $solutionPath
    Invoke-MSBuild $solutionPath `
        /m:$CpuCount `
        /t:rebuild `
        /p:Configuration=Debug `
        /clp:"Summary;ErrorsOnly" `
        /fl `
        /flp:"logFile=output\${ProjectName}\Build.log;verbosity=d"
}

function Build-Project-DotnetTool([string]$ProjectName, [string]$SolutionRelativePath) {
    if ($project -And -Not ($ProjectName -eq $project)) {
        Write-Host "Build skipped: $ProjectName"
        return
    }

    $projectGlobalJsonPath = ".\Projects\${ProjectName}\global.json"
    $globalJsonContent = $(Get-Content $projectGlobalJsonPath)
    Write-Host "Will build dotnet project: '${ProjectName}' (with ${projectGlobalJsonPath}) with dotnet version '${globalJsonContent}'."

    Prepare-Project($ProjectName)

    $solutionPath = Resolve-Path ".\Projects\${ProjectName}\${SolutionRelativePath}"

    # The PROJECT env variable is used by 'Directory.Build.targets'
    Write-Debug "Setting PROJECT environment variable to '${ProjectName}'"
    $Env:PROJECT = $ProjectName

    # Copy the global.json in the folder where we do analysis.
    $tempGlobalJsonPath = ".\global.json"
    Copy-Item $projectGlobalJsonPath $tempGlobalJsonPath

    dotnet --version
    dotnet restore --locked-mode $solutionPath

    # To change the verbosity, comment out the '-clp' parameter and add the '-v' parameter.
    Exec { & dotnet build $solutionPath `
        --no-restore `
        -t:rebuild `
        -p:Configuration=Debug `
        -clp:"Summary;ErrorsOnly" `
        -fl `
        -flp:"logFile=output\${ProjectName}\Build.log;verbosity=d" `
    } -errorMessage "ERROR: Build FAILED."

    Remove-Item $tempGlobalJsonPath
}

function Initialize-ActualFolder() {
    Write-Host "Initializing the actual issues folder with the expected result"
    if (Test-Path .\actual) {
        Write-Host "Removing existing folder 'actual'"
        Remove-Item -Recurse -Force actual
    }

    # this copies no files if ruleId is not set, and all but ending with ruleId if set
    Copy-FolderRecursively -From .\expected -To .\actual -Exclude "${ruleId}*.json"
}

function Initialize-OutputFolder() {
    Write-Host "Initializing the output folder"
    if (Test-Path .\output) {
        Write-Host "Removing existing folder 'output'"
        Remove-Item -Recurse -Force output
    }

    Write-Debug "Creating folder 'output'"
    New-Item -ItemType directory -Path .\output | out-null

    if ($ruleId) {
        Write-Host "Running ITs with only rule ${ruleId} turned on."
        $template = Get-Content -Path ".\SingleRule.ruleset.template" -Raw
        $rulesetWithOneRule = $template.Replace("<Rule Id=`"$ruleId`" Action=`"None`" />", `
                                                "<Rule Id=`"$ruleId`" Action=`"Warning`" />")
        Set-Content -Path ".\output\AllSonarAnalyzerRules.ruleset" -Value $rulesetWithOneRule
    }
    else {
        Write-Host "Running ITs with all rules turned on."
        Copy-Item ".\AllSonarAnalyzerRules.ruleset" -Destination ".\output"
    }

    Write-Host "The rule set we use is .\output\AllSonarAnalyzerRules.ruleset."
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
        Copy-Item $file.FullName -Destination $path
    }
}

function Show-DiffResults() {
    if (Test-Path .\diff) {
        Write-Host "Removing existing folder 'diff'"
        Remove-Item -Recurse -Force .\diff
    }

    if (!$ruleId -And !$project)
    {
        Write-Host "Will find differences for all projects, all rules."

        Exec { & git diff --no-index --exit-code ./expected ./actual } -errorMessage $DifferencesMsg
    }
    else
    {
        # do a partial diff
        New-Item ".\diff" -Type Directory | out-null
        New-Item ".\diff\actual" -Type Directory | out-null
        New-Item ".\diff\expected" -Type Directory | out-null

        if (!$ruleId -And $project) {
            Write-Host "Will find differences for '${project}', all rules."

            Copy-FolderRecursively -From ".\expected\${project}" -To .\diff\expected
            Copy-FolderRecursively -From ".\actual\${project}"   -To .\diff\actual

        } elseif ($ruleId -And !$project) {
            Write-Host "Will find differences for all projects, rule ${ruleId}."

            Copy-FolderRecursively -From .\expected -To .\diff\expected -Include "$*{ruleId}.json"
            Copy-FolderRecursively -From .\actual   -To .\diff\actual   -Include "$*{ruleId}.json"

        } else {
            Write-Host "Will find differences for '${project}', rule ${ruleId}."

            Copy-FolderRecursively -From ".\expected\${project}" -To .\diff\expected -Include "*${ruleId}.json"
            Copy-FolderRecursively -From ".\actual\${project}"   -To .\diff\actual   -Include "*${ruleId}.json"
        }

        Exec { & git diff --no-index --exit-code .\diff\expected .\diff\actual } -errorMessage $DifferencesMsg
    }
}

function Invoke-JsonParser()
{
    $JsonParser = Join-Path $PSScriptRoot "..\packaging\binaries\ITs.JsonParser\ITs.JsonParser.exe"
    $Arguments = @(" ")
    if ($ruleId){
        $Arguments += "--rule"
        $Arguments += $ruleId
    }
    if ($project){
        $Arguments += "--project"
        $Arguments += $project
    }
    Start-Process $JsonParser -ArgumentList $Arguments -NoNewWindow -Wait
}

try {
    . (Join-Path $PSScriptRoot "..\..\scripts\build\build-utils.ps1")
    Push-Location $PSScriptRoot
    Test-FileExists "..\packaging\binaries\SonarAnalyzer.dll"
    Test-FileExists "..\packaging\binaries\SonarAnalyzer.CFG.dll"
    Test-FileExists "..\packaging\binaries\ITs.JsonParser\ITs.JsonParser.exe"

    Write-Header "Initializing the environment"
    Initialize-ActualFolder
    Initialize-OutputFolder

    # Note: Automapper has multiple configurations that are built simultaneously and sometimes
    # it happens that a the same project is built in parallel in different configurations. The
    # protobuf-generating rules try to write their output in the same folder and fail, even
    # though there is a basic lock, because it is process-wide and not machine-wide.
    # Parallel builds are not a problem when run through the SonarScanner for .NET because it
    # redirects the outputs of the different configurations in separate folders.

    # Do not forget to update ValidateSet of -project parameter when new project is added.
    Build-Project-MSBuild "ManuallyAddedNoncompliantIssues.CS" "ManuallyAddedNoncompliantIssues.CS.sln"
    Build-Project-MSBuild "ManuallyAddedNoncompliantIssues.VB" "ManuallyAddedNoncompliantIssues.VB.sln"
    Build-Project-MSBuild "AnalyzeGenerated.CS" "AnalyzeGenerated.CS.sln"
    Build-Project-MSBuild "AnalyzeGenerated.VB" "AnalyzeGenerated.VB.sln"
    Build-Project-MSBuild "Ember-MM" "Ember Media Manager.sln"
    Build-Project-MSBuild "Nancy" "Nancy.sln"
    Build-Project-MSBuild "Roslyn.1.3.1" "Roslyn.1.3.1.sln"
    Build-Project-MSBuild "SkipGenerated.CS" "SkipGenerated.CS.sln"
    Build-Project-MSBuild "SkipGenerated.VB" "SkipGenerated.VB.sln"
    Build-Project-MSBuild "WebConfig" "WebConfig.sln"

    Build-Project-DotnetTool "akka.net" "src\Akka.sln"
    Build-Project-DotnetTool "AutoMapper" "AutoMapper.sln"
    Build-Project-DotnetTool "SonarLintExclusions" "SonarLintExclusions.sln"
    Build-Project-DotnetTool "RazorSample" "RazorSample.sln"
    Build-Project-DotnetTool "BlazorSample" "BlazorSample.sln"
    Build-Project-DotnetTool "CSharpLatest" "CSharpLatest.sln"

    Invoke-JsonParser

    # ToDo: Migrate all of the remaining logic to JsonParser
    Write-Host "Checking for differences..."
    Show-DiffResults
    Write-Host -ForegroundColor Green "SUCCESS: ITs were successful! No differences were found!"
    exit 0
}
catch {
    Write-Host -ForegroundColor Red $_
    # ToDo: Migrate this to JsonParser, remove update-expected.ps1 file
    if($_.FullyQualifiedErrorId -eq $DifferencesMsg) {
        ./update-expected.ps1 -Project $Project
    } else {
        Write-Host "----"
        Write-Host $_.Exception
        Write-Host "----"
        Write-Host $_.ScriptStackTrace
    }
    exit 1
}
finally {
    Pop-Location
    Remove-Item -ErrorAction Ignore -Force global.json
}
