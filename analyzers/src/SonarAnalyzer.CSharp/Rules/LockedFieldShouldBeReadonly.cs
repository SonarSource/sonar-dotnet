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
public sealed class LockedFieldShouldBeReadonly : SonarDiagnosticAnalyzer
{
    private const string LockedFieldDiagnosticId = "S2445";
    private const string LocalVariableDiagnosticId = "S6507";
    private const string MessageFormat = "Do not lock on {0}, use a readonly field instead.";

    private static readonly DiagnosticDescriptor LockedFieldRule = DescriptorFactory.Create(LockedFieldDiagnosticId, MessageFormat);
    private static readonly DiagnosticDescriptor LocalVariableRule = DescriptorFactory.Create(LocalVariableDiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(LockedFieldRule, LocalVariableRule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(CheckLockStatement, SyntaxKind.LockStatement);

    private static void CheckLockStatement(SonarSyntaxNodeReportingContext context)
    {
        var expression = ((LockStatementSyntax)context.Node).Expression?.RemoveParentheses();
        if (IsCreation(expression))
        {
            context.ReportIssue(CreateDiagnostic(LockedFieldRule, expression.GetLocation(), "a new instance because is a no-op"));
        }
        else
        {
            var lazySymbol = new Lazy<ISymbol>(() => context.SemanticModel.GetSymbolInfo(expression).Symbol);
            if (IsOfTypeString(expression, lazySymbol))
            {
                context.ReportIssue(CreateDiagnostic(LockedFieldRule, expression.GetLocation(), "strings as they can be interned"));
            }
            else if (expression is IdentifierNameSyntax && lazySymbol.Value is ILocalSymbol localSymbol)
            {
                context.ReportIssue(CreateDiagnostic(LocalVariableRule, expression.GetLocation(), $"local variable '{localSymbol.Name}'"));
            }
            else if (FieldWritable(expression, lazySymbol) is { } field)
            {
                context.ReportIssue(CreateDiagnostic(LockedFieldRule, expression.GetLocation(), $"writable field '{field.Name}'"));
            }
        }
    }

    private static bool IsCreation(ExpressionSyntax expression) =>
        expression.IsAnyKind(
            SyntaxKind.ObjectCreationExpression,
            SyntaxKind.AnonymousObjectCreationExpression,
            SyntaxKind.ArrayCreationExpression,
            SyntaxKind.ImplicitArrayCreationExpression,
            SyntaxKind.QueryExpression);

    private static bool IsOfTypeString(ExpressionSyntax expression, Lazy<ISymbol> lazySymbol) =>
        expression.IsAnyKind(SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression)
        || lazySymbol.Value.GetSymbolType().Is(KnownType.System_String);

    private static IFieldSymbol FieldWritable(ExpressionSyntax expression, Lazy<ISymbol> lazySymbol) =>
        expression.IsAnyKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression) && lazySymbol.Value is IFieldSymbol lockedField && !lockedField.IsReadOnly
            ? lockedField
            : null;
}
