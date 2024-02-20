## To re-generate the opencover.xml

With administrator rights:
```
nuget restore .\CoverageTest.MultipleProjects.sln

msbuild .\CoverageTest.MultipleProjects.sln /t:Rebuild

OpenCover.Console.exe -output:"opencover.xml" -register:administrator -target:"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" -targetargs:"C:\Workspace\sonar-dotnet\its\projects\CoverageTest.MultipleProjects\SecondProjectTests\bin\Debug\net7.0\SecondProjectTests.dll C:\Workspace\sonar-dotnet\its\projects\CoverageTest.MultipleProjects\FirstProjectTests\bin\Debug\net7.0\FirstProjectTests.dll"
```

After that, replace the absolute paths with relative paths (e.g. `fullPath="FirstProject\FirstClass.cs"`).

## To re-generate the VisualStudio.coveragexml

```
nuget restore .\CoverageTest.MultipleProjects.sln

msbuild .\CoverageTest.MultipleProjects.sln /t:Rebuild

vstest.console.exe /EnableCodeCoverage .\FirstProjectTests\bin\Debug\net7.0\FirstProjectTests.dll .\SecondProjectTests\bin\Debug\net7.0\SecondProjectTests.dll

CodeCoverage.exe analyze /output:"VisualStudio.coveragexml" "CoverageTest.MultipleProjects\TestResults\<GUID>\<REPORT_NAME>.coverage"
```

## Important : manual modification needed

For both OpenCover and VS Coverage, you have to manually replace absolute paths with relative paths for the Integration Tests to work.

For OpenCover, you should have e.g. `fullPath="FirstProject\FirstClass.cs"`.

For VS Coverage, you should have e.g. `<source_file id="0" path="FirstProject\SecondClass.cs">`.