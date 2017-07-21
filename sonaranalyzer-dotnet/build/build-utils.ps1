Add-Type -AssemblyName "System.IO.Compression.FileSystem"

$sonarqube_runner_version = "3.0.0.629"

$repo_path = (Resolve-Path (Join-Path $PSScriptRoot ".."))

# Resolves the given relative to the repository path to absolute.
function Resolve-RepoPath([string]$relativePath) {
    return [IO.Path]::GetFullPath((Join-Path $repo_path $relativePath))
}

# Original: http://jameskovacs.com/2010/02/25/the-exec-problem
function Exec ([scriptblock]$command, [string]$errorMessage = "Error executing command: " + $command) {
    $output = & $command
    if ((-not $?) -or ($lastexitcode -ne 0)) {
        Write-Host $output
        throw $errorMessage
    }
    return $output
}

function Test-ExitCode([string]$errorMessage = "Error executing command.") {
    if ((-not $?) -or ($lastexitcode -ne 0)) {
        throw $errorMessage
    }
}

# Sets the current folder and executes the given script.
# When the script finishes sets the original current folder.
function Exec-InLocation([string]$path, [scriptblock]$command) {
    try {
        Push-Location $path
        & $command
    }
    finally {
        Pop-Location
    }
}

function To-UnixLineEndings([string]$fileName) {
    Get-ChildItem $fileName | ForEach-Object {
        $content = [IO.File]::ReadAllText($_) -Replace "`r`n?", "`n"
        $utf8 = New-Object System.Text.UTF8Encoding $false
        [IO.File]::WriteAllText($_, $content, $utf8)
    }
}

function Write-Header([string]$text) {
    Write-Host "================================================"
    Write-Host $text
    Write-Host "================================================"
}

## Build ############################################################

function Get-BuildNumber([string]$default = "0") {
    if ($env:BUILD_NUMBER) {
        return $env:BUILD_NUMBER
    }
    return $default
}

function Get-BranchName {
    if ($env:GITHUB_BRANCH) {
        if ($env:GITHUB_BRANCH.StartsWith("refs/heads/")) {
            return $env:GITHUB_BRANCH.Substring(11)
        }
        return $env:GITHUB_BRANCH
    }
    return Exec { & git rev-parse --abbrev-ref HEAD }
}

function Get-Sha1 {
    if ($env:GIT_SHA1) {
        return $env:GIT_SHA1
    }
    return Exec { & git rev-parse HEAD }
}

function Set-MsBuild15Location {
    # Sets the path to MSBuild 15 into an the MSBUILD_PATH environment variable
    # All subsequent builds after this command will use MSBuild 15!
    
    # Test if vswhere.exe is in your path. Download from: https://github.com/Microsoft/vswhere/releases
    Get-VsWherePath

    $path = vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
    if ($path) {
        $path = join-path $path 'MSBuild\15.0\Bin\MSBuild.exe'
        if (test-path $path) {
            [environment]::SetEnvironmentVariable("MSBUILD_PATH", $path)
        }
    }
}

function Get-ExecutablePath([string]$name, [string]$directory, [string]$envVar) {
    $path = [environment]::GetEnvironmentVariable($envVar, "Process")

    if (!$path) {
        if (!$directory) {
            $path = Exec { & where.exe $name } `
                | Select-Object -First 1
        }
        else {
            $path = Exec { & where.exe /R $directory $name } `
                | Select-Object -First 1
        }
    }

    if (Test-Path $path) {
        Write-Debug "Found ${name} at ${path}"
        [environment]::SetEnvironmentVariable($envVar, $path)
        return $path
    }

    Write-Error "Cannot find $name"
    exit 1
}

function Get-VsWherePath {
    return Get-ExecutablePath -name "vswhere.exe" -envVar "VSWHERE_PATH"
}

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

