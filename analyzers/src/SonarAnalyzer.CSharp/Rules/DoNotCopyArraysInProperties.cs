/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotCopyArraysInProperties : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2365";
        private const string MessageFormat = "Refactor '{0}' into a method, properties should not copy collections.";

        private static readonly DiagnosticDescriptor s2365 =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(s2365);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var property = (PropertyDeclarationSyntax)c.Node;

                    var body = GetPropertyBody(property);
                    if (body == null)
                    {
                        return;
                    }

                    var walker = new PropertyWalker(c.Model);

                    walker.SafeVisit(body);

                    foreach (var location in walker.Locations)
                    {
                        c.ReportIssue(s2365, location, property.Identifier.Text);
                    }
                },
                SyntaxKind.PropertyDeclaration);
        }

        private static SyntaxNode GetPropertyBody(PropertyDeclarationSyntax property)
        {
            return property.ExpressionBody ??
                property.AccessorList
                    .Accessors
                    .Where(a => a.IsKind(SyntaxKind.GetAccessorDeclaration))
                    .Select(a => (SyntaxNode)a.Body ?? a.ExpressionBody())
                    .FirstOrDefault();
        }

        private sealed class PropertyWalker : SafeCSharpSyntaxWalker
        {
            private readonly SemanticModel semanticModel;
            private readonly List<Location> locations = new();

            private static readonly HashSet<SyntaxKind> ReturnStatements = new()
            {
                SyntaxKind.ReturnStatement,
                SyntaxKind.ArrowExpressionClause,
            };

            public IEnumerable<Location> Locations => locations;

            public PropertyWalker(SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (!(this.semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol invokedSymbol))
                {
                    return;
                }

                if (!invokedSymbol.IsArrayClone() &&
                    !invokedSymbol.IsEnumerableToList() &&
                    !invokedSymbol.IsEnumerableToArray())
                {
                    return;
                }

                var returnOrAssignment = node
                    .Ancestors()
                    .FirstOrDefault(IsReturnOrAssignment);

                if (IsReturn(returnOrAssignment))
                {
                    this.locations.Add(node.Expression.GetLocation());
                }
            }

            private static bool IsReturnOrAssignment(SyntaxNode node)
            {
                return IsReturn(node)
                    || node.IsKind(SyntaxKind.SimpleAssignmentExpression);
            }

            private static bool IsReturn(SyntaxNode node)
            {
                return node != null && node.IsAnyKind(ReturnStatements);
            }
        }
    }
}
