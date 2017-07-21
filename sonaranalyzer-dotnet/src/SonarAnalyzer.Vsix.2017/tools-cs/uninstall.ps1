param($installPath, $toolsPath, $package, $project)

if($project.Type -ne "C#")
{
    return
}

if ('14.0', '15.0' -notcontains $project.DTE.Version)
{
    return
}

if ($project.Object.AnalyzerReferences -eq $null)
{
    return
}

$analyzersPath = split-path -path $toolsPath -parent
$analyzersPath = join-path $analyzersPath "analyzers"

$analyzerFilePath = join-path $analyzersPath "Google.Protobuf.dll"
try
{
    $project.Object.AnalyzerReferences.Remove($analyzerFilePath)
}
catch
{
}

$analyzerFilePath = join-path $analyzersPath "SonarAnalyzer.dll"
try
{
    $project.Object.AnalyzerReferences.Remove($analyzerFilePath)
}
catch
{
}

$analyzerFilePath = join-path $analyzersPath "SonarAnalyzer.CSharp.dll"
try
{
    $project.Object.AnalyzerReferences.Remove($analyzerFilePath)
}
catch
{
}