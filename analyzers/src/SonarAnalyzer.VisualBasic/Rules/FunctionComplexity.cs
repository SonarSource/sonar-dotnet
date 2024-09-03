/*
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

using SonarAnalyzer.VisualBasic.Metrics;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class FunctionComplexity : FunctionComplexityBase
    {
        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterNodeAction(
                c => CheckComplexity<MethodBlockBaseSyntax>(c, m => m.BlockStatement.GetLocation(), "procedure"),
                SyntaxKind.SubBlock);

            context.RegisterNodeAction(
                c => CheckComplexity<MethodBlockBaseSyntax>(c, m => m.BlockStatement.GetLocation(), "function"),
                SyntaxKind.FunctionBlock);

            context.RegisterNodeAction(
                c => CheckComplexity<MethodBlockBaseSyntax>(c, m => m.BlockStatement.GetLocation(), "constructor"),
                SyntaxKind.ConstructorBlock);

            context.RegisterNodeAction(
                c => CheckComplexity<OperatorBlockSyntax>(c, m => m.OperatorStatement.GetLocation(), "operator"),
                SyntaxKind.OperatorBlock);

            context.RegisterNodeAction(
                c => CheckComplexity<AccessorBlockSyntax>(c, m => m.AccessorStatement.GetLocation(), "accessor"),
                SyntaxKind.GetAccessorBlock,
                SyntaxKind.SetAccessorBlock,
                SyntaxKind.AddHandlerAccessorBlock,
                SyntaxKind.RemoveHandlerAccessorBlock);
        }

        protected override int GetComplexity(SyntaxNode node, SemanticModel semanticModel) =>
            new VisualBasicMetrics(node.SyntaxTree, semanticModel).ComputeCyclomaticComplexity(node);

        protected sealed override GeneratedCodeRecognizer GeneratedCodeRecognizer => VisualBasicGeneratedCodeRecognizer.Instance;
    }
}
