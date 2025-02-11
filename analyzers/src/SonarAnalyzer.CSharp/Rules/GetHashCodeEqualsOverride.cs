/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
    public sealed class GetHashCodeEqualsOverride : SonarDiagnosticAnalyzer
    {
        internal const string EqualsName = "Equals";

        private const string DiagnosticId = "S3249";
        private const string MessageFormat = "Remove this 'base' call to 'object.{0}', which is directly based on the object reference.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private static readonly ISet<string> MethodNames = new HashSet<string> { "GetHashCode", EqualsName };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCodeBlockStartAction(
                cb =>
                {
                    if (!(cb.CodeBlock is MethodDeclarationSyntax methodDeclaration))
                    {
                        return;
                    }

                    if (methodDeclaration.AttributeLists.Any()
                        || !(cb.OwningSymbol is IMethodSymbol methodSymbol)
                        || !MethodIsRelevant(methodSymbol, MethodNames)
                        // this rule should not apply for records since Equals and GetHashCode are value-based
                        || RecordDeclarationSyntaxWrapper.IsInstance(methodDeclaration.Ancestors().FirstOrDefault(node => node is TypeDeclarationSyntax)))
                    {
                        return;
                    }

                    var locations = new List<Location>();

                    cb.RegisterNodeAction(
                        c =>
                        {
                            if (TryGetLocationFromInvocationInsideMethod(c, methodSymbol, out var location))
                            {
                                locations.Add(location);
                            }
                        },
                        SyntaxKind.InvocationExpression);

                    cb.RegisterCodeBlockEndAction(
                        c =>
                        {
                            if (!locations.Any())
                            {
                                return;
                            }

                            var firstPosition = locations.Select(loc => loc.SourceSpan.Start).Min();
                            var location = locations.First(loc => loc.SourceSpan.Start == firstPosition);
                            c.ReportIssue(Rule, location, methodSymbol.Name);
                        });
                });

        internal static bool IsEqualsCallInGuardCondition(InvocationExpressionSyntax invocation, IMethodSymbol invokedMethod) =>
            invokedMethod.Name == EqualsName
            && invocation.Parent is IfStatementSyntax ifStatement
            && ifStatement.Condition == invocation
            && IfStatementWithSingleReturnTrue(ifStatement);

        internal static bool MethodIsRelevant(ISymbol symbol, ISet<string> methodNames) =>
            methodNames.Contains(symbol.Name) && symbol.IsOverride;

        private static bool TryGetLocationFromInvocationInsideMethod(SonarSyntaxNodeReportingContext context, ISymbol symbol, out Location location)
        {
            location = null;
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!(context.Model.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedMethod)
                || invokedMethod.Name != symbol.Name
                || !invocation.IsOnBase())
            {
                return false;
            }

            if (invokedMethod.IsInType(KnownType.System_Object)
                && !IsEqualsCallInGuardCondition(invocation, invokedMethod))
            {
                location = invocation.GetLocation();
                return true;
            }

            return false;
        }

        private static bool IfStatementWithSingleReturnTrue(IfStatementSyntax ifStatement)
        {
            var statement = ifStatement.Statement;
            var returnStatement = statement as ReturnStatementSyntax;
            if (statement is BlockSyntax block)
            {
                if (!block.Statements.Any())
                {
                    return false;
                }

                returnStatement = block.Statements.First() as ReturnStatementSyntax;
            }

            return returnStatement?.Expression != null
                   && CSharpEquivalenceChecker.AreEquivalent(returnStatement.Expression, SyntaxConstants.TrueLiteralExpression);
        }
    }
}
