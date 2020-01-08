. (Join-Path $PSScriptRoot "..\utils.ps1")

function Get-NuGetPath {
    return Get-ExecutablePath -name "nuget.exe" -envVar "NUGET_PATH"
}

function Get-VsWherePath {
    # VsWhere is supposed to be at a fixed path, see https://github.com/Microsoft/vswhere/wiki/Installing
    if (-not (Test-Path env:VSWHERE_PATH)) {
         $env:VSWHERE_PATH = ${env:ProgramFiles(x86)} + '\Microsoft Visual Studio\Installer\vswhere.exe'
    }
    return Get-ExecutablePath -name "vswhere.exe" -envVar "VSWHERE_PATH"
}

function Get-MsBuildPath([ValidateSet("14.0", "15.0", "16.0", "Current")][string]$msbuildVersion) {
    if ($msbuildVersion -eq "14.0") {
        return Get-ExecutablePath -name "msbuild.exe" -envVar "MSBUILD_PATH"
    }

    #If MSBUILD_PATH environment variable is found, the version is not checked and the value of input parameter is ignored.
    Write-Host "Trying to find 'msbuild.exe' using 'MSBUILD_PATH' environment variable"
    $msbuildPathEnvVar = "MSBUILD_PATH"
    $msbuildPath = [environment]::GetEnvironmentVariable($msbuildPathEnvVar, "Process")

    if (!$msbuildPath) {
        Write-Host "Environment variable ${msbuildEnv} not found"
        Write-Host "Trying to find path using 'vswhere.exe'"

        # Sets the path to MSBuild into an the MSBUILD_PATH environment variable
        # All subsequent builds after this command will use that MSBuild!
        # Test if vswhere.exe is in your path. Download from: https://github.com/Microsoft/vswhere/releases
        $msbuildPath = Exec { & (Get-VsWherePath) -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe } | Select-Object -First 1
        if ($msbuildPath) {
            [environment]::SetEnvironmentVariable($msbuildPathEnvVar, $msbuildPath)
        }
    }

    if (Test-Path $msbuildPath) {
        Write-Debug "Found 'msbuild.exe' at '${msbuildPath}'"
        return $msbuildPath
    }

    throw "'msbuild.exe' located at '${msbuildPath}' doesn't exist"
}

function Get-VsTestPath {
    return Get-ExecutablePath -name "VSTest.Console.exe" -envVar "VSTEST_PATH"
}

function Get-CodeCoveragePath {
    $vstest_exe = Get-VsTestPath
    $codeCoverageDirectory = Join-Path (Get-ChildItem $vstest_exe).Directory "..\..\..\..\.."
    return Get-ExecutablePath -name "CodeCoverage.exe" -directory $codeCoverageDirectory -envVar "CODE_COVERAGE_PATH"
}

function Get-MSBuildImportBeforePath([ValidateSet("14.0", "15.0", "16.0", "Current")][string]$msbuildVersion) {
    return "$env:USERPROFILE\AppData\Local\Microsoft\MSBuild\${msbuildVersion}\Microsoft.Common.targets\ImportBefore"
}

# NuGet
function New-NuGetPackages([string]$binPath) {
    Write-Header "Building NuGet packages"

    $nugetExe = Get-NuGetPath
    Get-ChildItem "src" -Recurse *.nuspec | ForEach-Object {
        $outputDir = Join-Path $_.DirectoryName $binPath

        Write-Debug "Creating NuGet package for '$_' in ${outputDir}"

        $fixedBinPath = $binPath
        if ($_.Name -Like "Descriptor.*.nuspec" -Or $_.Name -Like "SonarAnalyzer.CFG.*.nuspec") {
            $fixedBinPath = "${binPath}\net46"
        }

        if (Test-Debug) {
            Exec { & $nugetExe pack $_.FullName -NoPackageAnalysis -OutputDirectory $outputDir `
                -Prop binPath=$fixedBinPath -Verbosity detailed `
            } -errorMessage "ERROR: NuGet package creation FAILED."
        }
        else {
            Exec { & $nugetExe pack $_.FullName -NoPackageAnalysis -OutputDirectory $outputDir `
                -Prop binPath=$fixedBinPath `
            } -errorMessage "ERROR: NuGet package creation FAILED."
        }
    }
}

