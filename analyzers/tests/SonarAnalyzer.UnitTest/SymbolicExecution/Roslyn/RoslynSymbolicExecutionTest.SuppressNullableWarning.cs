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

using SonarAnalyzer.Common;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.SymbolicExecution.Roslyn.Checks;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void NullForgivingOperatorSetsNotNullConstraints()
        {
            var validator = new SETestContext(@"
#nullable enable
public class C {
    public void M() {
        object? o = null;
        o!.ToString();
    }
}
", AnalyzerLanguage.CSharp, new SymbolicCheck[] { new SuppressNullableWarningCheck() }).Validator;
            validator.ValidateOrder(
                "LocalReference: o = null (Implicit)",
                "Literal: null",
                "Conversion: null (Implicit)",
                "SimpleAssignment: o = null (Implicit)",
                "LocalReference: o",
                "Invocation: o!.ToString()",
                "ExpressionStatement: o!.ToString();");
            validator.Validate("SimpleAssignment: o = null (Implicit)", x =>
            {
                x.State[x.Operation].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
                x.State[ISimpleAssignmentOperationWrapper.FromOperation(x.Operation.Instance).Target].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
            });
            validator.Validate("LocalReference: o", x =>
            {
                x.State[x.Operation].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                x.State[x.Operation.Instance.TrackedSymbol()].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
            });
        }

        [DataTestMethod]
        [DataRow("local!.ToString();", true)]
        [DataRow("field!.ToString();", true)]
        [DataRow("@event!.ToString();", false)]
        [DataRow("parameter!.ToString();", true)]
        [DataRow("array!.ToString();", true)]
        [DataRow("array[0]!.ToString();", false)]
        [DataRow("((int?)null)!.ToString();", false)]
        [DataRow("((int?)null + 3)!.ToString();", false)]
        public void NullForgivingOperatorSetsNotNullConstraintsOnOperationsAndSymbols(string expressionStatement, bool expectedHasSymbol)
        {
            var validator = new SETestContext($@"
#nullable enable
using System;

public class C {{
    object? field;
    event EventHandler? @event;

    public void M(object? parameter) {{
        object? local = null;
        object?[]? array = null;
        field = null;
        @event = null;
        parameter = null;

        {expressionStatement}
    }}
}}
", AnalyzerLanguage.CSharp, new SymbolicCheck[] { new SuppressNullableWarningCheck() }).Validator;
            validator.Validate($"ExpressionStatement: {expressionStatement}", x =>
            {
                var invocation = IExpressionStatementOperationWrapper.FromOperation(x.Operation.Instance).Operation;
                var instance = IInvocationOperationWrapper.FromOperation(invocation).Instance;
                var symbol = instance.TrackedSymbol();
                x.State[instance].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                if (expectedHasSymbol)
                {
                    x.State[symbol].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                }
                else
                {
                    symbol.Should().BeNull();
                }
            });
        }
    }
}
