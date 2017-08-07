. (Join-Path $PSScriptRoot "..\utils.ps1")

function Get-NuGetPath {
    return Get-ExecutablePath -name "nuget.exe" -envVar "NUGET_PATH"
}

function Get-MsBuildPath {
    return Get-ExecutablePath -name "msbuild.exe" -envVar "MSBUILD_PATH"
}

function Get-VsTestPath {
    return Get-ExecutablePath -name "VSTest.Console.exe" -envVar "VSTEST_PATH"
}

function Get-CodeCoveragePath {
    $vstest_exe = Get-VsTestPath
    $codeCoverageDirectory = Join-Path (Get-ChildItem $vstest_exe).Directory "..\..\..\..\.."
    return Get-ExecutablePath -name "CodeCoverage.exe" -directory $codeCoverageDirectory -envVar "CODE_COVERAGE_PATH"
}

# NuGet
function New-NuGetPackages([string]$binPath) {
    Write-Header "Building NuGet packages..."

    $nuget_exe = Get-NuGetPath

    Get-ChildItem "src" -Recurse *.nuspec | ForEach-Object {
        $output = (Join-Path $_.DirectoryName $binPath)

        & $nuget_exe pack $_.FullName -NoPackageAnalysis -OutputDirectory $output -Prop binPath=$binPath
        Test-ExitCode "ERROR: NuGet package creation FAILED."
    }
}

function Restore-Packages ([string]$solutionName) {
    Write-Header "Restoring NuGet packages for ${solutionName}..."

    $nuget_exe = Get-NuGetPath
    & $nuget_exe restore $solutionName
    Test-ExitCode "ERROR: Restoring NuGet packages FAILED."
}

# Build
function Invoke-MSBuild (
    [string][Parameter(Mandatory = $true, Position = 0)]$solutionName,
    [array][parameter(ValueFromRemainingArguments = $true)] $remainingArgs) {

    Write-Header "Building solution ${solutionName}..."

    $msbuild_exe = Get-MsBuildPath
    & $msbuild_exe $solutionName $remainingArgs
    Test-ExitCode "ERROR: Build FAILED."
}

# Tests
function Invoke-UnitTests([string]$binPath) {
    Write-Header "Running unit tests..."

    $escapedPath = $binPath -Replace '\\', '\\'

    $testFiles = @()
    Get-ChildItem ".\src" -Recurse -Include "*.UnitTest.dll" `
        | Where-Object { $_.DirectoryName -Match $escapedPath } `
        | ForEach-Object { $testFiles += $_ }

    $vstest_exe = Get-VsTestPath
    & $vstest_exe $testFiles /Enablecodecoverage /inIsolation /Logger:trx
    Test-ExitCode "ERROR: Unit Tests execution FAILED."
}

function Invoke-IntegrationTests() {
    Write-Header "Running integration tests..."

    Invoke-InLocation "its" {
        Exec { git submodule update --init --recursive --depth 1 }

        & .\regression-test.ps1
        Test-ExitCode "ERROR: Integration tests FAILED."
    }
}

# Coverage
function Invoke-CodeCoverage() {
    Write-Header "Creating coverage report..."

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
    Write-Header "Generating rules metadata..."

    #Generate the XML descriptor files for the SQ plugin
    Invoke-InLocation "src\SonarAnalyzer.RuleDescriptorGenerator\${binPath}" {
        Exec { .\SonarAnalyzer.RuleDescriptorGenerator.exe cs }
        Write-Host "Sucessfully created metadata for C#"
        Exec { .\SonarAnalyzer.RuleDescriptorGenerator.exe vbnet }
        Write-Host "Sucessfully created metadata for VB.Net"
    }
}