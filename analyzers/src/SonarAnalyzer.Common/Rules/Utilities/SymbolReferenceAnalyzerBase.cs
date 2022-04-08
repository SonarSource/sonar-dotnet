/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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

        protected abstract IEnumerable<ReferenceInfo> CreateDeclarationReferenceInfo(SyntaxNode node, SemanticModel model);

        protected abstract IEnumerable<SyntaxNode> GetDeclarations(SyntaxNode node);

        protected abstract StringComparer IdentifierComparer { get; }

        protected SymbolReferenceAnalyzerBase() : base(DiagnosticId, Title) { }

        protected sealed override SymbolReferenceInfo CreateMessage(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var symbolReferenceInfo = new SymbolReferenceInfo { FilePath = syntaxTree.FilePath };

            foreach (var references in GetReferences(syntaxTree.GetRoot(), semanticModel).GroupBy(x => x.Symbol))
            {
                if (GetSymbolReference(references.ToArray(), syntaxTree) is { } reference)
                {
                    symbolReferenceInfo.Reference.Add(reference);
                }
            }

            return symbolReferenceInfo;
        }

        protected override bool ShouldGenerateMetrics(SyntaxTree tree) =>
            base.ShouldGenerateMetrics(tree)
            && !HasTooManyTokens(tree);

        private IEnumerable<ReferenceInfo> GetReferences(SyntaxNode root, SemanticModel model)
        {
            var references = new HashSet<ReferenceInfo>();
            var knownIdentifiers = new HashSet<string>(IdentifierComparer);

            foreach (var declaration in GetDeclarations(root))
            {
                var declarationReferences = CreateDeclarationReferenceInfo(declaration, model);
                if (declarationReferences == null)
                {
                    continue;
                }

                foreach (var reference in declarationReferences)
                {
                    references.Add(reference);
                    knownIdentifiers.Add(reference.Identifier.ValueText);
                }
            }

            foreach (var identifier in root.DescendantTokens().Where(x => Language.Syntax.IsKind(x, Language.SyntaxKind.IdentifierToken)))
            {
                if (knownIdentifiers.Contains(identifier.Text)
                    && GetBindableParent(identifier) is { } parent
                    && references.All(x => x.Node != parent)
                    && GetReferenceSymbol(parent, model) is { } symbol)
                {
                    references.Add(new ReferenceInfo(parent, identifier, symbol, false));
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

        private static SymbolReferenceInfo.Types.SymbolReference GetSymbolReference(ReferenceInfo[] references, SyntaxTree tree)
        {
            var declarationSpan = GetDeclarationSpan(references);
            if (!declarationSpan.HasValue)
            {
                return null;
            }

            var symbolReference = new SymbolReferenceInfo.Types.SymbolReference { Declaration = GetTextRange(Location.Create(tree, declarationSpan.Value).GetLineSpan()) };
            foreach (var reference in references.Where(x => !x.IsDeclaration).Select(x => x.Identifier))
            {
                symbolReference.Reference.Add(GetTextRange(Location.Create(tree, reference.Span).GetLineSpan()));
            }
            return symbolReference;
        }

        private static TextSpan? GetDeclarationSpan(IEnumerable<ReferenceInfo> references) =>
            references.FirstOrDefault(x => x.IsDeclaration) is { } declaration
                ? declaration.Identifier.Span
                : null;

        private static bool HasTooManyTokens(SyntaxTree syntaxTree) =>
            syntaxTree.GetRoot().DescendantTokens().Count() > TokenCountThreshold;

        protected sealed record ReferenceInfo(SyntaxNode Node, SyntaxToken Identifier, ISymbol Symbol, bool IsDeclaration);
    }
}
