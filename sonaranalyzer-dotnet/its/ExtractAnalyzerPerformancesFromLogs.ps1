$ErrorActionPreference = "Stop"

function ExtractAnalyzerPerformancesFromLogs
{
  param ([string[]]$buildLogsPaths)

  return $buildLogsPaths |
    Foreach-Object { Get-Content $_ } |
    Where-Object { $_ -match '^\s*<?([0-9.]+)\s*<?[0-9]+\s*(SonarAnalyzer\..*)$' -and ($matches[1] -ne '0.001') } |
    Foreach-Object {
      New-Object PSObject -Property @{
        Rule = $matches[2];
        Time = [decimal]$matches[1]
      }
    } |
    Group-Object Rule |
    Foreach-Object {
      New-Object PSObject -property @{
        Rule = $_.Name;
        Time = [math]::Round(($_.Group.Time | Measure-Object -sum).Sum, 3)
      }
    } |
    Sort-Object Time -Descending
}

# Process all build logs in the "output" folder
$timings = ExtractAnalyzerPerformancesFromLogs(Get-ChildItem output -filter *.txt | Foreach-Object { $_.FullName })

$timings | Where-Object { $_.Rule -match 'SonarAnalyzer\.CSharp.*' }                | Format-Table -AutoSize
$timings | Where-Object { $_.Rule -match 'SonarAnalyzer\.VisualBasic.*' }           | Format-Table -AutoSize
$timings | Where-Object { $_.Rule -match 'SonarAnalyzer\.Rules\.CSharp.*' }         | Format-Table -AutoSize
$timings | Where-Object { $_.Rule -match 'SonarAnalyzer\.Rules\.VisualBasic.*' }    | Format-Table -AutoSize
