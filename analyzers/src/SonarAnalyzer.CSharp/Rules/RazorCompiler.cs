using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace SonarAnalyzer.Rules;

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
