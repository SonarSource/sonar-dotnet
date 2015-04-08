param($installPath, $toolsPath, $package, $project)

# Uninstall the language agnostic analyzers.
$analyzersPath = join-path $toolsPath "analyzers"
$analyzerFilePath = join-path $analyzersPath "SonarQube.Analyzers.dll"
$project.Object.AnalyzerReferences.Remove($analyzerFilePath)
