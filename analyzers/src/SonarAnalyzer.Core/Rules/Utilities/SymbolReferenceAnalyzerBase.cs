/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Core.Rules;

public abstract class SymbolReferenceAnalyzerBase<TSyntaxKind> : UtilityAnalyzerBase<TSyntaxKind, SymbolReferenceInfo>
    where TSyntaxKind : struct, Enum
{
    private const string DiagnosticId = "S9999-symbolRef";
    private const string Title = "Symbol reference calculator";
    private const int TokenCountThreshold = 40_000;
    private const string RazorGeneratedFileSuffix = "_razor.g.cs";
    private const string RazorDeclGeneratedFileSuffix = "_razor.decl.g.cs";

    protected abstract SyntaxNode BindableParent(SyntaxToken token);

    protected abstract ReferenceInfo[] CreateDeclarationReferenceInfo(SyntaxNode node, SemanticModel model);

    protected abstract IList<SyntaxNode> Declarations(SyntaxNode node);

    protected sealed override string FileName => "symrefs.pb";

    protected SymbolReferenceAnalyzerBase() : base(DiagnosticId, Title) { }

    protected sealed override SymbolReferenceInfo CreateMessage(UtilityAnalyzerParameters parameters, SyntaxTree tree, SemanticModel model)
    {
        var declarationTree = RazorDeclarationTree(tree, model.Compilation);
        if (TokenCount(tree) + (declarationTree is null ? 0 : TokenCount(declarationTree)) > TokenCountThreshold)
        {
            return null;
        }
        var filePath = MapFilePath(tree);
        var symbolReferenceInfo = new SymbolReferenceInfo { FilePath = filePath };
        SemanticModel[] models = declarationTree is null ? [model] : [model, model.Compilation.GetSemanticModel(declarationTree)];
        var references = References(models);
        foreach (var symbol in references.Keys)
        {
            if (SymbolReference(references[symbol], filePath, models.Length > 1) is { } reference)
            {
                symbolReferenceInfo.Reference.Add(reference);
            }
        }
        return symbolReferenceInfo;
    }

    // Workaround for NET-5129: GetDeclarations is per tree, so analyze the decl tree together with the main tree and emit one message per file.
    // Details in the workaround comment on NET-5129.
    protected sealed override bool ShouldGenerateMetrics(UtilityAnalyzerParameters parameters, SyntaxTree tree) =>
        !tree.EndsWith(RazorDeclGeneratedFileSuffix) && base.ShouldGenerateMetrics(parameters, tree);

    private Dictionary<ISymbol, List<ReferenceInfo>> References(SemanticModel[] models)
    {
        var references = new Dictionary<ISymbol, List<ReferenceInfo>>();
        var knownIdentifiers = new HashSet<string>(Language.NameComparer);
        var knownNodes = new HashSet<SyntaxNode>();
        // All declarations need to be known before looking for references, as they can be in another tree
        foreach (var model in models)
        {
            var declarations = Declarations(model.SyntaxTree.GetRoot());
            for (var i = 0; i < declarations.Count; i++)
            {
                var declarationReferences = CreateDeclarationReferenceInfo(declarations[i], model);
                if (declarationReferences is null)
                {
                    continue;
                }

                for (var j = 0; j < declarationReferences.Length; j++)
                {
                    var currentDeclaration = declarationReferences[j];
                    if (currentDeclaration.Symbol is not null)
                    {
                        references.GetOrAdd(currentDeclaration.Symbol, _ => []).Add(currentDeclaration);
                        knownNodes.Add(currentDeclaration.Node);
                        knownIdentifiers.Add(currentDeclaration.Identifier.ValueText);
                    }
                }
            }
        }

        foreach (var model in models)
        {
            foreach (var token in model.SyntaxTree.GetRoot().DescendantTokens())
            {
                if (Language.Syntax.IsKind(token, Language.SyntaxKind.IdentifierToken)
                    && knownIdentifiers.Contains(token.ValueText)
                    && BindableParent(token) is { } parent
                    && !knownNodes.Contains(parent)
                    && ReferenceSymbol(parent, model) is { } symbol)
                {
                    foreach (var part in symbol.AllPartialParts.Where(references.ContainsKey))
                    {
                        references[part].Add(new(parent, token, part, false));
                    }
                }
            }
        }

        return references;
    }

    private static ISymbol ReferenceSymbol(SyntaxNode node, SemanticModel model) =>
        model.GetSymbolInfo(node).Symbol switch
        {
            IMethodSymbol { MethodKind: MethodKind.Constructor, IsImplicitlyDeclared: true } constructor => constructor.ContainingType,
            var symbol => symbol
        };

    private static SymbolReferenceInfo.Types.SymbolReference SymbolReference(IReadOnlyList<ReferenceInfo> references, string filePath, bool deduplicate)
    {
        var declarationSpan = DeclarationSpan(references, filePath);
        if (!declarationSpan.HasValue)
        {
            return null;
        }

        var symbolReference = new SymbolReferenceInfo.Types.SymbolReference { Declaration = ToTextRange(declarationSpan.Value) };
        // Razor generates some code, like @typeparam constraints, into both the main and the declaration tree
        var seen = deduplicate ? new HashSet<TextRange>() : null;
        for (var i = 0; i < references.Count; i++)
        {
            var reference = references[i];
            if (!reference.IsDeclaration
                && reference.Identifier.GetLocation().MappedLineSpanIfAvailable is var mappedLineSpan
                // Syntax tree can contain elements from external files (e.g. razor imports files)
                // We need to make sure that we don't count these elements.
                && string.Equals(mappedLineSpan.Path, filePath, StringComparison.OrdinalIgnoreCase)
                && ToTextRange(mappedLineSpan) is var range
                && (seen is null || seen.Add(range)))
            {
                symbolReference.Reference.Add(range);
            }
        }
        return symbolReference;
    }

    private static FileLinePositionSpan? DeclarationSpan(IReadOnlyList<ReferenceInfo> references, string filePath)
    {
        for (var i = 0; i < references.Count; i++)
        {
            if (references[i].IsDeclaration
                && references[i].Identifier.GetLocation().MappedLineSpanIfAvailable is var mappedLineSpan
                && string.Equals(mappedLineSpan.Path, filePath, StringComparison.OrdinalIgnoreCase))
            {
                return mappedLineSpan;
            }
        }
        return null;
    }

    // Workaround for NET-5129: pairs the decl tree by file name. Returns null (old per-tree behavior) if there is no match.
    private static SyntaxTree RazorDeclarationTree(SyntaxTree tree, Compilation compilation)
    {
        if (!tree.EndsWith(RazorGeneratedFileSuffix))
        {
            return null;
        }
        var declarationPath = tree.FilePath.Substring(0, tree.FilePath.Length - RazorGeneratedFileSuffix.Length) + RazorDeclGeneratedFileSuffix;
        return compilation.SyntaxTrees.FirstOrDefault(x => x.FilePath.Equals(declarationPath, StringComparison.Ordinal));
    }

    private static int TokenCount(SyntaxTree tree) =>
        tree.GetRoot().DescendantTokens().Count();

    protected sealed record ReferenceInfo(SyntaxNode Node, SyntaxToken Identifier, ISymbol Symbol, bool IsDeclaration);
}
