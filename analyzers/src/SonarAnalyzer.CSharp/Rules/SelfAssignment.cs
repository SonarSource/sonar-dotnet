/*
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
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SelfAssignment : SelfAssignmentBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var expression = (AssignmentExpressionSyntax)c.Node;

                    if (expression.Parent is InitializerExpressionSyntax)
                    {
                        return;
                    }

                    foreach (var assigment in expression.MapAssignmentArguments().Where(x => CSharpEquivalenceChecker.AreEquivalent(x.Left, x.Right)))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, assigment.Left.GetLocation(), additionalLocations: new[] { assigment.Right.GetLocation() }));
                    }
                },
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKindEx.CoalesceAssignmentExpression);
    }
}
