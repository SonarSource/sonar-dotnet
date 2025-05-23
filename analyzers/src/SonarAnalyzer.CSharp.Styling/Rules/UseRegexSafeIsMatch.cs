﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp.Styling;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseRegexSafeIsMatch : StylingAnalyzer
{
    public UseRegexSafeIsMatch() : base("T0004", "Use '{0}{1}' instead.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var memberAccess = (MemberAccessExpressionSyntax)c.Node;
                if (memberAccess.NameIs("IsMatch", "Match", "Matches")
                    && c.SemanticModel.GetSymbolInfo(memberAccess).Symbol is IMethodSymbol method
                    && method.ContainingType.Is(KnownType.System_Text_RegularExpressions_Regex))
                {
                    c.ReportIssue(Rule, method.IsStatic ? memberAccess.Expression : memberAccess.Name, method.IsStatic ? "SafeRegex." : "Safe", method.Name);
                }
            },
            SyntaxKind.SimpleMemberAccessExpression);
}
