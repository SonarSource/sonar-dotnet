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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LockedFieldShouldBePrivateAndReadonly : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2445";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, "{0}");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(CheckLockStatement, SyntaxKind.LockStatement);

    private static void CheckLockStatement(SonarSyntaxNodeReportingContext context)
    {
        var expression = ((LockStatementSyntax)context.Node).Expression?.RemoveParentheses();
        if (IsCreation(expression))
        {
            ReportIssue("Locking on a new instance is a no-op.");
        }
        else
        {
            var symbol = context.SemanticModel.GetSymbolInfo(expression).Symbol;
            if (IsOfTypeString(context.SemanticModel, expression))
            {
                ReportIssue("Strings can be interned, and should not be used for locking.");
            }
            else if (expression is IdentifierNameSyntax && symbol is ILocalSymbol lockedSymbol)
            {
                ReportIssue($"'{lockedSymbol.Name}' is a local variable, and should not be used for locking.");
            }
            else if (FieldNotReadonlyOrNotPrivate(expression, symbol) is { } lockedField)
            {
                ReportIssue(FieldInSameTypeAs(lockedField, context.ContainingSymbol?.ContainingType) is { } containingType
                    ? $"Use members from '{containingType.ToMinimalDisplayString(context.SemanticModel, expression.SpanStart)}' for locking."
                    : $"'{lockedField.Name}' is not 'private readonly', and should not be used for locking.");
            }
        }

        void ReportIssue(string message) =>
            context.ReportIssue(Diagnostic.Create(Rule, expression.GetLocation(), message));
    }

    private static bool IsCreation(ExpressionSyntax expression) =>
        expression.IsAnyKind(
            SyntaxKind.ObjectCreationExpression,
            SyntaxKind.AnonymousObjectCreationExpression,
            SyntaxKind.ArrayCreationExpression,
            SyntaxKind.ImplicitArrayCreationExpression,
            SyntaxKind.QueryExpression);

    private static bool IsOfTypeString(SemanticModel model, ExpressionSyntax expression) =>
        expression.IsAnyKind(SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression)
            || expression.IsKnownType(KnownType.System_String, model);

    private static IFieldSymbol FieldNotReadonlyOrNotPrivate(ExpressionSyntax expression, ISymbol symbol) =>
        expression.IsAnyKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression)
            && symbol is IFieldSymbol lockedField
            && (!lockedField.IsReadOnly || lockedField.GetEffectiveAccessibility() != Accessibility.Private)
                ? lockedField
                : null;

    private static ITypeSymbol FieldInSameTypeAs(IFieldSymbol field, INamedTypeSymbol type) =>
        field.ContainingType is { } fieldType && type is { } && !fieldType.Equals(type)
            ? type
            : null;
}
