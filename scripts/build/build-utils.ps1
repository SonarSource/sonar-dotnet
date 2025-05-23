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

function Get-MsBuildPath {
    #If MSBUILD_PATH environment variable is found, the version is not checked and the value of input parameter is ignored.
    Write-Host "Trying to find 'msbuild.exe' using 'MSBUILD_PATH' environment variable"
    $msbuildPathEnvVar = "MSBUILD_PATH"
    $msbuildPath = [environment]::GetEnvironmentVariable($msbuildPathEnvVar, [System.EnvironmentVariableTarget]::Machine)

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

function Get-MSBuildImportBeforePath([ValidateSet("15.0", "16.0", "17.0", "Current")][string]$msbuildVersion) {
    return "$env:USERPROFILE\AppData\Local\Microsoft\MSBuild\${msbuildVersion}\Microsoft.Common.targets\ImportBefore"
}

function Get-MSBuildImportBeforePath-SystemX64([ValidateSet("15.0", "16.0", "17.0", "Current")][string]$msbuildVersion) {
    return "C:\Windows\SysWOW64\config\systemprofile\AppData\Local\Microsoft\MSBuild\${msbuildVersion}\Microsoft.Common.targets\ImportBefore"
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

# Used by the integration tests in analyzers\its\regression-test.ps1
function Restore-Packages ([Parameter(Mandatory = $true, Position = 1)][string]$solutionPath) {
    $solutionName = Split-Path $solutionPath -Leaf
    Write-Header "Restoring NuGet packages for ${solutionName}"

    $msbuildBinDir = Split-Path -Parent (Get-MsBuildPath)

    if (Test-Debug) {
        Exec { & (Get-NuGetPath) restore -LockedMode -MSBuildPath $msbuildBinDir -Verbosity detailed $solutionPath`
        } -errorMessage "ERROR: Restoring NuGet packages FAILED."
    }
    else {
        Exec { & (Get-NuGetPath) restore -LockedMode -MSBuildPath $msbuildBinDir $solutionPath`
        } -errorMessage "ERROR: Restoring NuGet packages FAILED."
    }
}

# Build
function Invoke-MSBuild (
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

    $msbuildExe = Get-MsBuildPath
    Exec { & $msbuildExe $solutionPath $remainingArgs `
    } -errorMessage "ERROR: Build FAILED."
}

# Tests
function Invoke-UnitTests([string]$binPath, [string]$buildConfiguration) {
    Write-Header "Running unit tests"

    $escapedPath = (Join-Path $binPath "net48") -Replace '\\', '\\'

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

    Write-Header "Running unit tests .NET Framework 4.8"
    & (Get-VsTestPath) $testFiles /Logger:"console;verbosity=minimal" /Parallel /Enablecodecoverage /InIsolation  /TestAdapterPath:$testDirs
    Test-ExitCode "ERROR: Unit Tests execution FAILED."

    $testProjFileName = "tests\SonarAnalyzer.Test\SonarAnalyzer.Test.csproj"

    Write-Header "Running unit tests .NET 6"
    dotnet test $testProjFileName -f net6.0 -v minimal -c $buildConfiguration --no-build --no-restore
    Test-ExitCode "ERROR: Unit tests for .NET 6 FAILED."
}

function Invoke-IntegrationTests() {
    Write-Header "Running integration tests"

    Invoke-InLocation "its" {
        Exec { & git submodule update --init --recursive --depth 1 }

        Exec { & .\regression-test.ps1 `
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
