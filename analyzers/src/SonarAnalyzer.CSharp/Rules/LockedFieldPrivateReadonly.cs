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
public sealed class LockedFieldPrivateReadonly : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2445";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, "{0}");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var expression = ((LockStatementSyntax)c.Node).Expression;
                if (expression is ObjectCreationExpressionSyntax
                    or AnonymousObjectCreationExpressionSyntax
                    or ArrayCreationExpressionSyntax
                    or ImplicitArrayCreationExpressionSyntax
                    or QueryExpressionSyntax)
                {
                    c.ReportIssue(Diagnostic.Create(Rule, expression.GetLocation(), "Locking on a new instance is a no-op."));
                }
                else if (expression.IsAnyKind(SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, expression.GetLocation(), "Strings can be interned, and should not be used for locking."));
                }
                else if (expression is IdentifierNameSyntax
                    && c.SemanticModel.GetSymbolInfo(expression).Symbol is ILocalSymbol lockedSymbol)
                {
                    c.ReportIssue(Diagnostic.Create(Rule, expression.GetLocation(), $"'{lockedSymbol.Name}' is a local variable, and should not be used for locking."));
                }
                else if (expression is (IdentifierNameSyntax or MemberAccessExpressionSyntax)
                    && c.SemanticModel.GetSymbolInfo(expression).Symbol is IFieldSymbol lockedField
                    && (!lockedField.IsReadOnly || lockedField.GetEffectiveAccessibility() != Accessibility.Private))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, expression.GetLocation(), $"'{lockedField.Name}' is not 'private readonly', and should not be used for locking."));
                }
            },
            SyntaxKind.LockStatement);
}
