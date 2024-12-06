$dotNet = (get-command dotnet.exe).Path
& $dotNet build

$dotMemoryUnit = dir $env:USERPROFILE\.nuget\packages\jetbrains.dotmemoryunit\*\lib\tools\dotMemoryUnit.exe | sort -Property {$_.VersionInfo.FileVersion} | select -First 1

& $dotMemoryUnit "$dotNet" --propagate-exit-code --no-updates -- test "bin/Debug/net9.0/SonarAnalyzer.Memory.Test.dll" --logger "trx;logfilename=net9.trx"
& $dotMemoryUnit "$dotNet" --propagate-exit-code --no-updates -- test "bin/Debug/net48/SonarAnalyzer.Memory.Test.dll" --logger "trx;logfilename=net48.trx"