/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotNestTypesInArguments : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4017";
        private const string MessageFormat = "Refactor this method to remove the nested type argument.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var argumentTypeSymbols = GetParametersSyntaxNodes(c.Node).Where(p => MaxDepthReached(p, c.SemanticModel));

                    foreach (var argument in argumentTypeSymbols)
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, argument.GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKindEx.LocalFunctionStatement);

        private static bool MaxDepthReached(SyntaxNode parameterSyntax, SemanticModel model)
        {
            var walker = new GenericWalker(2, model);
            walker.SafeVisit(parameterSyntax);
            return walker.IsMaxDepthReached;
        }

        private static IEnumerable<ParameterSyntax> GetParametersSyntaxNodes(SyntaxNode node) =>
            node switch
            {
                MethodDeclarationSyntax methodDeclarationSyntax => methodDeclarationSyntax.ParameterList.Parameters,
                var wrapper when LocalFunctionStatementSyntaxWrapper.IsInstance(wrapper) => ((LocalFunctionStatementSyntaxWrapper)wrapper).ParameterList.Parameters,
                _ => Enumerable.Empty<ParameterSyntax>()
            };

        private sealed class GenericWalker : SafeCSharpSyntaxWalker
        {
            private static readonly ImmutableArray<KnownType> IgnoredTypes =
                KnownType.SystemFuncVariants
                         .Union(KnownType.SystemActionVariants)
                         .Union(new[] { KnownType.System_Linq_Expressions_Expression_T })
                         .ToImmutableArray();

            private readonly int maxDepth;
            private readonly SemanticModel model;

            private int depth;

            public bool IsMaxDepthReached { get; private set; }

            public GenericWalker(int maxDepth, SemanticModel model)
            {
                this.maxDepth = maxDepth;
                this.model = model;
            }

            public override void VisitGenericName(GenericNameSyntax node)
            {
                if (model.GetSymbolInfo(node).Symbol is not INamedTypeSymbol namedTypeSymbol)
                {
                    return;
                }

                if (namedTypeSymbol.ConstructedFrom.IsAny(IgnoredTypes))
                {
                    base.VisitGenericName(node);
                }
                else
                {
                    if (depth == maxDepth - 1)
                    {
                        IsMaxDepthReached = true;
                    }
                    else
                    {
                        depth++;
                        base.VisitGenericName(node);
                        depth--;
                    }
                }
            }
        }
    }
}
