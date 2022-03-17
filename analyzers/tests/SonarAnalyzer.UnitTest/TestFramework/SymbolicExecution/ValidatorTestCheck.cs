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

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution
{
    internal class ValidatorTestCheck : SymbolicCheck
    {
        private readonly List<SymbolicContext> postProcessed = new();
        private readonly List<(string Name, SymbolicContext Context)> tags = new();
        private int exitReachedCount;
        private int executionCompletedCount;

        public override void ExitReached(SymbolicContext context) =>
            exitReachedCount++;

        public override void ExecutionCompleted() =>
            executionCompletedCount++;

        public void ValidateOrder(params string[] expected) =>
            postProcessed.Select(x => TestHelper.Serialize(x.Operation)).Should().OnlyContainInOrder(expected);

        public void ValidateTagOrder(params string[] expected) =>
            tags.Select(x => x.Name).Should().OnlyContainInOrder(expected);

        public void Validate(string operation, Action<SymbolicContext> action) =>
            action(postProcessed.Single(x => TestHelper.Serialize(x.Operation) == operation));

        public void ValidateTag(string tag, Action<SymbolicValue> action) =>
            action(TagValues(tag).Single());

        public ProgramState[] TagStates(string tag) =>
            tags.Where(x => x.Name == tag).Select(x => x.Context.State).ToArray();

        public SymbolicValue[] TagValues(string tag) =>
            tags.Where(x => x.Name == tag).Select(x => TagValue(x.Context)).ToArray();

        public void ValidateExitReachCount(int expected) =>
            exitReachedCount.Should().Be(expected);

        public void ValidateExecutionCompleted() =>
            executionCompletedCount.Should().Be(1);

        public void ValidateExecutionNotCompleted() =>
            executionCompletedCount.Should().Be(0);

        public void ValidatePostProcessCount(int expected) =>
            postProcessed.Should().HaveCount(expected);

        public void ValidateOperationValuesAreNull() =>
            postProcessed.Should().OnlyContain(x => x.State[x.Operation] == null);

        protected override ProgramState PostProcessSimple(SymbolicContext context)
        {
            postProcessed.Add(context);
            if (context.Operation.Instance is IInvocationOperation invocation && invocation.TargetMethod.Name == "Tag")
            {
                var tagName = invocation.Arguments.First().Value.ConstantValue;
                tagName.HasValue.Should().BeTrue("tag should have literal name");
                tags.Add(((string)tagName.Value, context));
            }
            return context.State;
        }

        private static ISymbol Symbol(IOperation operation) =>
            operation.TrackedSymbol() ?? operation switch
            {
                _ when IFieldReferenceOperationWrapper.IsInstance(operation) => IFieldReferenceOperationWrapper.FromOperation(operation).Member,
                _ when IPropertyReferenceOperationWrapper.IsInstance(operation) => IPropertyReferenceOperationWrapper.FromOperation(operation).Member,
                _ when IArrayElementReferenceOperationWrapper.IsInstance(operation) => IArrayElementReferenceOperationWrapper.FromOperation(operation).ArrayReference.TrackedSymbol(),
                _ => null
            };

        private static SymbolicValue TagValue(SymbolicContext context)
        {
            var invocation = (IInvocationOperation)context.Operation.Instance;
            invocation.Arguments.Should().HaveCount(2, "Asserted argument is expected in Tag(..) invocation");
            var symbol = Symbol(((IConversionOperation)invocation.Arguments[1].Value).Operand);
            return context.State[symbol];
        }
    }
}
