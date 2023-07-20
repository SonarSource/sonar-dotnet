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
    public sealed class MemberOverrideCallsBaseMember : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1185";
        private const string MessageFormat = "Remove this {1} '{0}' to simply inherit its behavior.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        private static readonly string[] IgnoredMethodNames = { "Equals", "GetHashCode" };
        private static readonly string[] IgnoredRecordMethodNames = { "ToString", "PrintMembers" };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var method = (MethodDeclarationSyntax)c.Node;
                    if (IsMethodCandidate(method, c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, method.GetLocation(), method.Identifier.ValueText, "method"));
                    }
                },
                SyntaxKind.MethodDeclaration);

            context.RegisterNodeAction(
                c =>
                {
                    var property = (PropertyDeclarationSyntax)c.Node;
                    if (IsPropertyCandidate(property, c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, property.GetLocation(), property.Identifier.ValueText, "property"));
                    }
                },
                SyntaxKind.PropertyDeclaration);
        }

        private static bool IsPropertyCandidate(PropertyDeclarationSyntax propertySyntax, SemanticModel semanticModel)
        {
            if (HasDocumentationComment(propertySyntax))
            {
                return false;
            }

            var propertySymbol = semanticModel.GetDeclaredSymbol(propertySyntax);
            if (propertySymbol == null
                || !propertySymbol.IsOverride
                || propertySymbol.IsSealed
                || propertySymbol.OverriddenProperty == null
                || (propertySymbol.GetMethod != null && propertySymbol.OverriddenProperty.GetMethod == null)
                || (propertySymbol.SetMethod != null && propertySymbol.OverriddenProperty.SetMethod == null)
                || SymbolHelper.IsAnyAttributeInOverridingChain(propertySymbol))
            {
                return false;
            }

            return CheckGetAccessorIfAny(propertySyntax, propertySymbol, semanticModel)
                   && CheckSetAccessorIfAny(propertySyntax, propertySymbol, semanticModel);
        }

        private static bool CheckGetAccessorIfAny(PropertyDeclarationSyntax propertySyntax, IPropertySymbol propertySymbol, SemanticModel semanticModel)
        {
            var getAccessor = propertySyntax.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
            if (getAccessor == null && propertySyntax.ExpressionBody == null)
            {
                // no getter
                return true;
            }

            var expression = propertySyntax.ExpressionBody?.Expression
                ?? getAccessor?.ExpressionBody()?.Expression
                ?? GetSingleStatementExpression(getAccessor?.Body, isVoid: false);

            return expression is MemberAccessExpressionSyntax memberAccess
                   && memberAccess.Expression is BaseExpressionSyntax
                   && IsBaseProperty(propertySymbol, semanticModel, memberAccess);
        }

        private static bool IsBaseProperty(IPropertySymbol propertySymbol, SemanticModel semanticModel, MemberAccessExpressionSyntax memberAccess) =>
            semanticModel.GetSymbolInfo(memberAccess).Symbol is IPropertySymbol invokedPropertySymbol
            && invokedPropertySymbol.Equals(propertySymbol.OverriddenProperty);

        private static bool CheckSetAccessorIfAny(PropertyDeclarationSyntax propertySyntax, IPropertySymbol propertySymbol, SemanticModel semanticModel)
        {
            var setAccessor = propertySyntax.AccessorList?.Accessors.FirstOrDefault(a => a.IsAnyKind(SyntaxKind.SetAccessorDeclaration, SyntaxKindEx.InitAccessorDeclaration));
            if (setAccessor == null)
            {
                return true;
            }

            var expression = setAccessor?.ExpressionBody()?.Expression ?? GetSingleStatementExpression(setAccessor?.Body, isVoid: true);

            return expression is AssignmentExpressionSyntax expressionToCheck
                   && expressionToCheck.IsKind(SyntaxKind.SimpleAssignmentExpression)
                   && expressionToCheck.Left is MemberAccessExpressionSyntax memberAccess
                   && memberAccess.Expression is BaseExpressionSyntax
                   && expressionToCheck.Right is IdentifierNameSyntax { Identifier: { ValueText: "value" } }
                   && semanticModel.GetSymbolInfo(expressionToCheck.Right).Symbol is IParameterSymbol { IsImplicitlyDeclared: true }
                   && IsBaseProperty(propertySymbol, semanticModel, memberAccess);
        }

        private static bool IsMethodCandidate(MethodDeclarationSyntax methodSyntax, SemanticModel semanticModel)
        {
            if (HasDocumentationComment(methodSyntax))
            {
                return false;
            }

            var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax);
            if (IsMethodSymbolExcluded(methodSymbol))
            {
                return false;
            }

            var expression = methodSyntax.ExpressionBody?.Expression ?? GetSingleStatementExpression(methodSyntax.Body, isVoid: methodSymbol.ReturnsVoid);
            var invocationExpression = expression as InvocationExpressionSyntax;

            return invocationExpression?.Expression is MemberAccessExpressionSyntax memberAccess
                   && memberAccess.Expression is BaseExpressionSyntax
                   && semanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol invokedMethod
                   && invokedMethod.Equals(methodSymbol.OverriddenMethod)
                   && AreArgumentsMatchParameters(methodSymbol, semanticModel, invocationExpression, invokedMethod);
        }

        private static bool IsMethodSymbolExcluded(IMethodSymbol methodSymbol) =>
            methodSymbol == null
            || !methodSymbol.IsOverride
            || methodSymbol.IsSealed
            || methodSymbol.OverriddenMethod == null
            || IgnoredMethodNames.Contains(methodSymbol.Name)
            || methodSymbol.Parameters.Any(p => p.HasExplicitDefaultValue)
            || methodSymbol.OverriddenMethod.Parameters.Any(p => p.HasExplicitDefaultValue)
            || SymbolHelper.IsAnyAttributeInOverridingChain(methodSymbol)
            || IsRecordCompilerGenerated(methodSymbol);

        private static bool IsRecordCompilerGenerated(IMethodSymbol methodSymbol) =>
            IgnoredRecordMethodNames.Contains(methodSymbol.Name)
            && methodSymbol.ContainingSymbol is ITypeSymbol type
            && type.IsRecord();

        private static bool HasDocumentationComment(SyntaxNode node) =>
            node.GetLeadingTrivia()
                .Any(t => t.IsAnyKind(SyntaxKind.SingleLineDocumentationCommentTrivia, SyntaxKind.MultiLineDocumentationCommentTrivia));

        private static bool AreArgumentsMatchParameters(IMethodSymbol methodSymbol, SemanticModel semanticModel, InvocationExpressionSyntax expressionToCheck, IMethodSymbol invokedMethod)
        {
            if (!invokedMethod.Parameters.Any())
            {
                return true;
            }

            if (expressionToCheck.ArgumentList == null || invokedMethod.Parameters.Length != expressionToCheck.ArgumentList.Arguments.Count)
            {
                return false;
            }

            var argumentExpressions = expressionToCheck.ArgumentList.Arguments.Select(a => a.Expression as IdentifierNameSyntax).ToList();
            for (var i = 0; i < argumentExpressions.Count; i++)
            {
                if (argumentExpressions[i] == null
                    || !(semanticModel.GetSymbolInfo(argumentExpressions[i]).Symbol is IParameterSymbol parameterSymbol)
                    || !parameterSymbol.Equals(methodSymbol.Parameters[i])
                    || parameterSymbol.Name != methodSymbol.OverriddenMethod.Parameters[i].Name)
                {
                    return false;
                }
            }

            return true;
        }

        private static ExpressionSyntax GetSingleStatementExpression(BlockSyntax block, bool isVoid)
        {
            if (block == null || block.Statements.Count != 1)
            {
                return null;
            }
            return isVoid
                ? (block.Statements[0] as ExpressionStatementSyntax)?.Expression
                : (block.Statements[0] as ReturnStatementSyntax)?.Expression;
        }
    }
}