function Expand-ZIPFile($source, $destination) {
    Write-Host "Expanding ZIP file ${source}"

    if (Get-Command "Expand-Archive" -errorAction SilentlyContinue) {
        # PS v5.0+
        Expand-Archive $source $destination -Force
    }
    else {
        if (-Not (Test-Path $destination)) {
            Write-Host "Creating ${destination}"
            New-Item $destination -ItemType Directory
        }

        $application = New-Object -Com Shell.Application

        $zip = $application.NameSpace($source)
        $destinationFolder = $application.NameSpace($destination)
        $destinationFolder.CopyHere($zip.items(), 0x14)
    }
}

function Get-SonarQubeRunnerPath {
    $sonarqube_runner_exe = (Resolve-RepoPath "MSBuild.SonarQube.Runner.exe")

    if (Test-Path $sonarqube_runner_exe) {
        return $sonarqube_runner_exe
    }

    $downloadLink = "https://github.com/SonarSource/sonar-msbuild-runner/releases/download/${sonarqube_runner_version}/sonar-scanner-msbuild-${sonarqube_runner_version}.zip"
    $sonarqube_runner_zip = (Resolve-RepoPath "MSBuild.SonarQube.Runner.zip")

    (New-Object System.Net.WebClient).DownloadFile($downloadLink, $sonarqube_runner_zip)

    # perhaps we could use other folder, not the repository root
    Expand-ZIPFile $sonarqube_runner_zip (Resolve-RepoPath "")

    Remove-Item $sonarqube_runner_zip -Force

    Write-Host "Found MSBuild.SonarQube.Runner.exe at ${sonarqube_runner_exe}"

    return $sonarqube_runner_exe
}

function Set-Version {
    Write-Header "Updating version in all files..."

    $buildNumber = Get-BuildNumber
    $branchName = Get-BranchName
    $sha1 = Get-Sha1

    Write-Host "Setting build number ${buildNumber}, sha1 ${sha1} and branch ${branchName}"

    $versionPropsPath = (Resolve-RepoPath "build\Version.props")

    (Get-Content $versionPropsPath) `
        -Replace '<Sha1>.*</Sha1>', "<Sha1>$sha1</Sha1>" `
        -Replace '<BuildNumber>\d+</BuildNumber>', "<BuildNumber>$buildNumber</BuildNumber>" `
        -Replace '<BranchName>.*</BranchName>', "<BranchName>$branchName</BranchName>" `
        | Set-Content $versionPropsPath

    $msbuild_exe = Get-MsBuildPath
    $changeVersionProj = (Resolve-RepoPath build\ChangeVersion.proj)
    Exec { & $msbuild_exe $changeVersionProj }

    $version = Get-Version
    Write-Host "Version successfully set to '${version}'"
}

function Get-Version {
    [xml]$versionProps = Get-Content (Resolve-RepoPath ".\build\Version.props")
    return $versionProps.Project.PropertyGroup.MainVersion + "." + $versionProps.Project.PropertyGroup.BuildNumber
}

