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
public sealed class UseVar : StylingAnalyzer
{
    public UseVar() : base("T0045", "Use var.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                if (c.Node is VariableDeclarationSyntax { Type: not IdentifierNameSyntax { Identifier.ValueText: "var" }, Variables.Count: 1, Parent: LocalDeclarationStatementSyntax } declaration
                    && declaration.Variables[0] is { Initializer.Value: ImplicitObjectCreationExpressionSyntax or IdentifierNameSyntax or InvocationExpressionSyntax or MemberAccessExpressionSyntax })
                {
                    c.ReportIssue(Rule, declaration.Type);
                }
            },
            SyntaxKind.VariableDeclaration);
}
