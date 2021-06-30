/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotTestThisWithIsOperator : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3060";
        private const string MessageFormat = "Offload the code that's conditional on this 'is' test to the appropriate subclass and remove the test.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c => Analyze(c, x => ((BinaryExpressionSyntax)x).Left), SyntaxKind.IsExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(c => Analyze(c, x => ((SwitchStatementSyntax)x).Expression), SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeActionInNonGenerated(c => Analyze(c, x => ((IsPatternExpressionSyntaxWrapper)x).Expression), SyntaxKindEx.IsPatternExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(c => Analyze(c, x => ((SwitchExpressionSyntaxWrapper)x).GoverningExpression), SyntaxKindEx.SwitchExpression);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, Func<SyntaxNode, ExpressionSyntax> expression)
        {
            if (expression(context.Node).RemoveParentheses() is ThisExpressionSyntax)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }
    }
}
