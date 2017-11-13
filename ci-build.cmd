@echo off
setlocal ENABLEDELAYEDEXPANSION

if "%DEBUG%" == "true" (
    PowerShell -NonInteractive -NoProfile -ExecutionPolicy Unrestricted -Command "scripts\build\build.ps1" -Verbose
) else (
    PowerShell -NonInteractive -NoProfile -ExecutionPolicy Unrestricted -Command "scripts\build\build.ps1"
)
echo From Cmd.exe: sonar-csharp build.ps1 exited with exit code !errorlevel!
exit !errorlevel!