function Restore-Packages (
    [Parameter(Mandatory = $true, Position = 0)][ValidateSet("14.0", "15.0", "16.0", "Current")][string]$msbuildVersion,
    [Parameter(Mandatory = $true, Position = 1)][string]$solutionPath) {

    $solutionName = Split-Path $solutionPath -Leaf
    Write-Header "Restoring NuGet packages for ${solutionName}"

    $msbuildBinDir = Split-Path -Parent (Get-MsBuildPath $msbuildVersion)

    if (Test-Debug) {
        Exec { & (Get-NuGetPath) restore $solutionPath -MSBuildPath $msbuildBinDir -Verbosity detailed `
        } -errorMessage "ERROR: Restoring NuGet packages FAILED."
    }
    else {
        Exec { & (Get-NuGetPath) restore $solutionPath -MSBuildPath $msbuildBinDir `
        } -errorMessage "ERROR: Restoring NuGet packages FAILED."
    }
}

# Build
function Invoke-MSBuild (
    [Parameter(Mandatory = $true, Position = 0)][ValidateSet("14.0", "15.0", "16.0", "Current")][string]$msbuildVersion,
    [Parameter(Mandatory = $true, Position = 1)][string]$solutionPath,
    [parameter(ValueFromRemainingArguments = $true)][array]$remainingArgs) {

    $solutionName = Split-Path $solutionPath -leaf
    Write-Header "Building solution ${solutionName}"

    if (Test-Debug) {
        $remainingArgs += "/v:detailed"
    }
    else {
        $remainingArgs += "/v:quiet"
    }

    $msbuildExe = Get-MsBuildPath $msbuildVersion
    Exec { & $msbuildExe $solutionPath $remainingArgs `
    } -errorMessage "ERROR: Build FAILED."
}

# Tests
function Invoke-UnitTests([string]$binPath, [bool]$failsIfNotTest) {
    Write-Header "Running unit tests"

    $escapedPath = (Join-Path $binPath "net46") -Replace '\\', '\\'

    Write-Debug "Running unit tests for"
    $testFiles = @()
    $testDirs = @()
    Get-ChildItem ".\tests" -Recurse -Include "*.UnitTest.dll" `
        | Where-Object { $_.DirectoryName -Match $escapedPath } `
        | ForEach-Object {
            $currentFile = $_
            Write-Debug "   - ${currentFile}"
            $testFiles += $currentFile
            $testDirs += $currentFile.Directory
        }
    $testDirs = $testDirs | Select-Object -Uniq

    & (Get-VsTestPath) $testFiles /Parallel /Enablecodecoverage /InIsolation /Logger:trx /TestAdapterPath:$testDirs
    Test-ExitCode "ERROR: Unit Tests execution FAILED."
}

function Invoke-IntegrationTests([ValidateSet("14.0", "15.0", "16.0", "Current")][string] $msbuildVersion) {
    Write-Header "Running integration tests"

    Invoke-InLocation "its" {
        Exec { & git submodule update --init --recursive --depth 1 }

        Exec { & .\regression-test.ps1 -msbuildVersion $msbuildVersion `
        } -errorMessage "ERROR: Integration tests FAILED."
    }
}

# Coverage
function Invoke-CodeCoverage() {
    Write-Header "Creating coverage report"

    $codeCoverageExe = Get-CodeCoveragePath

    Write-Host "Generating code coverage reports for"
    Get-ChildItem "TestResults" -Recurse -Include "*.coverage" | ForEach-Object {
        Write-Host "    -" $_.FullName

        $filePathWithNewExtension = $_.FullName + "xml"
        if (Test-Path $filePathWithNewExtension) {
            Write-Debug "Coveragexml report already exists, removing it"
            Remove-Item -Force $filePathWithNewExtension
        }

        Exec { & $codeCoverageExe analyze /output:$filePathWithNewExtension $_.FullName `
        } -errorMessage "ERROR: Code coverage reports generation FAILED."
    }
}
