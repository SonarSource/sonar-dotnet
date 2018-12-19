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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.Common;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class SingleStatementPerLine : SingleStatementPerLineBase<StatementSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override bool StatementShouldBeExcluded(StatementSyntax statement)
        {
            return //statement == null ||
                StatementIsBlock(statement) ||
                StatementIsSingleInLambda(statement);
        }

        private static bool StatementIsSingleInLambda(StatementSyntax st)
        {
            if (st.DescendantNodes()
                .OfType<StatementSyntax>()
                .Any())
            {
                return false;
            }

            if (!(st.Parent is MultiLineLambdaExpressionSyntax multiline))
            {
                return false;
            }

            return multiline.Statements.Count == 1;
        }

        private static bool StatementIsBlock(StatementSyntax st) =>
            st is NamespaceBlockSyntax ||
            st is TypeBlockSyntax ||
            st is EnumBlockSyntax ||
            st is MethodBlockBaseSyntax ||
            st is PropertyBlockSyntax ||
            st is EventBlockSyntax ||
            st is DoLoopBlockSyntax ||
            st is WhileBlockSyntax ||
            st is ForOrForEachBlockSyntax ||
            st is MultiLineIfBlockSyntax ||
            st is ElseStatementSyntax ||
            st is SyncLockBlockSyntax ||
            st is TryBlockSyntax ||
            st is UsingBlockSyntax ||
            st is WithBlockSyntax ||
            st is MethodBaseSyntax ||
            st is InheritsOrImplementsStatementSyntax;

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => Helpers.VisualBasic.VisualBasicGeneratedCodeRecognizer.Instance;
    }
}
