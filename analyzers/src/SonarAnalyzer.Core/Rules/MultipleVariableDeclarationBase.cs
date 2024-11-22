/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules;

public static class MultipleVariableDeclarationConstants
{
    internal const string DiagnosticId = "S1659";
}

public abstract class MultipleVariableDeclarationBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    protected override string MessageFormat => "Declare '{0}' in a separate statement.";

    protected MultipleVariableDeclarationBase() : base(MultipleVariableDeclarationConstants.DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                CheckAndReportVariables(c, Rule, Language.Syntax.LocalDeclarationIdentifiers(c.Node));
            },
            Language.SyntaxKind.LocalDeclaration);

        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                CheckAndReportVariables(c, Rule, Language.Syntax.FieldDeclarationIdentifiers(c.Node));
            },
            Language.SyntaxKind.FieldDeclaration);
    }

    private static void CheckAndReportVariables(SonarSyntaxNodeReportingContext context, DiagnosticDescriptor rule, ICollection<SyntaxToken> variables)
    {
        if (variables.Count <= 1)
        {
            return;
        }
        foreach (var variable in variables.Skip(1))
        {
            context.ReportIssue(rule, variable, variable.ValueText);
        }
    }
}
