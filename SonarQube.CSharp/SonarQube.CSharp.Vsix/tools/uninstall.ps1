param($installPath, $toolsPath, $package, $project)

# Uninstall the language agnostic analyzers.
$analyzersPath = join-path $toolsPath "analyzers"

foreach ($analyzerFilePath in Get-ChildItem $analyzersPath)
{
    $project.Object.AnalyzerReferences.Remove($analyzerFilePath.FullName)
}

# Uninstall language specific analyzers.
# $project.Type gives the language name like (C# or VB.NET)
$languageAnalyzersPath = join-path $analyzersPath $project.Type

foreach ($analyzerFilePath in Get-ChildItem $languageAnalyzersPath)
{
    $project.Object.AnalyzerReferences.Remove($analyzerFilePath.FullName)
}