$dotNet = (get-command dotnet.exe).Path
& $dotNet build -c Release

$dotMemoryUnit = dir $env:USERPROFILE\.nuget\packages\jetbrains.dotmemoryunit\*\lib\tools\dotMemoryUnit.exe | sort -Property {$_.VersionInfo.FileVersion} | select -First 1

try {
    Push-Location $PSScriptRoot
    & $dotMemoryUnit "$dotNet" --propagate-exit-code --no-updates -- test "bin/Release/net9.0/SonarAnalyzer.Memory.Test.dll" --logger "trx;logfilename=net9.trx" @args
    & $dotMemoryUnit "$dotNet" --propagate-exit-code --no-updates -- test "bin/Release/net48/SonarAnalyzer.Memory.Test.dll" --logger "trx;logfilename=net48.trx" @args
} finally {
    Pop-Location
}