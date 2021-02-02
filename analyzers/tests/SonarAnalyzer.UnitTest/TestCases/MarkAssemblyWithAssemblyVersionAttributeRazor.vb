Imports System.Reflection
Imports Microsoft.AspNetCore.Razor.Hosting

<Assembly: AssemblyCompany("SonarSource")>
' https://github.com/SonarSource/sonar-dotnet/issues/2921
' We should not raise on Razor ProjectName.Views.dll auto-generated assemblies
<Assembly: RazorCompiledItem(GetType(Sample), "mvc.1.0.view", "@/Views/Fake/Index.cshtml")>

Public Class Sample

End Class
