/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodExpressionBodyLine : StylingAnalyzer
{
    public MethodExpressionBodyLine() : base("T0032", "Move this expression body to the next line.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var method = (BaseMethodDeclarationSyntax)c.Node;
                if (method.ExpressionBody?.Expression is { } expression
                    && method.GetLocation().StartLine() == expression.GetLocation().StartLine()
                    && expression.DescendantTokens().Skip(1).Any()
                    && !IsStubThrow((IMethodSymbol)c.ContainingSymbol, expression))
                {
                    c.ReportIssue(Rule, expression);
                }
            },
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.OperatorDeclaration);

    private static bool IsStubThrow(IMethodSymbol method, ExpressionSyntax expression) =>
        expression is ThrowExpressionSyntax
        {
            Expression: ObjectCreationExpressionSyntax { Type: IdentifierNameSyntax { Identifier.ValueText: nameof(NotImplementedException) or nameof(NotSupportedException) } }
        }
        && (method.IsOverride || method.ExplicitOrImplicitInterfaceImplementations().Any());
}
