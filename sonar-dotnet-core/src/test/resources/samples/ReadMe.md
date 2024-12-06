# Sample projects

The projects in this folder are used for generating samples like unit test or coverage reports.

## How to generate test reports

### xUnit

```powershell
dotnet test .\Calculator.xUnit\Calculator.xUnit.csproj --logger:xunit
```

### MSTest

```powershell
dotnet test .\Calculator.MSTest\Calculator.MSTest.csproj --logger:trx
```

### NUnit 3 & 4

```powershell
dotnet test .\Calculator.NUnit3\Calculator.NUnit3.csproj --logger:nunit
dotnet test .\Calculator.NUnit4\Calculator.NUnit4.csproj --logger:nunit
```

## Delete all the test results folders

```powershell
Get-ChildItem . -Include TestResults -Recurse -Force | Remove-Item -Recurse -Force
```

## Regenerate all the test reports

```powershell
# Delete all the test results folders
Get-ChildItem . -Include TestResults -Recurse -Force | Remove-Item -Recurse -Force

# Rebuild the solution
dotnet clean
dotnet build --no-incremental -nr:false

# Run the tests and generate the test reports
dotnet test .\Calculator.xUnit\Calculator.xUnit.csproj --logger:xunit
dotnet test .\Calculator.MSTest\Calculator.MSTest.csproj --logger:trx
dotnet test .\Calculator.NUnit3\Calculator.NUnit3.csproj --logger:nunit
dotnet test .\Calculator.NUnit4\Calculator.NUnit4.csproj --logger:nunit
```
