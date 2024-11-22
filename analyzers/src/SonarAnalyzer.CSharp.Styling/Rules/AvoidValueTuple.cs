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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidValueTuple : StylingAnalyzer
{
    public AvoidValueTuple() : base("T0003", "Do not use ValueTuple in the production code due to missing System.ValueTuple.dll.", SourceScope.Main) { }    // It's not a problem in UTs

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c => c.ReportIssue(Rule, c.Node), SyntaxKind.TupleType);
        context.RegisterNodeAction(c =>
        {
            if (!c.Node.Parent.IsKind(SyntaxKind.SwitchExpression))
            {
                c.ReportIssue(Rule, c.Node);
            }
        }, SyntaxKind.TupleExpression);
    }
}
