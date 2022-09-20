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

using FluentAssertions.Common;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution
{
    public static class SymbolicValueExtensions
    {
        public static SymbolicValueAssertions Should(this SymbolicValue instance) =>
            new(instance);
    }

    public class SymbolicValueAssertions : ReferenceTypeAssertions<SymbolicValue, SymbolicValueAssertions>
    {
        protected override string Identifier => "symbolicValue";

        public SymbolicValueAssertions(SymbolicValue subject) : base(subject) { }

        public AndWhichConstraint<SymbolicValueAssertions, SymbolicConstraint> HaveConstraint(SymbolicConstraint constraint, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(Subject != null)
                .FailWith("The {context:symbolicValue} is null and can not have constraint {0}.", constraint)
                .Then
                .Given(() => Subject.AllConstraints.ToList())
                .ForCondition(allConstraints => allConstraints.Count != 0)
                .FailWith("Expected {context:symbolicValue} to have constraint {0}{reason}, but symbolicValue has no constraints.", constraint)
                .Then
                .ForCondition(allConstraints => allConstraints.Contains(constraint))
                .FailWith("Expected {context:symbolicValue} to have constraint {0}{reason}, but symbolicValue has {1} constraints.",
                    _ => constraint,
                    x => x.OrderBy(x => x.ToString()));
            var matchedConstrainted = Subject.AllConstraints.First(x => x == constraint);
            return new(this, matchedConstrainted);
        }

        public AndWhichConstraint<SymbolicValueAssertions, SymbolicConstraint> HaveOnlyConstraint(SymbolicConstraint constraint, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(Subject != null)
                .FailWith("The {context:symbolicValue} is null and can not have constraint {0}.", constraint)
                .Then
                .Given(() => Subject.AllConstraints.ToList())
                .ForCondition(allConstraints => allConstraints.Count > 0)
                .FailWith("Expected {context:symbolicValue} to have constraint {0}{reason}, but symbolicValue has no constraints.", constraint)
                .Then
                .ForCondition(allConstraints => allConstraints.Count == 1)
                .FailWith("Expected {context:symbolicValue} to have only constraint {0}{reason}, but symbolicValue has {1} constraints.",
                    _ => constraint,
                    x => x.OrderBy(x => x.ToString()))
                .Then
                .ForCondition(allConstraints => allConstraints[0] == constraint)
                .FailWith("Expected {context:symbolicValue} to have constraint {0}{reason}, but symbolicValue has {1} constraint.",
                    _ => constraint,
                    allConstraints => allConstraints[0]);
            var matchedConstrainted = Subject.AllConstraints.First(x => x == constraint);
            return new(this, matchedConstrainted);
        }

        public AndConstraint<SymbolicValueAssertions> HaveNoConstraints(string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(Subject == null || Subject.AllConstraints.All(x => x == null))
                .FailWith("Expected {context:symbolicValue} to have no constraints{reason}, but {0} constraints were found.", Subject?.AllConstraints.OrderBy(x => x.ToString()));
            return new(this);
        }

        public AndConstraint<SymbolicValueAssertions> HaveConstraints(params SymbolicConstraint[] constraints)
            => HaveConstraints(constraints, string.Empty);

        public AndConstraint<SymbolicValueAssertions> HaveConstraints(IEnumerable<SymbolicConstraint> constraints, string because = "", params object[] becauseArgs)
        {
            HaveConstraintsCommonAssertions(constraints, because, becauseArgs)
                .Given(allConstraints => constraints.Except(allConstraints).OrderBy(x => x.ToString()).ToList())
                .ForCondition(missingConstraints => missingConstraints.Count == 0)
                .FailWith("Expected {context:symbolicValue} to have constraints {0}{reason}, but constraints {1} are missing. Actual constraints {2}.",
                    _ => constraints,
                    x => x,
                    _ => Subject.AllConstraints.OrderBy(x => x.ToString()));
            return new(this);
        }

        public AndConstraint<SymbolicValueAssertions> HaveOnlyConstraints(params SymbolicConstraint[] constraints)
            => HaveOnlyConstraints(constraints, string.Empty);

        public AndConstraint<SymbolicValueAssertions> HaveOnlyConstraints(IEnumerable<SymbolicConstraint> constraints, string because = "", params object[] becauseArgs)
        {
            HaveConstraintsCommonAssertions(constraints, because, becauseArgs)
                .Given(allConstraints => new
                {
                    missingConstraints = constraints.Except(allConstraints).OrderBy(x => x.ToString()).ToList(),
                    addionalConstraints = allConstraints.Except(constraints).OrderBy(x => x.ToString()).ToList()
                })
                .ForCondition(missingAndAdditional => missingAndAdditional is { missingConstraints.Count: 0 } or { addionalConstraints.Count: > 0 })
                .FailWith("Expected {context:symbolicValue} to only have constraints {0}{reason}, but constraints {1} are missing. Actual constraints {2}.",
                    _ => constraints,
                    missingAndAdditional => missingAndAdditional.missingConstraints,
                    _ => Subject.AllConstraints.OrderBy(x => x.ToString()))
                .Then
                .ForCondition(missingAndAdditional => missingAndAdditional is { missingConstraints.Count: > 0 } or { addionalConstraints.Count: 0 })
                .FailWith("Expected {context:symbolicValue} to only have constraints {0}{reason}, but {1} additional constraints are present. Actual constraints {2}.",
                    _ => constraints,
                    missingAndAdditional => missingAndAdditional.addionalConstraints,
                    _ => Subject.AllConstraints.OrderBy(x => x.ToString()))
                .Then
                .ForCondition(missingAndAdditional => missingAndAdditional is { missingConstraints.Count: 0, addionalConstraints.Count: 0 })
                .FailWith("Expected {context:symbolicValue} to have constraints {0}{reason}, but constraints {1} are missing and additional constraints {2} are present. Actual constraints {3}.",
                    _ => constraints,
                    missingAndAdditional => missingAndAdditional.missingConstraints,
                    missingAndAdditional => missingAndAdditional.addionalConstraints,
                    _ => Subject.AllConstraints.OrderBy(x => x.ToString()));
            return new(this);
        }

        /// <inheritdoc cref="ObjectAssertions.Be(object, string, object[])"/>
        public AndConstraint<SymbolicValueAssertions> Be(object expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(Subject.IsSameOrEqualTo(expected))
                .FailWith("Expected {context:object} to be {0}{reason}, but found {1}.", expected, Subject);
            return new AndConstraint<SymbolicValueAssertions>(this);
        }

        private GivenSelector<List<SymbolicConstraint>> HaveConstraintsCommonAssertions(IEnumerable<SymbolicConstraint> constraints, string because, object[] becauseArgs) =>
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(constraints != null && constraints.Any())
                .FailWith("Expected constraints is empty. Use HaveNoConstraints() instead.")
                .Then
                .ForCondition(Subject != null)
                .FailWith("The {context:symbolicValue} is null and can not have constraints {0}.", constraints)
                .Then
                .Given(() => Subject.AllConstraints.ToList())
                .ForCondition(allConstraints => allConstraints.Count != 0)
                .FailWith("Expected {context:symbolicValue} to have constraints {0}{reason}, but symbolicValue has no constraints.", constraints)
                .Then;
    }
}
