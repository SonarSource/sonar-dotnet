using System.Reflection;
using Microsoft.AspNetCore.Razor.Hosting;

[assembly: AssemblyCompany("SonarSource")]
// We should not raise on Razor ProjectName.Views.dll auto-generated assemblies
[assembly: RazorCompiledItem(typeof(Sample), "mvc.1.0.view", "@/Views/Fake/Index.cshtml")]

public class Sample { }
