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

using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules
{
    public abstract class SymbolReferenceAnalyzerBase<TSyntaxKind> : UtilityAnalyzerBase<TSyntaxKind, SymbolReferenceInfo>
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S9999-symbolRef";
        private const string Title = "Symbol reference calculator";
        private const int TokenCountThreshold = 40_000;

        protected sealed override string FileName => "symrefs.pb";

        protected abstract SyntaxNode GetBindableParent(SyntaxToken token);

        protected abstract ReferenceInfo[] CreateDeclarationReferenceInfo(SyntaxNode node, SemanticModel model);

        protected abstract IList<SyntaxNode> GetDeclarations(SyntaxNode node);

        protected SymbolReferenceAnalyzerBase() : base(DiagnosticId, Title) { }

        protected sealed override SymbolReferenceInfo CreateMessage(UtilityAnalyzerParameters parameters, SyntaxTree tree, SemanticModel model)
        {
            var filePath = GetFilePath(tree);
            var symbolReferenceInfo = new SymbolReferenceInfo { FilePath = filePath };
            var references = GetReferences(tree.GetRoot(), model);
            foreach (var symbol in references.Keys)
            {
                if (GetSymbolReference(references[symbol], filePath) is { } reference)
                {
                    symbolReferenceInfo.Reference.Add(reference);
                }
            }
            return symbolReferenceInfo;
        }

        protected sealed override bool ShouldGenerateMetrics(UtilityAnalyzerParameters parameters, SyntaxTree tree) =>
            base.ShouldGenerateMetrics(parameters, tree)
            && !HasTooManyTokens(tree);

        private Dictionary<ISymbol, List<ReferenceInfo>> GetReferences(SyntaxNode root, SemanticModel model)
        {
            var references = new Dictionary<ISymbol, List<ReferenceInfo>>();
            var knownIdentifiers = new HashSet<string>(Language.NameComparer);
            var knownNodes = new List<SyntaxNode>();
            var declarations = GetDeclarations(root);

            for (var i = 0; i < declarations.Count; i++)
            {
                var declarationReferences = CreateDeclarationReferenceInfo(declarations[i], model);
                if (declarationReferences == null)
                {
                    continue;
                }

                for (var j = 0; j < declarationReferences.Length; j++)
                {
                    var currentDeclaration = declarationReferences[j];
                    if (currentDeclaration.Symbol != null)
                    {
                        references
                            .GetOrAdd(currentDeclaration.Symbol, _ => new List<ReferenceInfo>())
                            .Add(currentDeclaration);
                        knownNodes.Add(currentDeclaration.Node);
                        knownIdentifiers.Add(currentDeclaration.Identifier.ValueText);
                    }
                }
            }

            foreach (var token in root.DescendantTokens())
            {
                if (Language.Syntax.IsKind(token, Language.SyntaxKind.IdentifierToken)
                    && knownIdentifiers.Contains(token.ValueText)
                    && GetBindableParent(token) is { } parent
                    && !knownNodes.Contains(parent)
                    && GetReferenceSymbol(parent, model) is { } symbol
                    && references.ContainsKey(symbol)
                    && references[symbol] is { } symbolRefs)
                {
                    symbolRefs.Add(new ReferenceInfo(parent, token, symbol, false));
                }
            }

            return references;
        }

        private static ISymbol GetReferenceSymbol(SyntaxNode node, SemanticModel model) =>
            model.GetSymbolInfo(node).Symbol switch
            {
                IMethodSymbol { MethodKind: MethodKind.Constructor, IsImplicitlyDeclared: true } constructor => constructor.ContainingType,
                var symbol => symbol
            };

        private static SymbolReferenceInfo.Types.SymbolReference GetSymbolReference(IReadOnlyList<ReferenceInfo> references, string filePath)
        {
            var declarationSpan = GetDeclarationSpan(references, filePath);
            if (!declarationSpan.HasValue)
            {
                return null;
            }

            var symbolReference = new SymbolReferenceInfo.Types.SymbolReference { Declaration = GetTextRange(declarationSpan.Value) };
            for (var i = 0; i < references.Count; i++)
            {
                var reference = references[i];
                if (!reference.IsDeclaration
                    && reference.Identifier.GetLocation().GetMappedLineSpanIfAvailable() is var mappedLineSpan
                    // Syntax tree can contain elements from external files (e.g. razor imports files)
                    // We need to make sure that we don't count these elements.
                    && string.Equals(mappedLineSpan.Path, filePath, StringComparison.OrdinalIgnoreCase))
                {
                    symbolReference.Reference.Add(GetTextRange(mappedLineSpan));
                }
            }
            return symbolReference;
        }

        private static FileLinePositionSpan? GetDeclarationSpan(IReadOnlyList<ReferenceInfo> references, string filePath)
        {
            for (var i = 0; i < references.Count; i++)
            {
                if (references[i].IsDeclaration
                    && references[i].Identifier.GetLocation().GetMappedLineSpanIfAvailable() is var mappedLineSpan
                    && string.Equals(mappedLineSpan.Path, filePath, StringComparison.OrdinalIgnoreCase))
                {
                    return mappedLineSpan;
                }
            }
            return null;
        }

        private static bool HasTooManyTokens(SyntaxTree syntaxTree) =>
            syntaxTree.GetRoot().DescendantTokens().Count() > TokenCountThreshold;

        protected sealed record ReferenceInfo(SyntaxNode Node, SyntaxToken Identifier, ISymbol Symbol, bool IsDeclaration);
    }
}
