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

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks
{
    public abstract class NullPointerDereferenceBase : SymbolicRuleCheck
    {
        internal const string DiagnosticId = "S2259";

        protected virtual bool IsSupressed(SyntaxNode node) => true;

        protected override ProgramState PreProcessSimple(SymbolicContext context)
        {
            var reference = context.Operation.Instance.Kind switch
            {
                OperationKindEx.Invocation => context.Operation.Instance.ToInvocation().Instance,
                OperationKindEx.PropertyReference => context.Operation.Instance.ToPropertyReference().Instance,
                OperationKindEx.Await => context.Operation.Instance.ToAwait().Operation,
                OperationKindEx.ArrayElementReference => context.Operation.Instance.ToArrayElementReference().ArrayReference,
                _ => null,
            };
            if (reference != null
                && context.HasConstraint(reference, ObjectConstraint.Null)
                && !reference.Type.IsStruct() // ToDo: IsStruct() is a workaround before MMF-2401
                && !IsSupressed(reference.Syntax))
            {
                ReportIssue(reference, reference.Syntax.ToString());
            }

            return context.State;
        }
    }
}
