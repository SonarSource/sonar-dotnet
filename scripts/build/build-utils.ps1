. (Join-Path $PSScriptRoot "..\utils.ps1")

function Get-NuGetPath {
    return Get-ExecutablePath -name "nuget.exe" -envVar "NUGET_PATH"
}

function Get-VsWherePath {
    return Get-ExecutablePath -name "vswhere.exe" -envVar "VSWHERE_PATH"
}

function Get-MsBuildPath([ValidateSet("14.0", "15.0")][string]$msbuildVersion) {
    if ($msbuildVersion -eq "14.0") {
        return Get-ExecutablePath -name "msbuild.exe" -envVar "MSBUILD_14_PATH"
    }

    $msbuild15Env = "MSBUILD_15_PATH"
    $msbuild15Path = [environment]::GetEnvironmentVariable($msbuild15Env, "Process")

    if (!$msbuild15Path) {
        # Sets the path to MSBuild 15 into an the MSBUILD_PATH environment variable
        # All subsequent builds after this command will use MSBuild 15!
        # Test if vswhere.exe is in your path. Download from: https://github.com/Microsoft/vswhere/releases
        $path = & (Get-VsWherePath) -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
        if ($path) {
            $msbuild15Path = Join-Path $path "MSBuild\15.0\Bin\MSBuild.exe"
            [environment]::SetEnvironmentVariable($msbuild15Env, $msbuild15Path)
        }
    }

    if (Test-Path $msbuild15Path) {
        Write-Host "Found 'MSBuild 15' at '${msbuild15Path}'"
        return $msbuild15Path
    }

    Write-Error "Cannot find 'MSBuild 15'"
    exit 1
}

function Get-VsTestPath {
    return Get-ExecutablePath -name "VSTest.Console.exe" -envVar "VSTEST_PATH"
}

function Get-CodeCoveragePath {
    $vstest_exe = Get-VsTestPath
    $codeCoverageDirectory = Join-Path (Get-ChildItem $vstest_exe).Directory "..\..\..\..\.."
    return Get-ExecutablePath -name "CodeCoverage.exe" -directory $codeCoverageDirectory -envVar "CODE_COVERAGE_PATH"
}

function Get-MSBuildImportBeforePath([ValidateSet("14.0", "15.0")][string]$msbuildVersion) {
    return "$env:USERPROFILE\AppData\Local\Microsoft\MSBuild\${msbuildVersion}\Microsoft.Common.targets\ImportBefore"
}

# NuGet
function New-NuGetPackages([string]$binPath) {
    Write-Header "Building NuGet packages"

    $nuget_exe = Get-NuGetPath

    Get-ChildItem "src" -Recurse *.nuspec | ForEach-Object {
        & $nuget_exe pack $_.FullName -NoPackageAnalysis -OutputDirectory (Join-Path $_.DirectoryName $binPath) `
            -Prop binPath=$binPath
        Test-ExitCode "ERROR: NuGet package creation FAILED."
    }
}

function Restore-Packages ([string]$solutionPath) {
    $solutionName = Split-Path $solutionPath -leaf
    Write-Header "Restoring NuGet packages for ${solutionName}"

    & (Get-NuGetPath) restore $solutionPath
    Test-ExitCode "ERROR: Restoring NuGet packages FAILED."
}

# Build
function Invoke-MSBuild (
    [Parameter(Mandatory = $true, Position = 0)][ValidateSet("14.0", "15.0")][string]$msbuildVersion,
    [Parameter(Mandatory = $true, Position = 1)][string]$solutionPath,
    [parameter(ValueFromRemainingArguments = $true)][array]$remainingArgs) {

    $solutionName = Split-Path $solutionPath -leaf
    Write-Header "Building solution ${solutionName}"

    $msbuild_exe = Get-MsBuildPath $msbuildVersion
    & $msbuild_exe $solutionPath $remainingArgs
    Test-ExitCode "ERROR: Build FAILED."
}

# Tests
function Invoke-UnitTests([string]$binPath) {
    Write-Header "Running unit tests"

    $escapedPath = $binPath -Replace '\\', '\\'

    $testFiles = @()
    Get-ChildItem ".\src" -Recurse -Include "*.UnitTest.dll" `
        | Where-Object { $_.DirectoryName -Match $escapedPath } `
        | ForEach-Object { $testFiles += $_ }

    & (Get-VsTestPath) $testFiles /Enablecodecoverage /inIsolation /Logger:trx
    Test-ExitCode "ERROR: Unit Tests execution FAILED."
}

function Invoke-IntegrationTests([ValidateSet("14.0", "15.0")][string] $msbuildVersion) {
    Write-Header "Running integration tests"

    Invoke-InLocation "its" {
        Exec { git submodule update --init --recursive --depth 1 }

        & .\regression-test.ps1 -msbuildVersion $msbuildVersion
        Test-ExitCode "ERROR: Integration tests FAILED."
    }
}

# Coverage
function Invoke-CodeCoverage() {
    Write-Header "Creating coverage report"

    $codeCoverage_exe = Get-CodeCoveragePath
    Write-Host "Generating code coverage reports:"
    Get-ChildItem "TestResults" -Recurse -Include "*.coverage" | ForEach-Object {
        $filePathWithNewExtension = $_.FullName + "xml"
        Write-Host "  ${filePathWithNewExtension}"
        & $codeCoverage_exe analyze /output:$filePathWithNewExtension $_.FullName
        Test-ExitCode "ERROR: Code coverage reports generation FAILED."
    }
}

# Metadata
function New-Metadata([string]$binPath) {
    Write-Header "Generating rules metadata"

    #Generate the XML descriptor files for the SQ plugin
    Invoke-InLocation "src\SonarAnalyzer.RuleDescriptorGenerator\${binPath}" {
        Exec { .\SonarAnalyzer.RuleDescriptorGenerator.exe cs }
        Write-Host "Sucessfully created metadata for C#"
        Exec { .\SonarAnalyzer.RuleDescriptorGenerator.exe vbnet }
        Write-Host "Sucessfully created metadata for VB.Net"
    }
}