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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PropertyToAutoProperty : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2292";
    private const string MessageFormat = "Make this an auto-implemented property and remove its backing field.";
    private const int AccessorCount = 2;

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var propertyDeclaration = (PropertyDeclarationSyntax)c.Node;

                if (propertyDeclaration.AccessorList?.Accessors is { Count: AccessorCount } accessors
                    && !HasDifferentModifiers(accessors)
                    && !HasAttributes(accessors)
                    && !propertyDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                    && accessors.FirstOrDefault(x => x.IsKind(SyntaxKind.GetAccessorDeclaration)) is { } getter
                    && accessors.FirstOrDefault(x => x.Kind() is SyntaxKind.SetAccessorDeclaration or SyntaxKindEx.InitAccessorDeclaration) is { } setter
                    && FieldFromGetter(getter, c.Model) is { } getterField
                    && FieldFromSetter(setter, c.Model) is { } setterField
                    && getterField.Equals(setterField)
                    && !getterField.GetAttributes().Any()
                    && !getterField.IsVolatile
                    && c.Model.GetDeclaredSymbol(propertyDeclaration) is { } propertySymbol
                    && getterField.IsStatic == propertySymbol.IsStatic
                    && getterField.Type.Equals(propertySymbol.Type))
                {
                    c.ReportIssue(Rule, propertyDeclaration.Identifier);
                }
            },
            SyntaxKind.PropertyDeclaration);

    private static bool HasAttributes(SyntaxList<AccessorDeclarationSyntax> accessors) =>
        accessors.Any(x => x.AttributeLists.Any());

    private static bool HasDifferentModifiers(SyntaxList<AccessorDeclarationSyntax> accessors)
    {
        var modifiers = ModifierKinds(accessors.First()).ToHashSet();
        return accessors.Skip(1).Any(x => !modifiers.SetEquals(ModifierKinds(x)));
    }

    private static IEnumerable<SyntaxKind> ModifierKinds(AccessorDeclarationSyntax accessor) =>
        accessor.Modifiers.Select(x => x.Kind());

    private static IFieldSymbol FieldFromSetter(AccessorDeclarationSyntax setter, SemanticModel model)
    {
        var assignment = AssignmentFromBody(setter.Body) ?? AssignmentFromExpressionBody(setter.ExpressionBody());

        return assignment is { RawKind: (int)SyntaxKind.SimpleAssignmentExpression, Right: { } right }
            && model.GetSymbolInfo(right).Symbol is IParameterSymbol { Name: "value", IsImplicitlyDeclared: true }
                ? FieldSymbol(assignment.Left, model.GetDeclaredSymbol(setter).ContainingType, model)
                : null;

        AssignmentExpressionSyntax AssignmentFromBody(BlockSyntax body) =>
            body?.Statements.Count == 1 && body.Statements[0] is ExpressionStatementSyntax statement
                ? statement.Expression as AssignmentExpressionSyntax
                : null;

        AssignmentExpressionSyntax AssignmentFromExpressionBody(ArrowExpressionClauseSyntax expressionBody) =>
            expressionBody?.ChildNodes().Count() == 1
                ? expressionBody.ChildNodes().Single() as AssignmentExpressionSyntax
                : null;
    }

    private static IFieldSymbol FieldSymbol(ExpressionSyntax expression, INamedTypeSymbol declaringType, SemanticModel model)
    {
        if (expression is IdentifierNameSyntax)
        {
            return model.GetSymbolInfo(expression).Symbol as IFieldSymbol;
        }
        else if (expression is MemberAccessExpressionSyntax memberAccess && memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression))
        {
            return memberAccess.Expression is ThisExpressionSyntax
                || (memberAccess.Expression is IdentifierNameSyntax identifier && model.GetSymbolInfo(identifier).Symbol is INamedTypeSymbol type && type.Equals(declaringType))
                    ? model.GetSymbolInfo(expression).Symbol as IFieldSymbol
                    : null;
        }
        else
        {
            return null;
        }
    }

    private static IFieldSymbol FieldFromGetter(AccessorDeclarationSyntax getter, SemanticModel model)
    {
        var returnedExpression = GetReturnExpressionFromBody(getter.Body) ?? getter.ExpressionBody()?.Expression;

        return returnedExpression is null
            ? null
            : FieldSymbol(returnedExpression, model.GetDeclaredSymbol(getter).ContainingType, model);

        static ExpressionSyntax GetReturnExpressionFromBody(BlockSyntax body) =>
            body is not null && body.Statements.Count == 1 && body.Statements[0] is ReturnStatementSyntax returnStatement
                ? returnStatement.Expression
                : null;
    }
}
