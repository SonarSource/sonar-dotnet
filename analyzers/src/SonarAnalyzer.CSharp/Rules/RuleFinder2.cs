using System.Reflection;
using System.Reflection.Metadata;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Rules;

// Copied from the test framework.
// ToDo: rework both this class and the RuleFinder from the test framework.
internal static class RuleFinder2
{
    private const string DotDelimiterString = ".";

    public static IEnumerable<Type> AllAnalyzerTypes { get; }       // Rules and Utility analyzers
    public static IEnumerable<Type> RuleAnalyzerTypes { get; }      // Rules-only, without Utility analyzers
    public static IEnumerable<Type> UtilityAnalyzerTypes { get; }

    static RuleFinder2()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        List<Type> allTypes;
        using (var assemblyMetadata = AssemblyMetadata.CreateFromFile(executingAssembly.Location))
        {
            allTypes = assemblyMetadata
                       .GetModules()
                       .SelectMany(GetFullyQualifiedNames)
                       .Select(executingAssembly.GetType)
                       .ToList();
        }

        AllAnalyzerTypes = allTypes.Where(x => x.IsSubclassOf(typeof(DiagnosticAnalyzer)) && x.GetCustomAttributes<DiagnosticAnalyzerAttribute>().Any()).ToArray();
        UtilityAnalyzerTypes = AllAnalyzerTypes.Where(x => typeof(UtilityAnalyzerBase).IsAssignableFrom(x)).ToList();
        RuleAnalyzerTypes = AllAnalyzerTypes.Except(UtilityAnalyzerTypes).ToList();
    }

    public static IEnumerable<Type> GetAnalyzerTypes(AnalyzerLanguage language) =>
        RuleAnalyzerTypes.Where(x => TargetLanguage(x) == language);

    public static IEnumerable<DiagnosticAnalyzer> CreateAnalyzers(AnalyzerLanguage language, bool includeUtilityAnalyzers)
    {
        var types = GetAnalyzerTypes(language);
        if (includeUtilityAnalyzers)
        {
            types = types.Concat(UtilityAnalyzerTypes.Where(x => TargetLanguage(x) == language));
        }

        // EXCLUDE ourselves, or FrameworkViewCompiler will try to instantiate itself indefinitely, until StackOverflowException.
        types = types.Where(x => x != typeof(FrameworkViewCompiler));
        foreach (var type in types)
        {
            yield return typeof(HotspotDiagnosticAnalyzer).IsAssignableFrom(type) && type.GetConstructor([typeof(IAnalyzerConfiguration)]) is not null
                ? (DiagnosticAnalyzer)Activator.CreateInstance(type, AnalyzerConfiguration.AlwaysEnabled)
                : (DiagnosticAnalyzer)Activator.CreateInstance(type);
        }
    }

    public static bool IsParameterized(Type analyzerType) =>
        analyzerType.GetProperties().Any(x => x.GetCustomAttributes<RuleParameterAttribute>().Any());

    private static AnalyzerLanguage TargetLanguage(MemberInfo analyzerType)
    {
        var languages = analyzerType.GetCustomAttributes<DiagnosticAnalyzerAttribute>().SingleOrDefault()?.Languages
            ?? throw new NotSupportedException($"Can not find any language for the given type {analyzerType.Name}!");
        return languages.Length == 1
            ? AnalyzerLanguage.FromName(languages.Single())
            : throw new NotSupportedException($"Analyzer can not have multiple languages: {analyzerType.Name}");
    }

    private static IEnumerable<string> GetFullyQualifiedNames(ModuleMetadata module)
    {
        var metadataReader = module.GetMetadataReader();
        return metadataReader.TypeDefinitions
                             .Select(definitionHandle => metadataReader.GetTypeDefinition(definitionHandle))
                             .Where(definition => definition.GetCustomAttributes().Any(attributeHandle => IsDiagnosticAnalyzerAttribute(attributeHandle, metadataReader)))
                             .Select(definition => GetFullyQualifiedTypeName(definition, metadataReader));
    }

    private static bool IsDiagnosticAnalyzerAttribute(CustomAttributeHandle attributeHandle, MetadataReader metadataReader)
    {
        var attribute = metadataReader.GetCustomAttribute(attributeHandle);
        var ctor = attribute.Constructor;
        if (ctor.Kind == HandleKind.MemberReference)
        {
            var memberRef = metadataReader.GetMemberReference((MemberReferenceHandle)ctor);
            var ctorType = memberRef.Parent;
            if (metadataReader.StringComparer.Equals(memberRef.Name, WellKnownMemberNames.InstanceConstructorName)
                && ctorType.Kind == HandleKind.TypeReference
                && metadataReader.GetString(metadataReader.GetTypeReference((TypeReferenceHandle)ctorType).Name) == "DiagnosticAnalyzerAttribute"
                && metadataReader.GetString(metadataReader.GetTypeReference((TypeReferenceHandle)ctorType).Namespace) == "Microsoft.CodeAnalysis.Diagnostics")
            {
                return true;
            }
        }
        return false;
    }

    private static string GetFullyQualifiedTypeName(TypeDefinition typeDef, MetadataReader metadataReader)
    {
        var declaringType = typeDef.GetDeclaringType();
        if (declaringType.IsNil) // Non nested type - simply get the full name
        {
            return BuildQualifiedName(metadataReader.GetString(typeDef.Namespace), metadataReader.GetString(typeDef.Name));
        }
        else
        {
            var declaringTypeDef = metadataReader.GetTypeDefinition(declaringType);
            return GetFullyQualifiedTypeName(declaringTypeDef, metadataReader) + "+" + metadataReader.GetString(typeDef.Name);
        }
    }

    private static string BuildQualifiedName(string qualifier, string name) =>
        string.IsNullOrEmpty(qualifier)
            ? name
            : string.Concat(qualifier, DotDelimiterString, name);
}
