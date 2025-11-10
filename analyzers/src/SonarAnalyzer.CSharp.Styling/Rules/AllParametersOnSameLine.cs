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
public sealed class AllParametersOnSameLine : AllParametersBase
{
    public AllParametersOnSameLine() : base("T0023", "Parameters should be on the same line or all on separate lines.") { }

    protected override void Verify(SonarSyntaxNodeReportingContext context, SyntaxNode[] parameters)
    {
        if (parameters.Length < 3)
        {
            return;
        }

        var isSameLine = parameters[0].HasSameStartLineAs(parameters[1]);
        for (var i = 2; i < parameters.Length; i++)
        {
            if (isSameLine != parameters[i - 1].HasSameStartLineAs(parameters[i]))
            {
                context.ReportIssue(Rule, parameters[i]);
                return;
            }
        }
    }
}
