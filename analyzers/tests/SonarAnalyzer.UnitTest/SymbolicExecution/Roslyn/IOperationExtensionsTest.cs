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

using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class IOperationExtensionsTest
    {
        [Ignore][TestMethod]
        public void TrackedSymbol_LocalReference_IsVariableSymbol()
        {
            var localReference = ((ISimpleAssignmentOperation)TestHelper.CompileCfgBodyCS("var a = true;").Blocks[1].Operations[0]).Target;
            var symbol = ILocalReferenceOperationWrapper.FromOperation(localReference).Local;
            localReference.TrackedSymbol().Should().Be(symbol);
        }

        [Ignore][TestMethod]
        public void TrackedSymbol_ParameterReference_IsParameterSymbol()
        {
            var expressionStatement = (IExpressionStatementOperation)TestHelper.CompileCfgBodyCS("parameter = true;", "bool parameter").Blocks[1].Operations[0];
            var parameterReference = ((ISimpleAssignmentOperation)expressionStatement.Operation).Target;
            var symbol = IParameterReferenceOperationWrapper.FromOperation(parameterReference).Parameter;
            parameterReference.TrackedSymbol().Should().Be(symbol);
        }

        [DataTestMethod]
        [DataRow(@"field = 1")]
        [DataRow(@"this.field = 1")]
        [DataRow(@"StaticField = 1")]
        [DataRow(@"C.StaticField = 1")]
        public void TrackedSymbol_FieldReference_IsFieldSymbol(string assignment)
        {
            var code = $"public class C {{ int field; static int StaticField; void Method() {{ {assignment}; }} }}";
            var graph = TestHelper.CompileCfgCS(code);
            var expressionStatement = (IExpressionStatementOperation)graph.Blocks[1].Operations[0];
            var assignmentTarget = ((ISimpleAssignmentOperation)expressionStatement.Operation).Target;
            var fieldReferenceSymbol = IFieldReferenceOperationWrapper.FromOperation(assignmentTarget).Field;
            assignmentTarget.TrackedSymbol().Should().Be(fieldReferenceSymbol);
        }

        [Ignore][TestMethod]
        public void TrackedSymbol_SimpleAssignment_IsNull()
        {
            var simpleAssignment = TestHelper.CompileCfgBodyCS("var a = true; bool b; b = a;").Blocks[1].Operations[0];
            simpleAssignment.TrackedSymbol().Should().BeNull();
        }
    }
}
