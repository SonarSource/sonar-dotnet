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

using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.TestFramework.SymbolicExecution
{
    /// <summary>
    /// This checks looks for specific tags in the source and collects them:
    /// tag = "TagName" - registers TagName, doesn't change the flow
    /// Tag("TagName") - can change flow, because invocations can throw exceptions in the engine
    /// Tag("TagName", variable) - can change flow, enables asserting on variable state
    /// </summary>
    internal class ValidatorTestCheck : SymbolicCheck
    {
        private readonly ControlFlowGraph cfg;
        private readonly List<SymbolicContext> postProcessed = new();
        private readonly List<(string Name, SymbolicContext Context)> tags = new();
        private readonly Dictionary<string, ISymbol> symbols = new();
        private int executionCompletedCount;

        public List<ProgramState> ExitStates { get; } = new();

        public ValidatorTestCheck(ControlFlowGraph cfg) =>
            this.cfg = cfg;

        public override void ExitReached(SymbolicContext context) =>
            ExitStates.Add(context.State);

        public override void ExecutionCompleted() =>
            executionCompletedCount++;

        public ISymbol Symbol(string name)
        {
            symbols.Should().ContainKey(name, "asserted symbol '{0}' should be in the compilation", name);
            return symbols[name];
        }

        public void ValidateOrder(params string[] expected) =>
            postProcessed.Select(x => TestHelper.Serialize(x.Operation)).Should().Equal(expected);

        public void ValidateTagOrder(params string[] expected) =>
            tags.Select(x => x.Name).Should().Equal(expected);

        public void Validate(string operation, Action<SymbolicContext> action) =>
            action(postProcessed.Single(x => TestHelper.Serialize(x.Operation) == operation));

        public ProgramState TagState(string tag) =>
            TagStates(tag).Should().ContainSingle().Subject;

        public ProgramState[] TagStates(string tag) =>
            tags.Where(x => x.Name == tag).Select(x => x.Context.State).ToArray();

        public SymbolicValue TagValue(string tag) =>
            TagValues(tag).Should().ContainSingle().Subject;

        public SymbolicValue TagValue(string tag, string symbol) =>
            TagState(tag)[Symbol(symbol)];

        public SymbolicValue[] TagValues(string tag) =>
            tags.Where(x => x.Name == tag).Select(x => TagValue(x.Context)).ToArray();

        public void ValidateExitReachCount(int expected) =>
            ExitStates.Should().HaveCount(expected);

        public void ValidateExecutionCompleted() =>
            executionCompletedCount.Should().Be(1);

        public void ValidateExecutionNotCompleted() =>
            executionCompletedCount.Should().Be(0);

        public void ValidatePostProcessCount(int expected) =>
            postProcessed.Should().HaveCount(expected);

        public void ValidateOperationValuesAreNull() =>
            postProcessed.Should().OnlyContain(x => x.State[x.Operation] == null);

        public void ValidateContainsOperation(OperationKind operationKind) =>
            cfg.Blocks.Any(x => x.OperationsAndBranchValue.ToExecutionOrder().Any(op => op.Instance.Kind == operationKind));

        public void ValidateHasSingleExitStateAndNoException() =>
            ExitStates.Should().ContainSingle().And.ContainSingle(x => x.Exception == null);

        protected override ProgramState PostProcessSimple(SymbolicContext context)
        {
            postProcessed.Add(context);
            if (context.Operation.Instance is IAssignmentOperation assignment && assignment.Target.TrackedSymbol(context.State) is { Name: "tag" or "Tag" })
            {
                AddTagName(assignment.Value.ConstantValue, context);
            }
            else if (context.Operation.Instance is IInvocationOperation invocation && invocation.TargetMethod.Name == "Tag")
            {
                (invocation.TargetMethod.IsStatic || invocation.Arguments.Length == 1).Should().BeTrue("Tag method should be static to not infer with object states.");
                AddTagName(invocation.Arguments.First(x => x.Parameter.Name.Equals("name", StringComparison.OrdinalIgnoreCase)).Value.ConstantValue, context);
            }
            if (context.Operation.Instance.TrackedSymbol(context.State) is { } symbol)
            {
                symbols[symbol.Name] = symbol;
            }
            return context.State;
        }

        private void AddTagName(Optional<object> tagName, SymbolicContext context)
        {
            tagName.HasValue.Should().BeTrue("tag should have literal name");
            tags.Add(((string)tagName.Value, context));
        }

        private static ISymbol Symbol(ProgramState state, IOperation operation) =>
            operation.TrackedSymbol(state) ?? operation switch
            {
                _ when IFieldReferenceOperationWrapper.IsInstance(operation) => IFieldReferenceOperationWrapper.FromOperation(operation).Member,
                _ when IPropertyReferenceOperationWrapper.IsInstance(operation) => IPropertyReferenceOperationWrapper.FromOperation(operation).Member,
                _ when IArrayElementReferenceOperationWrapper.IsInstance(operation) => IArrayElementReferenceOperationWrapper.FromOperation(operation).ArrayReference.TrackedSymbol(state),
                _ => null
            };

        private static SymbolicValue TagValue(SymbolicContext context)
        {
            var invocation = (IInvocationOperation)context.Operation.Instance;
            invocation.Arguments.Should().HaveCount(2, "Asserted argument is expected in Tag(..) invocation");
            var argument = invocation.Arguments[1].Value;
            var symbol = Symbol(context.State, argument is IConversionOperation conversion ? conversion.Operand : argument);
            symbol.Should().NotBeNull("Tag should have symbol specified");
            return context.State[symbol];
        }
    }
}
