@echo off
setlocal ENABLEDELAYEDEXPANSION

if "%DEBUG%" == "true" (
    PowerShell -NonInteractive -NoProfile -ExecutionPolicy Unrestricted -Command "scripts\build\ci-qa.ps1" -Verbose
) else (
    PowerShell -NonInteractive -NoProfile -ExecutionPolicy Unrestricted -Command "scripts\build\ci-qa.ps1"
)
echo From Cmd.exe: sonar-csharp ci-qa.ps1 exited with exit code !errorlevel!
exit !errorlevel!