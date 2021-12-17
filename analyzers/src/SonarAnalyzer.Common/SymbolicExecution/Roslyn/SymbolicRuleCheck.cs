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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    public abstract class SymbolicRuleCheck : SymbolicCheck
    {
        protected SonarAnalysisContext SonarContext { get; private set; }
        protected SyntaxNodeAnalysisContext NodeContext { get; private set; }

        protected abstract DiagnosticDescriptor Rule { get; }

        /// <summary>
        /// Decide if a CFG should be created for current method and SE should be evaluated. We should only run SE for a method if there's a chance for finding something for performance reasons.
        /// </summary>
        /// <remarks>
        /// For example: It doesn't make sense to execute SE about handling disposing if there's no Dispose() invocation in the code.
        /// </remarks>
        public abstract bool ShouldExecute();

        public void Init(SonarAnalysisContext sonarContext, SyntaxNodeAnalysisContext nodeContext)
        {
            SonarContext = sonarContext;
            NodeContext = nodeContext;
        }

        protected void ReportIssue(IOperationWrapperSonar operationWrapperSonar)
        {
            var location = operationWrapperSonar.Instance.Syntax.GetLocation();
            NodeContext.ReportIssue(Diagnostic.Create(Rule, location));
        }
    }
}
