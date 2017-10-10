/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class FunctionNestingDepth : FunctionNestingDepthBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager)
                                       .DisabledByDefault();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private const int DefaultValueMaximum = 3;

        [RuleParameter("maximumNestingLevel", PropertyType.Integer,
            "Maximum allowed control flow statement nesting depth.", DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        private static readonly SyntaxKind[] FunctionKinds =
        {
            SyntaxKind.SubBlock,
            SyntaxKind.FunctionBlock,
            SyntaxKind.OperatorBlock,
            SyntaxKind.ConstructorBlock,
            SyntaxKind.GetAccessorBlock,
            SyntaxKind.SetAccessorBlock,
            SyntaxKind.AddHandlerAccessorBlock,
            SyntaxKind.RemoveHandlerAccessorBlock
        };

        protected override void Initialize(ParameterLoadingAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c => CheckFunctionNestingDepth(c), FunctionKinds);

        private void CheckFunctionNestingDepth(SyntaxNodeAnalysisContext context)
        {
            var walker = new NestingDepthWalker(Maximum, token => context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, token.GetLocation(), Maximum)));
            walker.Visit(context.Node);
        }

        private class NestingDepthWalker : VisualBasicSyntaxWalker
        {
            private readonly NestingDepthCounter counter;

            public NestingDepthWalker(int maximumNestingDepth, Action<SyntaxToken> actionMaximumExceeded)
            {
                counter = new NestingDepthCounter(maximumNestingDepth, actionMaximumExceeded);
            }

            public override void VisitMultiLineIfBlock(MultiLineIfBlockSyntax node) =>
                counter.CheckNesting(node.IfStatement.IfKeyword, () => base.VisitMultiLineIfBlock(node));

            public override void VisitForBlock(ForBlockSyntax node) =>
                counter.CheckNesting(node.ForStatement.ForKeyword, () => base.VisitForBlock(node));

            public override void VisitForEachBlock(ForEachBlockSyntax node) =>
                counter.CheckNesting(node.ForEachStatement.ForKeyword, () => base.VisitForEachBlock(node));

            public override void VisitWhileBlock(WhileBlockSyntax node) =>
                counter.CheckNesting(node.WhileStatement.WhileKeyword, () => base.VisitWhileBlock(node));

            public override void VisitDoLoopBlock(DoLoopBlockSyntax node) =>
                counter.CheckNesting(node.DoStatement.DoKeyword, () => base.VisitDoLoopBlock(node));

            public override void VisitSelectBlock(SelectBlockSyntax node) =>
                counter.CheckNesting(node.SelectStatement.SelectKeyword, () => base.VisitSelectBlock(node));

            public override void VisitTryBlock(TryBlockSyntax node) =>
                counter.CheckNesting(node.TryStatement.TryKeyword, () => base.VisitTryBlock(node));
        }
    }
}