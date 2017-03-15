@PowerShell -NonInteractive -NoProfile -ExecutionPolicy Unrestricted -Command "& {.\build.ps1; exit $LastExitCode }" 
@echo From Cmd.exe: sonar-csharp build.ps1 exited with exit code %errorlevel%
exit %errorlevel%