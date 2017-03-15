@PowerShell -NonInteractive -NoProfile -ExecutionPolicy Unrestricted -Command "& {.\build\release.ps1; exit $LastExitCode }" 
@echo From Cmd.exe: release.ps1 exited with exit code %errorlevel%
exit %errorlevel%