/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class IfConditionalAlwaysTrueOrFalseBase<TIfSyntax> : SonarDiagnosticAnalyzer
        where TIfSyntax : SyntaxNode
    {
        internal const string DiagnosticId = "S1145";
        protected const string MessageFormat = "Remove this useless {0}.";

        protected abstract void ReportIfTrue(TIfSyntax ifSyntax, SyntaxNodeAnalysisContext context);

        protected abstract void ReportIfFalse(TIfSyntax ifSyntax, SyntaxNodeAnalysisContext context);

        protected abstract bool ConditionIsTrueLiteral(TIfSyntax ifSyntax);

        protected abstract bool ConditionIsFalseLiteral(TIfSyntax ifSyntax);

        protected void TreatNode(SyntaxNodeAnalysisContext context)
        {
            var ifNode = (TIfSyntax)context.Node;

            var isTrue = ConditionIsTrueLiteral(ifNode);
            var isFalse = ConditionIsFalseLiteral(ifNode);

            if (!isTrue && !isFalse)
            {
                return;
            }

            if (isTrue)
            {
                ReportIfTrue(ifNode, context);
            }
            else
            {
                ReportIfFalse(ifNode, context);
            }
        }
    }
}
