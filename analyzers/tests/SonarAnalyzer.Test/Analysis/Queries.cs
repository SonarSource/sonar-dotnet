using System.IO;
using System.Reflection;

namespace SonarAnalyzer.UnitTest.Analysis;

[TestClass]
public class Queries
{
    private static readonly HashSet<string> CSharpOnly =
    [
        "S1116",
        "S1199",
        "S1264",
        "S1848",
    ];

    [TestMethod]
    public void Find_rules_with_limited_code_size_to_implement_to_VB()
    {
        var csharps = typeof(SonarAnalyzer.CSharp.Rules.AbstractClassToInterface).Assembly
            .GetExportedTypes()
            .Where(x => !x.IsAbstract && x.GetCustomAttributes<DiagnosticAnalyzerAttribute>().Any())
            .Select(AnalyzerInfo.FromType)
            .Where(x =>
                x.Size.HasValue
                && x.Base == typeof(SonarDiagnosticAnalyzer)
                && !x.DiagnosticIds.Any(CSharpOnly.Contains))
            .OrderBy(x => x.Size)
            .ToArray();

        foreach (var csharp in csharps)
        {
            Console.WriteLine(csharp);
        }
    }
}

internal class AnalyzerInfo
{
    private DiagnosticAnalyzer analyzer;

    public string Name => Type.Name;
    public Type Type { get; init; }
    public Type Base => Type.BaseType;

    public IReadOnlyCollection<string> DiagnosticIds => [.. analyzer.SupportedDiagnostics.Select(x => x.Id)];

    public FileInfo File { get; init; }

    public long? Size => File.Exists ? File.Length : null;

    public override string ToString() =>
        $"{string.Join(",", DiagnosticIds)} {Name}, Size: {Size / 1000.0:0.0}kb";

    public static AnalyzerInfo FromType(Type type)
    {
        var name = $"SonarAnalyzer.CSharp/Rules/{type.Name}.cs";
        var file = new FileInfo(Path.Combine(type.Assembly.Location, "../../../../../../src", name));
        var instance = (DiagnosticAnalyzer)Activator.CreateInstance(type);

        return new AnalyzerInfo
        {
            Type = type,
            File = file,
            analyzer = instance,
        };
    }
}
