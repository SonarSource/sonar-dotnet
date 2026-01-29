param($installPath, $toolsPath, $package, $project)

if ($project.Type -ne "C#") {
    return
}

if ([Version]$project.DTE.Version -lt [Version]"14.0") {
    return
}

if ($project.Object.AnalyzerReferences -eq $null) {
    return
}

$analyzersPath = Split-Path -Path $toolsPath -Parent
$analyzersPath = Join-Path $analyzersPath "analyzers"

$analyzerFilePath = Join-Path $analyzersPath "SonarAnalyzer.CSharp.dll"
try {
    $project.Object.AnalyzerReferences.Remove($analyzerFilePath)
}
catch {
}


