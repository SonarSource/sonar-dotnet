## To re-generate the opencover.xml

With administrator rights:
```
nuget restore .\MultipleProjects.sln

msbuild .\MultipleProjects.sln /t:Rebuild

OpenCover.Console.exe -output:"opencover.xml" -register:administrator -target:"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" -targetargs:"C:\Workspace\sonar-dotnet\its\projects\MultipleProjects\SecondProjectTests\bin\Debug\netcoreapp3.1\SecondProjectTests.dll C:\Workspace\sonar-dotnet\its\projects\MultipleProjects\FirstProjectTests\bin\Debug\netcoreapp3.1\FirstProjectTests.dll"
```

After that, replace the absolute paths with relative paths (e.g. `fullPath="FirstProject\FirstClass.cs"`).

## To re-generate the VisualStudio.coveragexml

```
nuget restore .\MultipleProjects.sln

msbuild .\MultipleProjects.sln /t:Rebuild

vstest.console.exe /EnableCodeCoverage .\FirstProjectTests\bin\Debug\netcoreapp3.1\FirstProjectTests.dll .\SecondProjectTests\bin\Debug\netcoreapp3.1\SecondProjectTests.dll

CodeCoverage.exe analyze /output:"VisualStudio.coveragexml" "MultipleProjects\TestResults\<GUID>\<REPORT_NAME>.coverage"
```

## Important : manual modification needed

For both OpenCover and VS Coverage, you have to manually replace absolute paths with relative paths for the Integration Tests to work.

For OpenCover, you should have e.g. `fullPath="FirstProject\FirstClass.cs"`.

For VS Coverage, you should have e.g. `<source_file id="0" path="FirstProject\SecondClass.cs">`.