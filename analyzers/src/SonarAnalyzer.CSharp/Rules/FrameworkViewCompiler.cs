/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FrameworkViewCompiler : SonarDiagnosticAnalyzer
{
    private ImmutableArray<DiagnosticAnalyzer> Rules;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

    protected override bool EnableConcurrentExecution => false;

    public FrameworkViewCompiler()
    {
        Rules = RuleFinder2.CreateAnalyzers(AnalyzerLanguage.CSharp, false).ToImmutableArray();
        SupportedDiagnostics = Rules.SelectMany(x => x.SupportedDiagnostics).ToImmutableArray();
    }

    protected override void Initialize(SonarAnalysisContext context) =>

        context.RegisterCompilationAction(
            c =>
            {
                var projectConfiguration = c.ProjectConfiguration();
                var root = Path.GetDirectoryName(projectConfiguration.ProjectPath);
                var supportedDiagnostics = SupportedDiagnostics.ToImmutableDictionary(x => x.Id, x => ReportDiagnostic.Warn);

                var dummy = CompileViews(c.Compilation, root)
                    .WithOptions(c.Compilation.Options.WithSpecificDiagnosticOptions(supportedDiagnostics))
                    .WithAnalyzers(Rules, c.Options);

                var diagnostics = dummy.GetAnalyzerDiagnosticsAsync().Result;
                foreach (var diagnostic in diagnostics)
                {
                    c.ReportIssue(diagnostic);
                }
            });

    Compilation CompileViews(Compilation compilation, string rootDir)
    {
        FilesToAnalyzeProvider filesProvider = new(Directory.GetFiles(rootDir, "*.*", SearchOption.AllDirectories));
        var razorCompiler = new RazorCompiler(rootDir, filesProvider);
        var dummyCompilation = compilation;

        var documents = razorCompiler.CompileAll();
        foreach (var razorDocument in documents)
        {
            if (razorDocument.GetCSharpDocument()?.GeneratedCode is { } csharpCode)
            {
                var razorTree = CSharpSyntaxTree.ParseText(
                    csharpCode,
                    new CSharpParseOptions(compilation.GetLanguageVersion()),
                    path: "x.cshtml.g.cs"); // TODO: Give unique names to all the files, e.g. 0.cshtml.g.cs, 1.cshtml.g.cs, etc.
                dummyCompilation = dummyCompilation.AddSyntaxTrees(razorTree);
            }
        }
        return dummyCompilation;
    }
}
#region RULE_FINDER
// Copied from the test framework.
internal static class RuleFinder2
{
    public static IEnumerable<Type> AllAnalyzerTypes { get; }       // Rules and Utility analyzers
    public static IEnumerable<Type> AllTypesWithDiagnosticAnalyzerAttribute { get; }
    public static IEnumerable<Type> RuleAnalyzerTypes { get; }      // Rules-only, without Utility analyzers
    public static IEnumerable<Type> UtilityAnalyzerTypes { get; }
    public static IEnumerable<Type> CodeFixTypes { get; }

    static RuleFinder2()
    {
        var allTypes = new[]
        {
            typeof(FlagsEnumZeroMember),
            typeof(FlagsEnumZeroMemberBase<int>)
        }
            .SelectMany(x => x.Assembly.GetExportedTypes())
            .ToArray();
        CodeFixTypes = allTypes.Where(x => typeof(CodeFixProvider).IsAssignableFrom(x) && x.GetCustomAttributes<ExportCodeFixProviderAttribute>().Any()).ToArray();
        AllAnalyzerTypes = allTypes.Where(x => x.IsSubclassOf(typeof(DiagnosticAnalyzer)) && x.GetCustomAttributes<DiagnosticAnalyzerAttribute>().Any()).ToArray();
        AllTypesWithDiagnosticAnalyzerAttribute = allTypes.Where(x => x.GetCustomAttributes<DiagnosticAnalyzerAttribute>().Any()).ToArray();
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
        foreach (var type in types.Where(x => !IsParameterized(x)))
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
}

#endregion

#region RAZOR_COMPILER

// Copied from sonar-security.

/// <summary>
/// Compiler for manual Razor Views cross-compilation to C# code for .NET Framework projects.
/// We don't need to manually compile .NET Core Razor projects. They are compiled during build time and analyzed directly with Roslyn analyzers.
/// </summary>
internal sealed class RazorCompiler
{
    public const string SonarRazorCompiledItemAttribute = nameof(SonarRazorCompiledItemAttribute);

