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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution
{
    internal abstract class AssignmentRuleCheck : SymbolicRuleCheck
    {
        protected abstract DiagnosticDescriptor Rule { get; }

        public override ProgramState PostProcess(SymbolicContext context)
        {
            if (context.Operation.Instance.Kind == OperationKind.SimpleAssignment)
            {
                NodeContext.ReportIssue(Diagnostic.Create(Rule, context.Operation.Instance.Syntax.GetLocation()), SonarContext);
            }
            return context.State;
        }

        public override bool ShouldExecute() =>
            true;

        protected static DiagnosticDescriptor CreateDescriptor(string id, params string[] customTags) =>
            new(id, "Title", "Message for " + id, "Category", DiagnosticSeverity.Warning, true, customTags: customTags);
    }

    internal class MainScopeAssignmentRuleCheck : AssignmentRuleCheck
    {
        public static readonly DiagnosticDescriptor SMain = CreateDescriptor("SMain", DiagnosticDescriptorBuilder.MainSourceScopeTag);

        protected override DiagnosticDescriptor Rule { get; } = SMain;
    }

    internal class TestScopeAssignmentRuleCheck : AssignmentRuleCheck
    {
        public static readonly DiagnosticDescriptor STest = CreateDescriptor("STest", DiagnosticDescriptorBuilder.TestSourceScopeTag);

        protected override DiagnosticDescriptor Rule { get; } = STest;
    }

    internal class AllScopeAssignmentRuleCheck : AssignmentRuleCheck
    {
        public static readonly DiagnosticDescriptor SAll = CreateDescriptor("SAll", DiagnosticDescriptorBuilder.MainSourceScopeTag, DiagnosticDescriptorBuilder.TestSourceScopeTag);

        protected override DiagnosticDescriptor Rule { get; } = SAll;
    }

    internal class InvocationAssignmentRuleCheck : AssignmentRuleCheck
    {
        public static readonly DiagnosticDescriptor SInvocation = CreateDescriptor("SInvocation", DiagnosticDescriptorBuilder.MainSourceScopeTag);

        protected override DiagnosticDescriptor Rule { get; } = SInvocation;

        // This RuleCheck executes ONLY on methods that contains at least one invocation
        public override bool ShouldExecute() =>
            NodeContext.Node.DescendantNodes().Any(x => x.IsKind(SyntaxKind.InvocationExpression));
    }
}
