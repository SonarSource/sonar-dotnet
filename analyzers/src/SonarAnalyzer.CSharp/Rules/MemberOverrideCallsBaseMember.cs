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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MemberOverrideCallsBaseMember : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S1185";
    private const string MessageFormat = "Remove this {1} '{0}' to simply inherit its behavior.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly string[] IgnoredMethodNames = ["Equals", "GetHashCode"];
    private static readonly string[] IgnoredRecordMethodNames = ["ToString", "PrintMembers"];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c =>
            {
                var method = (MethodDeclarationSyntax)c.Node;
                if (IsMethodCandidate(method, c.Model))
                {
                    c.ReportIssue(Rule, method, method.Identifier.ValueText, "method");
                }
            },
            SyntaxKind.MethodDeclaration);

        context.RegisterNodeAction(c =>
            {
                var property = (PropertyDeclarationSyntax)c.Node;
                if (IsPropertyCandidate(property, c.Model))
                {
                    c.ReportIssue(Rule, property, property.Identifier.ValueText, "property");
                }
            },
            SyntaxKind.PropertyDeclaration);
    }

    private static bool IsPropertyCandidate(PropertyDeclarationSyntax propertySyntax, SemanticModel model) =>
        !HasDocumentationComment(propertySyntax)
        && model.GetDeclaredSymbol(propertySyntax) is { IsOverride: true, IsSealed: false, OverriddenProperty: not null, IsAnyAttributeInOverridingChain: false } propertySymbol
        // Reject only if the override adds a getter the base property doesn't have - that's new behavior, not a pure forwarding call.
        && propertySymbol is not { GetMethod: not null, OverriddenProperty.GetMethod: null }
        && propertySymbol is not { SetMethod: not null, OverriddenProperty.SetMethod: null }
        && CheckGetAccessorIfAny(propertySyntax, propertySymbol, model)
        && CheckSetAccessorIfAny(propertySyntax, propertySymbol, model);

    private static bool CheckGetAccessorIfAny(PropertyDeclarationSyntax propertySyntax, IPropertySymbol propertySymbol, SemanticModel model)
    {
        var getAccessor = propertySyntax.AccessorList?.Accessors.FirstOrDefault(x => x.IsKind(SyntaxKind.GetAccessorDeclaration));
        if (getAccessor is null && propertySyntax.ExpressionBody is null)
        {
            // no getter
            return true;
        }
        else
        {
            var expression = propertySyntax.ExpressionBody?.Expression
                ?? getAccessor?.ExpressionBody?.Expression
                ?? SingleStatementExpression(getAccessor?.Body, isVoid: false);
            return expression is MemberAccessExpressionSyntax { Expression: BaseExpressionSyntax } memberAccess && IsBaseProperty(propertySymbol, model, memberAccess);
        }
    }

    private static bool IsBaseProperty(IPropertySymbol propertySymbol, SemanticModel model, MemberAccessExpressionSyntax memberAccess) =>
        model.GetSymbolInfo(memberAccess).Symbol is IPropertySymbol invokedPropertySymbol
        && invokedPropertySymbol.Equals(propertySymbol.OverriddenProperty);

    private static bool CheckSetAccessorIfAny(PropertyDeclarationSyntax propertySyntax, IPropertySymbol propertySymbol, SemanticModel model)
    {
        if (propertySyntax.AccessorList?.Accessors.FirstOrDefault(x => x.Kind() is SyntaxKind.SetAccessorDeclaration or SyntaxKindEx.InitAccessorDeclaration) is { } setAccessor)
        {
            var expression = setAccessor.ExpressionBody?.Expression ?? SingleStatementExpression(setAccessor.Body, isVoid: true);
            return expression is AssignmentExpressionSyntax { Left: MemberAccessExpressionSyntax { Expression: BaseExpressionSyntax } memberAccess } expressionToCheck
                && expressionToCheck.IsKind(SyntaxKind.SimpleAssignmentExpression)
                && expressionToCheck.Right is IdentifierNameSyntax { Identifier.ValueText: "value" }
                && model.GetSymbolInfo(expressionToCheck.Right).Symbol is IParameterSymbol { IsImplicitlyDeclared: true }
                && IsBaseProperty(propertySymbol, model, memberAccess);
        }
        else
        {
            return true;
        }
    }

    private static bool IsMethodCandidate(MethodDeclarationSyntax methodSyntax, SemanticModel model)
    {
        if (HasDocumentationComment(methodSyntax))
        {
            return false;
        }

        var methodSymbol = model.GetDeclaredSymbol(methodSyntax);
        if (IsMethodSymbolExcluded(methodSymbol))
        {
            return false;
        }

        var expression = methodSyntax.ExpressionBody?.Expression ?? SingleStatementExpression(methodSyntax.Body, isVoid: methodSymbol.ReturnsVoid);
        return expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: BaseExpressionSyntax } } invocationExpression
            && model.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol invokedMethod
            && invokedMethod.Equals(methodSymbol.OverriddenMethod)
            && AreArgumentsMatchingParameters(methodSymbol, model, invocationExpression, invokedMethod);
    }

    private static bool IsMethodSymbolExcluded(IMethodSymbol methodSymbol) =>
        methodSymbol is not { IsOverride: true, IsSealed: false, OverriddenMethod: not null, IsAnyAttributeInOverridingChain: false }
        || IgnoredMethodNames.Contains(methodSymbol.Name)
        || methodSymbol.Parameters.Any(x => x.HasExplicitDefaultValue)
        || methodSymbol.OverriddenMethod.Parameters.Any(x => x.HasExplicitDefaultValue)
        || IsRecordCompilerGenerated(methodSymbol);

    private static bool IsRecordCompilerGenerated(IMethodSymbol methodSymbol) =>
        IgnoredRecordMethodNames.Contains(methodSymbol.Name) && methodSymbol.ContainingSymbol is ITypeSymbol { IsRecord: true };

    private static bool HasDocumentationComment(SyntaxNode node) =>
        node.GetLeadingTrivia().Any(x => x.Kind() is SyntaxKind.SingleLineDocumentationCommentTrivia or SyntaxKind.MultiLineDocumentationCommentTrivia);

    private static bool AreArgumentsMatchingParameters(IMethodSymbol methodSymbol, SemanticModel model, InvocationExpressionSyntax expressionToCheck, IMethodSymbol invokedMethod)
    {
        if (!invokedMethod.Parameters.Any())
        {
            return true;
        }

        if (expressionToCheck.ArgumentList is null || invokedMethod.Parameters.Length != expressionToCheck.ArgumentList.Arguments.Count)
        {
            return false;
        }

        var argumentExpressions = expressionToCheck.ArgumentList.Arguments.Select(x => x.Expression as IdentifierNameSyntax).ToList();
        for (var i = 0; i < argumentExpressions.Count; i++)
        {
            if (argumentExpressions[i] is null || !IsMatch(i))
            {
                return false;
            }
        }
        return true;

        bool IsMatch(int index) =>
            model.GetSymbolInfo(argumentExpressions[index]).Symbol is IParameterSymbol parameterSymbol
            && parameterSymbol.Equals(methodSymbol.Parameters[index])
            && parameterSymbol.Name == methodSymbol.OverriddenMethod.Parameters[index].Name;
    }

    private static ExpressionSyntax SingleStatementExpression(BlockSyntax block, bool isVoid)
    {
        if (block is null || block.Statements.Count != 1)
        {
            return null;
        }
        else
        {
            return isVoid
                ? (block.Statements[0] as ExpressionStatementSyntax)?.Expression
                : (block.Statements[0] as ReturnStatementSyntax)?.Expression;
        }
    }
}
