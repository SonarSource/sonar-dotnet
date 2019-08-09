using System;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class MlirGenerator : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1234";
        internal static int id = 0;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                new DiagnosticDescriptor(DiagnosticId, string.Empty, string.Empty, "Sonar CBDE", DiagnosticSeverity.Hidden, true, null, null, new[] { "MainSourceScope", "TestSourceScope"})
            );

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (MethodDeclarationSyntax)c.Node;
                    WriteMlir(declaration, c.SemanticModel);
                },
                SyntaxKind.MethodDeclaration);
        }


        private void WriteMlir(MethodDeclarationSyntax node, SemanticModel semanticModel)
        {
            try
            {
                var path = Path.Combine(Path.GetTempPath(), $"csharp.{node.Identifier}.{id.ToString()}.mlir");
                ++id;
                using (var writer = new StreamWriter(path))
                {
                    var exporter = new MLIRExporter(writer, semanticModel, false);
                    exporter.ExportFunction(node);
                }
            }
            catch (System.NotSupportedException e)
            {
                // expressions that are not supported by CFG (eg: DeclarationExpression, DefaultLiteralExpression)
                System.Console.WriteLine(e.Message);
            }
        }
    }
}
