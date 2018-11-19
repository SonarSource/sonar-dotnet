nuget restore %~dp0src\Akka.sln -Verbosity quiet && msbuild /verbosity:detailed /m /t:rebuild /p:Configuration=Debug %~dp0src\Akka.sln