function Pack-Nugets([ValidateSet("Release", "Debug")][string]$configuration) {
    Write-Header "Packing nugets..."

    $nuget_exe = Get-NuGetPath

    Get-ChildItem (Resolve-RepoPath "src") -Recurse *.nuspec `
        | ForEach-Object {
        $output = (Join-Path $_.DirectoryName "bin\${configuration}")

        Write-Host "Creating NuGet package: " + $_.FullName

        & $nuget_exe pack $_.FullName -NoPackageAnalysis -OutputDirectory $output -Prop Configuration=$configuration
        Test-ExitCode "ERROR: NuGet package creation FAILED."
    }
}

function Setup-NugetConfig(
    [string]$articfactoryUrl,
    [string]$artifactoryUser,
    [string]$artifactoryPass) {

    Write-Header "Setting up nuget.config..."

    $nuget_exe = Get-NuGetPath

    Remove-Item $env:APPDATA\NuGet\NuGet.Config

    Exec { & $nuget_exe Sources Add -Name "repox" -Source "${artifactoryUrl}/sonarsource-nuget-qa" }

    Exec { & $nuget_exe SetApiKey "${artifactoryUser}:${artifactoryPass}" -Source repox }
}

function Publish-Packages {
    Write-Header "Publishing NuGet packages..."

    $nuget_exe = Get-NuGetPath

    $packages = Get-ChildItem (Resolve-RepoPath "src") -Recurse "*.nupkg"

    foreach ($file in $packages) {
        Exec { & $nuget_exe push $file.FullName -Source "repox" }
    }
}

function Restore-Packages ([string]$solutionPath) {
    Write-Header "Restoring NuGet packages..."
    $nuget_exe = Get-NuGetPath
    & $nuget_exe restore $solutionPath
    Test-ExitCode "ERROR: Restoring NuGet packages FAILED."
}

function Begin-Analysis(
    [string]$sonarQubeUrl,
    [string]$sonarQubeToken,
    [string]$sonarQubeProjectKey,
    [string]$sonarQubeProjectName,
    [array][parameter(ValueFromRemainingArguments = $true)] $remainingArgs) {

    Write-Header "Running SonarQube Analysis begin step..."

    $sonarqube_runner_exe = Get-SonarQubeRunnerPath

    & $sonarqube_runner_exe begin `
        /k:$sonarQubeProjectKey `
        /n:$sonarQubeProjectName `
        /d:sonar.host.url=$sonarQubeUrl `
        /d:sonar.login=$sonarQubeToken `
        $remainingArgs
    Test-ExitCode "ERROR: SonarQube Analysis begin step FAILED."
}

function End-Analysis([string]$sonarQubeToken) {
    Write-Header "Running SonarQube Analysis end step..."

    $sonarqube_runner_exe = Get-SonarQubeRunnerPath

    & $sonarqube_runner_exe end /d:sonar.login=$sonarQubeToken
    Test-ExitCode "ERROR: SonarQube Analysis end step FAILED."
}

function Build-Solution (
    [string][Parameter(Mandatory = $true, Position = 0)]$solutionPath,
    [array][parameter(ValueFromRemainingArguments = $true)] $remainingArgs) {

    Write-Header "Building solution ${solutionPath}..."

    Restore-Packages $solutionPath

    $msbuild_exe = Get-MsBuildPath

    & $msbuild_exe $solutionPath $remainingArgs
    Test-ExitCode "ERROR: Build FAILED."
}

function Build-ReleaseSolution(
    [string][Parameter(Mandatory = $true, Position = 0)]$solutionPath,
    [string][Parameter(Mandatory = $true, Position = 1)]$certificatePath,
    [array][parameter(ValueFromRemainingArguments = $true)] $remainingArgs) {

    Build-Solution $solutionPath `
        /v:m `
        /p:configuration=Release `
        /p:DeployExtension=false `
        /p:ZipPackageCompressionLevel=normal `
        /p:defineConstants=SignAssembly `
        /p:SignAssembly=true `
        /p:AssemblyOriginatorKeyFile=$certificatePath `
        $remainingArgs
}

function Run-Tests {
    Write-Header "Starting test execution..."

    $testFiles = @()
    Get-ChildItem (Resolve-RepoPath "src") -Recurse -Include "*.UnitTest.dll" `
        | Where-Object { $_.DirectoryName -Match "bin" } `
        | ForEach-Object { $testFiles += $_ }

    Write-Host "Running unit tests for: ${testFiles}"

    $vstest_exe = Get-VsTestPath
    & $vstest_exe $testFiles /Enablecodecoverage /inIsolation /Logger:trx
    Test-ExitCode "ERROR: Unit Tests execution FAILED."

    $codeCoverage_exe = Get-CodeCoveragePath
    Write-Host "Generating code coverage reports:"
    Get-ChildItem (Resolve-RepoPath "TestResults") -Recurse -Include "*.coverage" | ForEach-Object {
        $filePathWithNewExtension = $_.FullName + "xml"
        Write-Host "  ${filePathWithNewExtension}"
        & $codeCoverage_exe analyze /output:$filePathWithNewExtension $_.FullName
        Test-ExitCode "ERROR: Code coverage reports generation FAILED."
    }
}