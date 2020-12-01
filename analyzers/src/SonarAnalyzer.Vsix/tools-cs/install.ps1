param($installPath, $toolsPath, $package, $project)

$invalidVsVersion = $false

if ('14.0', '15.0', '16.0' -notcontains $project.DTE.Version) {
    $invalidVsVersion = $true
}

if ($project.DTE.Version -eq '14.0') {
    $currentAppDomainBaseDir = [System.AppDomain]::CurrentDomain.BaseDirectory
    $path = Join-Path $currentAppDomainBaseDir "msenv.dll"

    if (Test-Path $path) {
        $versionInfo = (Get-Item $path).VersionInfo
        $fullVersion =  New-Object System.Version -ArgumentList @(
            $versionInfo.FileMajorPart
            $versionInfo.FileMinorPart
            $versionInfo.FileBuildPart
            $versionInfo.FilePrivatePart
        )
        $minVersion = [version]"14.0.25420.00"
        if ($fullVersion -lt $minVersion) {
            $invalidVsVersion = $true
        }
    } else {
        $invalidVsVersion = $true
    }
}

if ($invalidVsVersion) {
    throw 'This package can only be installed on Visual Studio 2015 Update 3 or later.'
}

if ($project.Object.AnalyzerReferences -eq $null) {
    throw 'This package cannot be installed without an analyzer reference.'
}

if ($project.Type -ne "C#") {
    throw 'This package can only be installed on C# projects.'
}

$analyzersPath = Split-Path -Path $toolsPath -Parent
$analyzersPath = Join-Path $analyzersPath "analyzers"

$analyzerFilePath = Join-Path $analyzersPath "Google.Protobuf.dll"
$project.Object.AnalyzerReferences.Add($analyzerFilePath)

$analyzerFilePath = Join-Path $analyzersPath "SonarAnalyzer.CFG.dll"
$project.Object.AnalyzerReferences.Add($analyzerFilePath)

$analyzerFilePath = Join-Path $analyzersPath "SonarAnalyzer.dll"
$project.Object.AnalyzerReferences.Add($analyzerFilePath)

$analyzerFilePath = Join-Path $analyzersPath "SonarAnalyzer.CSharp.dll"
$project.Object.AnalyzerReferences.Add($analyzerFilePath)

