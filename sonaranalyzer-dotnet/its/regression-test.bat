@echo OFF

REM Set the current working directory to the script's folder
pushd %~dp0

REM Setup the actual folder with the expected files
echo Initializing the actual issues Git repo with the expected ones...
rmdir /S /Q actual 2>NUL
mkdir actual
xcopy expected actual /E >NUL
pushd actual
git init
git config user.email "regression-test@example.com"
git config user.name "regression-test"
git add -A .
git commit -m "Initial commit with expected files" >NUL
del /F /S /Q *.json >NUL
popd

REM Setup output folder
rmdir /S /Q output 2>NUL
mkdir output

REM Generate the SonarAnalyzer all rules ruleset
powershell.exe -executionpolicy bypass ./AllRulesGenerator.ps1 > output/AllSonarAnalyzerRules.ruleset
if not %ERRORLEVEL%==0 goto ps_error

REM Install the imports before targets file
mkdir "%USERPROFILE%\AppData\Local\Microsoft\MSBuild\14.0\Microsoft.Common.targets\ImportBefore" 2>NUL
copy SonarAnalyzer.Testing.ImportBefore.targets "%USERPROFILE%\AppData\Local\Microsoft\MSBuild\14.0\Microsoft.Common.targets\ImportBefore"

REM Building projects
echo Building: Akka.NET
set PROJECT=akka.net
mkdir output\%PROJECT% 2>NUL
call %PROJECT%\sonarlint-build.bat > output\%PROJECT%.txt
if not %ERRORLEVEL%==0 goto build_error

echo Building: Nancy
set PROJECT=Nancy
mkdir output\%PROJECT% 2>NUL
call %PROJECT%\sonarlint-build.bat > output\%PROJECT%.txt
if not %ERRORLEVEL%==0 goto build_error

echo Building: Ember-MM
set PROJECT=Ember-MM
mkdir output\%PROJECT% 2>NUL
call %PROJECT%\sonarlint-build.bat > output\%PROJECT%.txt
if not %ERRORLEVEL%==0 goto build_error

REM Normalize SARIF reports
echo Normalizing the SARIF reports
powershell.exe -executionpolicy bypass ./CreateIssueReports.ps1
if not %ERRORLEVEL%==0 goto ps_error

REM Compute analyzer performances
echo Computing analyzer performances
powershell.exe -executionpolicy bypass ./ExtractAnalyzerPerformancesFromLogs.ps1
if not %ERRORLEVEL%==0 goto ps_error

REM Assert no differences
echo Checking for differences...
pushd actual
git add -A .
git diff --cached --exit-code >NUL
set _diff=%ERRORLEVEL%
popd
if not %_diff%==0 goto diff_error

REM Possible outcomes
echo SUCCESS: No differences were found!
set _exit=0
goto cleanup

:build_error
echo ERROR: The project %PROJECT% could not be built!
set _exit=1
goto cleanup

:diff_error
echo ERROR: There are differences between the actual and the expected issues!
set _exit=2
goto cleanup

:ps_error
echo ERROR: Error while executing an internal PowerShell script
set _exit=3
goto cleanup

REM Cleanup
:cleanup

REM Uninstall the imports before targets file
del "%USERPROFILE%\AppData\Local\Microsoft\MSBuild\14.0\Microsoft.Common.targets\ImportBefore\SonarAnalyzer.Testing.ImportBefore.targets" 2>NUL

REM Restore current working directory
popd

REM Exit
exit /B %_exit%
