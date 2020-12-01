/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MemberOverrideCallsBaseMember : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1185";
        private const string MessageFormat = "Remove this {1} '{0}' to simply inherit its behavior.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<string> IgnoredMethodNames = new HashSet<string> { "Equals", "GetHashCode" };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var method = (MethodDeclarationSyntax)c.Node;
                    if (IsMethodCandidate(method, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, method.GetLocation(), method.Identifier.ValueText, "method"));
                    }
                },
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var property = (PropertyDeclarationSyntax)c.Node;
                    if (IsPropertyCandidate(property, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, property.GetLocation(), property.Identifier.ValueText, "property"));
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

            if (propertySymbol == null ||
                !propertySymbol.IsOverride ||
                propertySymbol.IsSealed ||
                propertySymbol.OverriddenProperty == null)
            {
                return false;
            }

            if (propertySymbol.GetMethod != null && propertySymbol.OverriddenProperty.GetMethod == null)
            {
                return false;
            }

            if (propertySymbol.SetMethod != null && propertySymbol.OverriddenProperty.SetMethod == null)
            {
                return false;
            }

            if (SymbolHelper.IsAnyAttributeInOverridingChain(propertySymbol))
            {
                return false;
            }

            return CheckGetAccessorIfAny(propertySyntax, propertySymbol, semanticModel) &&
                CheckSetAccessorIfAny(propertySyntax, propertySymbol, semanticModel);
        }

        private static bool CheckGetAccessorIfAny(PropertyDeclarationSyntax propertySyntax, IPropertySymbol propertySymbol,
            SemanticModel semanticModel)
        {
            var getAccessor = propertySyntax.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));

            if (getAccessor == null &&
                propertySyntax.ExpressionBody == null)
            {
                // no getter
                return true;
            }

            var expression = propertySyntax.ExpressionBody?.Expression
                ?? getAccessor?.ExpressionBody()?.Expression
                ?? GetSingleStatementExpression(getAccessor?.Body, isVoid: false);

            if (!(expression is MemberAccessExpressionSyntax memberAccess) ||
                !(memberAccess.Expression is BaseExpressionSyntax))
            {
                return false;
            }

            return IsBaseProperty(propertySymbol, semanticModel, memberAccess);
        }

        private static bool IsBaseProperty(IPropertySymbol propertySymbol, SemanticModel semanticModel,
            MemberAccessExpressionSyntax memberAccess)
        {
            return semanticModel.GetSymbolInfo(memberAccess).Symbol is IPropertySymbol invokedPropertySymbol &&
                invokedPropertySymbol.Equals(propertySymbol.OverriddenProperty);
        }

        private static bool CheckSetAccessorIfAny(PropertyDeclarationSyntax propertySyntax, IPropertySymbol propertySymbol,
            SemanticModel semanticModel)
        {
            var setAccessor = propertySyntax.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
            if (setAccessor == null)
            {
                return true;
            }

            var expression = setAccessor?.ExpressionBody()?.Expression
                ?? GetSingleStatementExpression(setAccessor?.Body, isVoid: true);

            if (!(expression is AssignmentExpressionSyntax expressionToCheck) ||
                !expressionToCheck.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                return false;
            }

            //check right:
            if (!(semanticModel.GetSymbolInfo(expressionToCheck.Right).Symbol is IParameterSymbol valueParameter) || valueParameter.Name != "value" || !valueParameter.IsImplicitlyDeclared)
            {
                return false;
            }

            //check left:

            if (!(expressionToCheck.Left is MemberAccessExpressionSyntax memberAccess) ||
                !(memberAccess.Expression is BaseExpressionSyntax))
            {
                return false;
            }

            return IsBaseProperty(propertySymbol, semanticModel, memberAccess);
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

            var expression = methodSyntax.ExpressionBody?.Expression
                ?? GetSingleStatementExpression(methodSyntax.Body, isVoid: methodSymbol.ReturnsVoid);
            var invocationExpression = expression as InvocationExpressionSyntax;

            if (!(invocationExpression?.Expression is MemberAccessExpressionSyntax memberAccess) ||
                !(memberAccess.Expression is BaseExpressionSyntax))
            {
                return false;
            }

            if (!(semanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol invokedMethod) ||
                !invokedMethod.Equals(methodSymbol.OverriddenMethod))
            {
                return false;
            }

            return AreArgumentsMatchParameters(methodSymbol, semanticModel, invocationExpression, invokedMethod);
        }

        private static bool IsMethodSymbolExcluded(IMethodSymbol methodSymbol)
        {
            return
                methodSymbol == null ||
                !methodSymbol.IsOverride ||
                methodSymbol.IsSealed ||
                methodSymbol.OverriddenMethod == null ||
                IgnoredMethodNames.Contains(methodSymbol.Name) ||
                methodSymbol.Parameters.Any(p => p.HasExplicitDefaultValue) ||
                methodSymbol.OverriddenMethod.Parameters.Any(p => p.HasExplicitDefaultValue) ||
                SymbolHelper.IsAnyAttributeInOverridingChain(methodSymbol);
        }

        private static bool HasDocumentationComment(SyntaxNode node)
        {
            return node.GetLeadingTrivia()
                .Any(t =>
                    t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                    t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
        }

        private static bool AreArgumentsMatchParameters(IMethodSymbol methodSymbol, SemanticModel semanticModel,
            InvocationExpressionSyntax expressionToCheck, IMethodSymbol invokedMethod)
        {
            if (!invokedMethod.Parameters.Any())
            {
                return true;
            }

            if (expressionToCheck.ArgumentList == null ||
                invokedMethod.Parameters.Length != expressionToCheck.ArgumentList.Arguments.Count)
            {
                return false;
            }

            var argumentExpressions = expressionToCheck.ArgumentList.Arguments
                .Select(a => a.Expression as IdentifierNameSyntax)
                .ToList();

            if (argumentExpressions.Any(identifier => identifier == null))
            {
                return false;
            }

            for (var i = 0; i < argumentExpressions.Count; i++)
            {
                if (!(semanticModel.GetSymbolInfo(argumentExpressions[i]).Symbol is IParameterSymbol parameterSymbol) ||
                    !parameterSymbol.Equals(methodSymbol.Parameters[i]) ||
                    parameterSymbol.Name != methodSymbol.OverriddenMethod.Parameters[i].Name)
                {
                    return false;
                }
            }

            return true;
        }

        private static ExpressionSyntax GetSingleStatementExpression(BlockSyntax block, bool isVoid)
        {
            if (block == null ||
                block.Statements.Count != 1)
            {
                return null;
            }

            return isVoid
                ? (block.Statements[0] as ExpressionStatementSyntax)?.Expression
                : (block.Statements[0] as ReturnStatementSyntax)?.Expression;
        }
    }
}
