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

using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class ProgramStateTest
    {
        [TestMethod]
        public void SetException_IsImmutable()
        {
            var sut = ProgramState.Empty;
            sut.Exception.Should().BeNull();

            sut = sut.SetException(ExceptionState.UnknownException);
            sut.Exception.Should().Be(ExceptionState.UnknownException);

            ProgramState.Empty.Exception.Should().BeNull();
        }

        [TestMethod]
        public void ResetException_IsImmutable()
        {
            var original = ProgramState.Empty.SetException(ExceptionState.UnknownException);
            var other = original.ResetException();
            other.Exception.Should().BeNull();
            original.Exception.Should().Be(ExceptionState.UnknownException);
        }
    }
}
