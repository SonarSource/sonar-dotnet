@PowerShell -NonInteractive -NoProfile -ExecutionPolicy Unrestricted -Command "& {.\build\qa.ps1; exit $LastExitCode }" 
@echo From Cmd.exe: qa.ps1 exited with exit code %errorlevel%
exit %errorlevel%