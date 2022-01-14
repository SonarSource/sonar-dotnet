﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ConditionalStructureSameCondition : ConditionalStructureSameConditionBase
    {
        protected override ILanguageFacade Language => CSharpFacade.Instance;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var ifStatement = (IfStatementSyntax)c.Node;
                    var currentCondition = ifStatement.Condition;
                    if (ifStatement.GetPrecedingConditionsInConditionChain().FirstOrDefault(x => CSharpEquivalenceChecker.AreEquivalent(currentCondition, x)) is { } precedingCondition)
                    {
                        c.ReportIssue(Diagnostic.Create(rule, currentCondition.GetLocation(), new[] { precedingCondition.GetLocation() }, precedingCondition.GetLineNumberToReport()));
                    }
                },
                SyntaxKind.IfStatement);
    }
}
