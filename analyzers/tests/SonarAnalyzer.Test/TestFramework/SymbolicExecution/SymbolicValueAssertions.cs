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

using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.Test.TestFramework.SymbolicExecution
{
    public static class SymbolicValueExtensions
    {
        public static SymbolicValueAssertions Should(this SymbolicValue instance) =>
            new(instance);
    }

    public class SymbolicValueAssertions : ReferenceTypeAssertions<SymbolicValue, SymbolicValueAssertions>
    {
        protected override string Identifier => "SymbolicValue";

        public SymbolicValueAssertions(SymbolicValue subject) : base(subject) { }

        [CustomAssertion]
        public AndWhichConstraint<SymbolicValueAssertions, SymbolicConstraint> HaveOnlyConstraint(SymbolicConstraint expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(Subject is not null)
                .FailWith("{context:SymbolicValue} is <null> and can not have constraint {0}.", expected)
                .Then
                .Given(() => Subject.AllConstraints.ToList())
                .ForCondition(actuals => actuals.Count > 0)
                .FailWith("Expected {context:SymbolicValue} to have constraint {0}{reason}, but SymbolicValue has no constraints.", expected)
                .Then
                .ForCondition(actuals => actuals.Count == 1)
                .FailWith("Expected {context:SymbolicValue} to have only constraint {0}{reason}, but SymbolicValue has {1} constraints.", _ => expected, x => x.OrderBy(x => x.ToString()))
                .Then
                .Given(actuals => actuals[0])
                .ForCondition(actual => actual == expected)
                .FailWith("Expected {context:SymbolicValue} to have constraint {0}{reason}, but SymbolicValue has {1} constraint.", _ => expected, actual => actual);
            return new(this, Subject.AllConstraints.Single());
        }

        [CustomAssertion]
        public AndConstraint<SymbolicValueAssertions> HaveNoConstraints(string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(Subject is null || Subject.AllConstraints.Count() == 0)
                .FailWith("Expected {context:SymbolicValue} to have no constraints{reason}, but {0} was found.", Subject?.AllConstraints.OrderBy(x => x.ToString()));
            return new(this);
        }

        [CustomAssertion]
        public AndConstraint<SymbolicValueAssertions> HaveOnlyConstraints(params SymbolicConstraint[] expected) =>
            HaveOnlyConstraints(expected, string.Empty);

        [CustomAssertion]
        public AndConstraint<SymbolicValueAssertions> HaveOnlyConstraints(IEnumerable<SymbolicConstraint> expected, string because = "", params object[] becauseArgs)
        {
            expected = expected?.WhereNotNull();
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(expected is not null && expected.Any())
                .FailWith("Expected constraints are empty. Use HaveNoConstraints() instead.")
                .Then
                .ForCondition(Subject is not null)
                .FailWith("{context:SymbolicValue} is <null> and can not have constraints {0}.", expected)
                .Then
                .Given(() => Subject.AllConstraints.OrderBy(x => x.ToString()).ToList())
                .ForCondition(actual => actual.Count != 0)
                .FailWith("Expected {context:SymbolicValue} to have constraints {0}{reason}, but SymbolicValue has no constraints.", expected)
                .Then
                .Given(actual => new
                {
                    Missing = expected.Except(actual).OrderBy(x => x.ToString()).ToList(),
                    Additional = actual.Except(expected).OrderBy(x => x.ToString()).ToList(),
                    Actual = actual,
                })
                .ForCondition(given => given is { Missing.Count: 0 } or { Additional.Count: > 0 })
                .FailWith("Expected {context:SymbolicValue} to have constraints {0}{reason}, but {1} are missing. Actual are {2}.",
                    _ => expected,
                    given => given.Missing,
                    given => given.Actual)
                .Then
                .ForCondition(given => given is { Missing.Count: > 0 } or { Additional.Count: 0 })
                .FailWith("Expected {context:SymbolicValue} to have constraints {0}{reason}, but additional {1} are present. Actual are {2}.",
                    _ => expected,
                    given => given.Additional,
                    given => given.Actual)
                .Then
                .ForCondition(given => given is { Missing.Count: 0, Additional.Count: 0 })
                .FailWith("Expected {context:SymbolicValue} to have constraints {0}{reason}, but {1} are missing and additional {2} are present. Actual are {3}.",
                    _ => expected,
                    given => given.Missing,
                    given => given.Additional,
                    given => given.Actual);
            return new(this);
        }

        /// <inheritdoc cref="ObjectAssertions.Be(object, string, object[])"/>
        [CustomAssertion]
        public AndConstraint<SymbolicValueAssertions> Be(object expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(EqualityComparer<object>.Default.Equals(Subject, expected))
                .FailWith("Expected {context:SymbolicValue} to be {0}{reason}, but found {1}.", expected, Subject);
            return new(this);
        }
    }
}
