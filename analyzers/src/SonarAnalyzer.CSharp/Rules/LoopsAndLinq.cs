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
    public sealed class LoopsAndLinq : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3267";
        private const string MessageFormat = "{0}";
        private const string WhereMessageFormat = @"Loops should be simplified with ""LINQ"" expressions";
        private const string SelectMessageFormat = "Loop should be simplified by calling Select({0} => {0}.{1}))";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var forEachStatementSyntax = (ForEachStatementSyntax)c.Node;
                    if (CanBeSimplifiedUsingWhere(forEachStatementSyntax.Statement, out var ifConditionLocation))
                    {
                        c.ReportIssue(Rule.CreateDiagnostic(c.Compilation, forEachStatementSyntax.Expression.GetLocation(), new[] { ifConditionLocation }, WhereMessageFormat));
                    }
                    else
                    {
                        CheckIfCanBeSimplifiedUsingSelect(c, forEachStatementSyntax);
                    }
                },
                SyntaxKind.ForEachStatement);

        private static bool CanBeSimplifiedUsingWhere(SyntaxNode statement, out Location ifConditionLocation)
        {
            if (GetIfStatement(statement) is { } ifStatementSyntax && CanIfStatementBeMoved(ifStatementSyntax))
            {
                ifConditionLocation = ifStatementSyntax.Condition.GetLocation();
                return true;
            }

            ifConditionLocation = null;
            return false;
        }

        private static IfStatementSyntax GetIfStatement(SyntaxNode node) =>
            node switch
            {
                IfStatementSyntax ifStatementSyntax => ifStatementSyntax,
                BlockSyntax blockSyntax when blockSyntax.ChildNodes().Count() == 1 => GetIfStatement(blockSyntax.ChildNodes().Single()),
                _ => null
            };

        private static bool CanIfStatementBeMoved(IfStatementSyntax ifStatementSyntax)
        {
            return ifStatementSyntax.Else == null && (ConditionValidIsPattern() || ConditionValidInvocation());

            bool ConditionValidIsPattern() => ifStatementSyntax.Condition.IsAnyKind(SyntaxKind.IsExpression, SyntaxKindEx.IsPatternExpression)
                                              && !ifStatementSyntax.Condition.DescendantNodes()
                                                                             .Any(d => d.IsAnyKind(SyntaxKindEx.VarPattern,
                                                                                                   SyntaxKindEx.SingleVariableDesignation,
                                                                                                   SyntaxKindEx.ParenthesizedVariableDesignation));

            bool ConditionValidInvocation() => ifStatementSyntax.Condition is InvocationExpressionSyntax invocationExpressionSyntax
                                               && !invocationExpressionSyntax.DescendantNodes()
                                                                             .OfType<ArgumentSyntax>()
                                                                             .Any(argument => argument.RefOrOutKeyword.IsAnyKind(SyntaxKind.OutKeyword, SyntaxKind.RefKeyword));
        }

        /// <remarks>
        /// There are multiple scenarios where the code can be simplified using LINQ.
        /// For simplicity, we consider that Select() can be used
        /// only when a single property from the foreach variable is used.
        /// We skip checking method invocations since depending on the method being called, moving it can make the code harder to read.
        /// The issue is raised if:
        ///  - the property is used more than once
        ///  - the property is the right side of a variable declaration.
        /// </remarks>
        private static void CheckIfCanBeSimplifiedUsingSelect(SonarSyntaxNodeReportingContext c, ForEachStatementSyntax forEachStatementSyntax)
        {
            var declaredSymbol = new Lazy<ILocalSymbol>(() => c.SemanticModel.GetDeclaredSymbol(forEachStatementSyntax));
            var expressionTypeIsOrImplementsIEnumerable = new Lazy<bool>(
                () =>
                {
                    var expressionType = c.SemanticModel.GetTypeInfo(forEachStatementSyntax.Expression).Type;
                    return expressionType.Is(KnownType.System_Collections_Generic_IEnumerable_T)
                           || expressionType.Implements(KnownType.System_Collections_Generic_IEnumerable_T);
                });

            var accessedProperties = new Dictionary<ISymbol, UsageStats>();

            foreach (var identifierSyntax in GetStatementIdentifiers(forEachStatementSyntax))
            {
                if (identifierSyntax.Parent is MemberAccessExpressionSyntax { Parent: not InvocationExpressionSyntax } memberAccessExpressionSyntax
                    && IsNotLeftSideOfAssignment(memberAccessExpressionSyntax)
                    && expressionTypeIsOrImplementsIEnumerable.Value
                    && c.SemanticModel.GetSymbolInfo(identifierSyntax).Symbol.Equals(declaredSymbol.Value)
                    && c.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Name).Symbol is { } symbol)
                {
                    var usageStats = accessedProperties.GetOrAdd(symbol, _ => new UsageStats());

                    usageStats.IsInVarDeclarator = memberAccessExpressionSyntax.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax };
                    usageStats.Count++;
                }
                else
                {
                    return;
                }
            }

            if (accessedProperties.Count == 1
                && accessedProperties.First().Value is var stats
                && (stats.IsInVarDeclarator || stats.Count > 1))
            {
                var diagnostic = CreateDiagnostic(Rule,
                                                   forEachStatementSyntax.Expression.GetLocation(),
                                                   string.Format(SelectMessageFormat, forEachStatementSyntax.Identifier.ValueText, accessedProperties.Single().Key.Name));
                c.ReportIssue(diagnostic);
            }

            static IEnumerable<IdentifierNameSyntax> GetStatementIdentifiers(ForEachStatementSyntax forEachStatementSyntax) =>
                forEachStatementSyntax.Statement
                                      .DescendantNodes()
                                      .OfType<IdentifierNameSyntax>()
                                      .Where(identifierNameSyntax => identifierNameSyntax.Identifier.ValueText == forEachStatementSyntax.Identifier.ValueText);

            static bool IsNotLeftSideOfAssignment(MemberAccessExpressionSyntax memberAccess) =>
                !(memberAccess.Parent is AssignmentExpressionSyntax assignment && assignment.Left == memberAccess);
        }

        private sealed class UsageStats
        {
            public int Count { get; set; }

            public bool IsInVarDeclarator { get; set; }
        }
    }
}
