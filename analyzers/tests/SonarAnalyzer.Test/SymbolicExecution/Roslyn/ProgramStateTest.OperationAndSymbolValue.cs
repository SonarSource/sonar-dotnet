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

using SonarAnalyzer.Extensions;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class ProgramStateTest
{
    [TestMethod]
    public void SetOperationAndSymbolValue_TrackedSymbol()
    {
        var operations = TestHelper.CompileCfgBodyCS("bool b = true;").Blocks[1].Operations;
        var localReference = operations[0].ChildOperations.First().ToLocalReference();
        var symbol = localReference.Local;
        var sut = ProgramState.Empty;

        sut[localReference].Should().BeNull();
        sut[symbol].Should().BeNull();

        sut = sut.SetOperationAndSymbolValue(localReference.WrappedOperation, SymbolicValue.True);

        sut[localReference].Should().Be(SymbolicValue.True);
        sut[symbol].Should().Be(SymbolicValue.True);
    }

    [TestMethod]
    public void SetOperationAndSymbolValue_NotTrackedSymbol()
    {
        var operations = TestHelper.CompileCfgBodyCS("""var texts = new string[] { }; texts[42] = string.Empty;""").Blocks[1].Operations;
        var arrayElementReference = operations[1].ChildOperations.First().ChildOperations.First().ToArrayElementReference();
        var sut = ProgramState.Empty;

        sut[arrayElementReference].Should().BeNull();

        sut = sut.SetOperationAndSymbolValue(arrayElementReference.WrappedOperation, SymbolicValue.NotNull);

        sut[arrayElementReference].Should().Be(SymbolicValue.NotNull);
    }
}
