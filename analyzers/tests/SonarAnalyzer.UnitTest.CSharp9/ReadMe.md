# Proof of concept for running SonarAnalyzer.CSharp on C# 9 code 

## Current limitations

- Microsoft.CodeAnalysis.CSharp compatibility issues:

    In order to be able to run the analysis on C# 9 code we have to reference `Microsoft.CodeAnalysis.CSharp` (`3.7.0-4.final`) which is not compatible with the version used by the `SonarAnalyzer.CSharp` (`1.3.2`).
    
    If we try to reference the analyzer as a project, NuGet will report the following error:
    ```
    Error	NU1107	Version conflict detected for Microsoft.CodeAnalysis.Common. Install/reference Microsoft.CodeAnalysis.Common 3.7.0-4.final directly to project SonarAnalyzer.UnitTest.CSharp9 to resolve this issue. 
     SonarAnalyzer.UnitTest.CSharp9 -> Microsoft.CodeAnalysis.CSharp 3.7.0-4.final -> Microsoft.CodeAnalysis.Common (= 3.7.0-4.final) 
     SonarAnalyzer.UnitTest.CSharp9 -> SonarAnalyzer.CSharp -> Microsoft.CodeAnalysis.CSharp.Workspaces 1.3.2 -> Microsoft.CodeAnalysis.Workspaces.Common 1.3.2 -> Microsoft.CodeAnalysis.Common (= 1.3.2).	SonarAnalyzer.UnitTest.CSharp9	C:\src\sonar-dotnet\sonaranalyzer-dotnet\tests\SonarAnalyzer.UnitTest.CSharp9\SonarAnalyzer.UnitTest.CSharp9.csproj	1	
    ```
    
    However, as far as I can tell at this point, this doesn't seem to be a problem at runtime if we reference the analyzer as a file.
     
    **We have to see if this compatibility problem is indeed a breaking change or just a temporary problem in how preview packages are generated since this kind of problem didn't occured in the past (e.g. our test project which references version 3.4.0 is compatible with SonarAnalyzer.CSharp which is using 1.3.2)**  
    
- Our test framework is embedded in `SonarAnalyzer.Test` project which makes it harder to reuse. Incompatibilities between `Microsoft.CodeAnalysis.CSharp` versions accentuate that.
    Referencing the assembly directly can be a work around but it is not a nice solution. 
    
    **A possible solution would be to extract the test framework in a separate assembly which can be shared between test projects.** 
    
