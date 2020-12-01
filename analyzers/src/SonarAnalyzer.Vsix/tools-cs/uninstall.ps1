param($installPath, $toolsPath, $package, $project)

if ($project.Type -ne "C#") {
    return
}

if ('14.0', '15.0', '16.0' -notcontains $project.DTE.Version) {
    return
}

if ($project.Object.AnalyzerReferences -eq $null) {
    return
}

$analyzersPath = Split-Path -Path $toolsPath -Parent
$analyzersPath = Join-Path $analyzersPath "analyzers"

$analyzerFilePath = Join-Path $analyzersPath "Google.Protobuf.dll"
try {
    $project.Object.AnalyzerReferences.Remove($analyzerFilePath)
}
catch {
}

$analyzerFilePath = Join-Path $analyzersPath "SonarAnalyzer.CFG.dll"
try {
    $project.Object.AnalyzerReferences.Remove($analyzerFilePath)
}
catch {
}

$analyzerFilePath = Join-Path $analyzersPath "SonarAnalyzer.dll"
try {
    $project.Object.AnalyzerReferences.Remove($analyzerFilePath)
}
catch {
}

$analyzerFilePath = Join-Path $analyzersPath "SonarAnalyzer.CSharp.dll"
try {
    $project.Object.AnalyzerReferences.Remove($analyzerFilePath)
}
catch {
}


