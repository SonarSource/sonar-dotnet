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
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution
{
    internal abstract class AssignmentRuleCheck : SymbolicRuleCheck
    {
        protected override ProgramState PostProcessSimple(SymbolicContext context)
        {
            if (context.Operation.Instance.Kind == OperationKind.SimpleAssignment)
            {
                ReportIssue(context.Operation);
            }
            return context.State;
        }

        public override bool ShouldExecute() =>
            true;
    }

    internal class MainScopeAssignmentRuleCheck : AssignmentRuleCheck
    {
        public static readonly DiagnosticDescriptor SMain = TestHelper.CreateDescriptor("SMain", DiagnosticDescriptorBuilder.MainSourceScopeTag);

        protected override DiagnosticDescriptor Rule => SMain;
    }

    internal class TestScopeAssignmentRuleCheck : AssignmentRuleCheck
    {
        public static readonly DiagnosticDescriptor STest = TestHelper.CreateDescriptor("STest", DiagnosticDescriptorBuilder.TestSourceScopeTag);

        protected override DiagnosticDescriptor Rule => STest;
    }

    internal class AllScopeAssignmentRuleCheck : AssignmentRuleCheck
    {
        public static readonly DiagnosticDescriptor SAll = TestHelper.CreateDescriptor("SAll", DiagnosticDescriptorBuilder.MainSourceScopeTag, DiagnosticDescriptorBuilder.TestSourceScopeTag);

        protected override DiagnosticDescriptor Rule => SAll;
    }

    internal class InvocationAssignmentRuleCheck : AssignmentRuleCheck
    {
        public static readonly DiagnosticDescriptor SInvocation = TestHelper.CreateDescriptor("SInvocation", DiagnosticDescriptorBuilder.MainSourceScopeTag);

        protected override DiagnosticDescriptor Rule => SInvocation;

        // This RuleCheck executes ONLY on methods that contains at least one invocation
        public override bool ShouldExecute() =>
            NodeContext.Node.DescendantNodes().Any(x => x.IsKind(SyntaxKind.InvocationExpression));
    }

    internal class ThrowAssignmentRuleCheck : AssignmentRuleCheck
    {
        public static readonly DiagnosticDescriptor SThrow = TestHelper.CreateDescriptor("SThrow", DiagnosticDescriptorBuilder.MainSourceScopeTag);

        protected override DiagnosticDescriptor Rule => SThrow;

        protected override ProgramState PostProcessSimple(SymbolicContext context) => throw new InvalidOperationException("This check is not useful.");
    }
}
