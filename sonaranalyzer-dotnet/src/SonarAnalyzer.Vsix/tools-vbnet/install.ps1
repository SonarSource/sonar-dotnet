param($installPath, $toolsPath, $package, $project)

if ('14.0', '15.0' -notcontains $project.DTE.Version) {
    throw 'This package can only be installed on Visual Studio 2015  or Visual Studio 2017.'
}

if ($project.Object.AnalyzerReferences -eq $null) {
    throw 'This package cannot be installed without an analyzer reference.'
}

if ($project.Type -ne "VB.NET") {
    throw 'This package can only be installed on VB.NET projects.'
}

$analyzersPath = Split-Path -Path $toolsPath -Parent
$analyzersPath = Join-Path $analyzersPath "analyzers"

$analyzerFilePath = Join-Path $analyzersPath "Google.Protobuf.dll"
$project.Object.AnalyzerReferences.Add($analyzerFilePath)

$analyzerFilePath = Join-Path $analyzersPath "SonarAnalyzer.dll"
$project.Object.AnalyzerReferences.Add($analyzerFilePath)

$analyzerFilePath = Join-Path $analyzersPath "SonarAnalyzer.VisualBasic.dll"
$project.Object.AnalyzerReferences.Add($analyzerFilePath)