@echo off
setlocal ENABLEDELAYEDEXPANSION

PowerShell -NonInteractive -NoProfile -ExecutionPolicy Unrestricted -Command "scripts\build\qa.ps1"
echo From Cmd.exe: sonar-csharp qa.ps1 exited with exit code !errorlevel!
exit !errorlevel!