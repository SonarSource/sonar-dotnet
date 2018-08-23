/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class FunctionNestingDepth : FunctionNestingDepthBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);

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
            context.RegisterSyntaxNodeActionInNonGenerated(CheckFunctionNestingDepth, FunctionKinds);

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
                this.counter = new NestingDepthCounter(maximumNestingDepth, actionMaximumExceeded);
            }

            public override void VisitMultiLineIfBlock(MultiLineIfBlockSyntax node) =>
                this.counter.CheckNesting(node.IfStatement.IfKeyword, () => base.VisitMultiLineIfBlock(node));

            public override void VisitForBlock(ForBlockSyntax node) =>
                this.counter.CheckNesting(node.ForStatement.ForKeyword, () => base.VisitForBlock(node));

            public override void VisitForEachBlock(ForEachBlockSyntax node) =>
                this.counter.CheckNesting(node.ForEachStatement.ForKeyword, () => base.VisitForEachBlock(node));

            public override void VisitWhileBlock(WhileBlockSyntax node) =>
                this.counter.CheckNesting(node.WhileStatement.WhileKeyword, () => base.VisitWhileBlock(node));

            public override void VisitDoLoopBlock(DoLoopBlockSyntax node) =>
                this.counter.CheckNesting(node.DoStatement.DoKeyword, () => base.VisitDoLoopBlock(node));

            public override void VisitSelectBlock(SelectBlockSyntax node) =>
                this.counter.CheckNesting(node.SelectStatement.SelectKeyword, () => base.VisitSelectBlock(node));

            public override void VisitTryBlock(TryBlockSyntax node) =>
                this.counter.CheckNesting(node.TryStatement.TryKeyword, () => base.VisitTryBlock(node));
        }
    }
}
