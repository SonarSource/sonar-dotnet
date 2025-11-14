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
public class AllParametersOnSameColumn : AllParametersBase
{
    public AllParametersOnSameColumn() : base("T0022", "Parameters should start on the same column.") { }

    protected override void Verify(SonarSyntaxNodeReportingContext context, SyntaxNode[] parameters)
    {
        foreach (var parameter in parameters.Skip(1))
        {
            if (!parameters[0].HasSameStartLineAs(parameter) && !IsSameColumn(parameters[0], parameter))
            {
                context.ReportIssue(Rule, parameter);
            }
        }
    }

    private static bool IsSameColumn(SyntaxNode first, SyntaxNode second) =>
        first.GetLocation().StartColumn() == second.GetLocation().StartColumn();
}
