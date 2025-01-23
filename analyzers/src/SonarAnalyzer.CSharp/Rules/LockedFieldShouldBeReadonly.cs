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
            context.ReportIssue(LockedFieldRule, expression, "a new instance because is a no-op");
        }
        else
        {
            var lazySymbol = new Lazy<ISymbol>(() => context.Model.GetSymbolInfo(expression).Symbol);
            if (IsOfTypeString(expression, lazySymbol))
            {
                context.ReportIssue(LockedFieldRule, expression, "strings as they can be interned");
            }
            else if (expression is IdentifierNameSyntax && lazySymbol.Value is ILocalSymbol localSymbol)
            {
                context.ReportIssue(LocalVariableRule, expression, $"local variable '{localSymbol.Name}'");
            }
            else if (FieldWritable(expression, lazySymbol) is { } field)
            {
                context.ReportIssue(LockedFieldRule, expression, $"writable field '{field.Name}'");
            }
        }
    }

    private static bool IsCreation(ExpressionSyntax expression) =>
        expression?.Kind() is
            SyntaxKind.ObjectCreationExpression or
            SyntaxKind.AnonymousObjectCreationExpression or
            SyntaxKind.ArrayCreationExpression or
            SyntaxKind.ImplicitArrayCreationExpression or
            SyntaxKind.QueryExpression;

    private static bool IsOfTypeString(ExpressionSyntax expression, Lazy<ISymbol> lazySymbol) =>
        expression?.Kind() is SyntaxKind.StringLiteralExpression or SyntaxKind.InterpolatedStringExpression
        || lazySymbol.Value.GetSymbolType().Is(KnownType.System_String);

    private static IFieldSymbol FieldWritable(ExpressionSyntax expression, Lazy<ISymbol> lazySymbol) =>
        expression?.Kind() is SyntaxKind.IdentifierName or SyntaxKind.SimpleMemberAccessExpression
        && lazySymbol.Value is IFieldSymbol lockedField && !lockedField.IsReadOnly
            ? lockedField
            : null;
}
