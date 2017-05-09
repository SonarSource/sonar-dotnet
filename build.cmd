@echo off
setlocal ENABLEDELAYEDEXPANSION

PowerShell -NonInteractive -NoProfile -ExecutionPolicy Unrestricted -Command ".\build.ps1" 
echo From Cmd.exe: sonar-csharp build.ps1 exited with exit code !errorlevel!
exit !errorlevel!