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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProtectedFieldsCase : StylingAnalyzer
{
    public ProtectedFieldsCase() : base("T0039", "Protected field should start with lower case letter.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var field = (FieldDeclarationSyntax)c.Node;
                if (field.Modifiers.Count == 2
                    && field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)
                    && field.Modifiers.Any(SyntaxKind.ProtectedKeyword))
                {
                    foreach (var variable in field.Declaration.Variables.Where(x => char.IsUpper(x.Identifier.ValueText[0])))
                    {
                        c.ReportIssue(Rule, variable.Identifier);
                    }
                }
            },
            SyntaxKind.FieldDeclaration);
}
