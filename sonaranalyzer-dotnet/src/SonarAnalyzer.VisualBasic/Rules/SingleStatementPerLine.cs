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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Rules.Common;
using System.Linq;
using System;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public class SingleStatementPerLine : SingleStatementPerLineBase<StatementSyntax>
    {
        protected static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected override DiagnosticDescriptor Rule => rule;

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

            var multiline = st.Parent as MultiLineLambdaExpressionSyntax;
            if (multiline == null)
            {
                return false;
            }

            return multiline.Statements.Count == 1;
        }

        private static bool StatementIsBlock(StatementSyntax st) =>
            ExcludedTypes.Any(bType => bType.IsInstanceOfType(st));

        private static readonly Type[] ExcludedTypes =
        {
            typeof(NamespaceBlockSyntax),
            typeof(TypeBlockSyntax),
            typeof(EnumBlockSyntax),
            typeof(MethodBlockBaseSyntax),
            typeof(PropertyBlockSyntax),
            typeof(EventBlockSyntax),
            typeof(DoLoopBlockSyntax),
            typeof(WhileBlockSyntax),
            typeof(ForOrForEachBlockSyntax),
            typeof(MultiLineIfBlockSyntax),
            typeof(ElseStatementSyntax),
            typeof(SyncLockBlockSyntax),
            typeof(TryBlockSyntax),
            typeof(UsingBlockSyntax),
            typeof(WithBlockSyntax),
            typeof(MultiLineLambdaExpressionSyntax),
            typeof(MethodBaseSyntax),
            typeof(InheritsOrImplementsStatementSyntax)
        };

        protected sealed override GeneratedCodeRecognizer GeneratedCodeRecognizer => Helpers.VisualBasic.GeneratedCodeRecognizer.Instance;
    }
}