    private static readonly Regex CshtmlFileRegex = new(@"\.cshtml$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2000));

    private readonly string rootDirectory;
    private readonly string[] viewPaths;
    private readonly Dictionary<string, Engine> engines = new();    // Cache for Razor engines based on the View and web.config directory.

    public RazorCompiler(string rootDirectory, FilesToAnalyzeProvider filesToAnalyzeProvider)
    {
        this.rootDirectory = rootDirectory;
        viewPaths = filesToAnalyzeProvider.FindFiles(CshtmlFileRegex).ToArray();
        if (viewPaths.Length != 0)
        {
            BuildEngines(filesToAnalyzeProvider.FindFiles("web.config"));
        }
    }

    public IEnumerable<RazorCodeDocument> CompileAll()
    {
        foreach (var viewPath in viewPaths)
        {
            if (FindEngine(Path.GetDirectoryName(viewPath)) is { } engine)
            {
                yield return engine.Compile(viewPath);
            }
        }
    }

    private void BuildEngines(IEnumerable<string> webConfigs)
    {
        // Each web.config can change the list of imported namespace and we need a separate engine for it
        var razorFileSystem = RazorProjectFileSystem.Create(rootDirectory);
        foreach (var webConfigPath in webConfigs.OrderBy(x => x.Length))
        {
            var webConfig = File.ReadAllText(webConfigPath);
            if (webConfig.Contains("<system.web.webPages.razor")
                && ParseXDocument(webConfig)?.XPathSelectElement("configuration/system.web.webPages.razor/pages/namespaces")?.Elements() is { } xmlElements
                && xmlElements.Any())
            {
                var webConfigDir = Path.GetDirectoryName(webConfigPath);
                var namespaces = ReadNamespaces(webConfigDir, xmlElements);
                engines.Add(webConfigDir, new Engine(razorFileSystem, namespaces));
            }
        }
        if (!engines.TryGetValue(rootDirectory, out var rootEngine))
        {
            rootEngine = new Engine(razorFileSystem, Array.Empty<string>());
        }
        // Fill cache with values for each View directory
        foreach (var directory in viewPaths.Select(Path.GetDirectoryName).Distinct().OrderBy(x => x.Length))
        {
            if (!engines.ContainsKey(directory))
            {
                engines.Add(directory, FindEngine(directory) ?? rootEngine);
            }
        }
    }

    private string[] ReadNamespaces(string webConfigDir, IEnumerable<XElement> xmlElements)
    {
        // web.config structure is hierarchical. Every level inherits parent's configuration and modifies it with Add/Clear operations.
        var ret = FindEngine(webConfigDir)?.Namespaces.ToList() ?? new List<string>();
        foreach (var element in xmlElements)
        {
            switch (element.Name.LocalName)
            {
                case "add":
                    var ns = element.Attribute("namespace")?.Value;
                    if (!string.IsNullOrEmpty(ns))
                    {
                        ret.Add(ns);
                    }
                    break;

                case "clear":
                    ret.Clear();
                    break;
            }
        }
        return ret.Distinct().ToArray();
    }

    private Engine FindEngine(string directory)
    {
        while (directory.StartsWith(rootDirectory, StringComparison.OrdinalIgnoreCase))
        {
            if (engines.ContainsKey(directory))
            {
                return engines[directory];
            }
            directory = Path.GetDirectoryName(directory);
        }
        return null;
    }

    private static XDocument ParseXDocument(string text)
    {
        try
        {
            return XDocument.Parse(text);
        }
        catch
        {
            return null;
        }
    }

    private sealed class Engine
    {
        public readonly string[] Namespaces;
        private readonly RazorProjectEngine razorProjectEngine;

        public Engine(RazorProjectFileSystem razorFileSystem, string[] namespaces)
        {
            Namespaces = namespaces;
            razorProjectEngine = RazorProjectEngine.Create(RazorConfiguration.Default, razorFileSystem, builder =>
            {
                builder.AddDefaultImports(Namespaces.Select(x => $"@using {x}").ToArray());
                builder.AddDirective(ModificationPass.Directive);
                builder.Features.Add(new ModificationPass());
                var targetExtension = builder.Features.OfType<IRazorTargetExtensionFeature>().Single();
                UpdateCompiledItemAttributeName(targetExtension.TargetExtensions.Single(x => x.GetType().Name == "MetadataAttributeTargetExtension"));
            });
        }

        public RazorCodeDocument Compile(string viewPath) =>
            razorProjectEngine.Process(razorProjectEngine.FileSystem.GetItem(viewPath, FileKinds.Legacy));

        private static void UpdateCompiledItemAttributeName(ICodeTargetExtension metadataAttributeTargetExtension) =>
            metadataAttributeTargetExtension.GetType().GetProperty("CompiledItemAttributeName").SetValue(metadataAttributeTargetExtension, "global::" + SonarRazorCompiledItemAttribute);
    }

    private sealed class ModificationPass : IRazorOptimizationPass
    {
        // This represents the '@model' directive in Razor syntax tree. AddTypeToken() ensures that there will be exactly one (space separated) type token following the directive.
        // i.e.: @model ThisIs.Single.TypeToken
        public static readonly DirectiveDescriptor Directive = DirectiveDescriptor.CreateDirective("model", DirectiveKind.SingleLine, builder => builder.AddTypeToken());

        public int Order => 1;  // Must be higher than MetadataAttributePass.Order == 0, that generates the attribute nodes.
        public RazorEngine Engine { get; set; }

        public void Execute(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            var namespaceNode = FindNode<NamespaceDeclarationIntermediateNode>(documentNode);
            var classNode = FindNode<ClassDeclarationIntermediateNode>(namespaceNode);
            var methodNode = FindNode<MethodDeclarationIntermediateNode>(classNode);
            var modelNode = FindNode<DirectiveIntermediateNode>(methodNode, x => x.Directive == Directive);
            documentNode.Children.Add(new SonarAttributeIntermediateNode());
            namespaceNode.Children.Remove(namespaceNode.Children.Single(x => x.GetType().Name == "RazorSourceChecksumAttributeIntermediateNode"));
            classNode.BaseType = $"global::System.Web.Mvc.WebViewPage<{modelNode?.Tokens.Single().Content ?? "dynamic"}>";
            methodNode.Modifiers.Remove("async");
            methodNode.ReturnType = "void";
            methodNode.MethodName = "Execute";
        }

        private static TNode FindNode<TNode>(IntermediateNode parent, Func<TNode, bool> predicate = null) where TNode : IntermediateNode =>
            parent.Children.OfType<TNode>().FirstOrDefault(x => predicate is null || predicate(x));
    }

    private sealed class SonarAttributeIntermediateNode : ExtensionIntermediateNode
    {
        public override IntermediateNodeCollection Children => IntermediateNodeCollection.ReadOnly;

        public override void Accept(IntermediateNodeVisitor visitor) =>
            AcceptExtensionNode(this, visitor);

        public override void WriteNode(CodeTarget target, CodeRenderingContext context) =>
            // The constructor signature must be same as the original Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute
            context.CodeWriter.WriteLine($$"""
            public sealed class {{SonarRazorCompiledItemAttribute}} : System.Attribute
                {
                    public SonarRazorCompiledItemAttribute(System.Type type, string kind, string identifier) { }
                }
            """);
    }
}
#endregion

