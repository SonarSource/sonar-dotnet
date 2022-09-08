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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class ObjectConstraintTest
    {
        [DataTestMethod]
        [DataRow("private object field;", false)]
        [DataRow("private static object field;", true)]
        [DataRow("private readonly object field;", false)]
        [DataRow("private readonly static object field;", true)]
        [DataRow("private volatile object field;", false)]
        [DataRow("private volatile static object field;", true)]
        [DataRow("protected object field;", false)]
        [DataRow("protected static object field;", true)]
        [DataRow("protected readonly object field;", false)]
        [DataRow("protected readonly static object field;", true)]
        [DataRow("protected volatile object field;", false)]
        [DataRow("protected volatile static object field;", true)]
        [DataRow("internal object field;", false)]
        [DataRow("internal static object field;", true)]
        [DataRow("internal readonly object field;", false)]
        [DataRow("internal readonly static object field;", true)]
        [DataRow("internal volatile object field;", false)]
        [DataRow("internal volatile static object field;", true)]
        [DataRow("public object field;", false)]
        [DataRow("public static object field;", true)]
        [DataRow("public readonly object field;", false)]
        [DataRow("public readonly static object field;", true)]
        [DataRow("public volatile object field;", false)]
        [DataRow("public volatile static object field;", true)]
        public void PreserveOnFieldReset_Null(string fieldDeclaration, bool expectedPreserveOnFieldReset)
        {
            var fieldSymbol = TestHelper.GetFieldSymbol(fieldDeclaration);
            var result = ObjectConstraint.Null.PreserveOnFieldReset(fieldSymbol);
            result.Should().Be(expectedPreserveOnFieldReset);
        }

        [DataTestMethod]
        [DataRow("private object field;", false)]
        [DataRow("private static object field;", true)]
        [DataRow("private readonly object field;", false)]
        [DataRow("private readonly static object field;", true)]
        [DataRow("private volatile object field;", false)]
        [DataRow("private volatile static object field;", true)]
        [DataRow("protected object field;", false)]
        [DataRow("protected static object field;", true)]
        [DataRow("protected readonly object field;", false)]
        [DataRow("protected readonly static object field;", true)]
        [DataRow("protected volatile object field;", false)]
        [DataRow("protected volatile static object field;", true)]
        [DataRow("internal object field;", false)]
        [DataRow("internal static object field;", true)]
        [DataRow("internal readonly object field;", false)]
        [DataRow("internal readonly static object field;", true)]
        [DataRow("internal volatile object field;", false)]
        [DataRow("internal volatile static object field;", true)]
        [DataRow("public object field;", false)]
        [DataRow("public static object field;", true)]
        [DataRow("public readonly object field;", false)]
        [DataRow("public readonly static object field;", true)]
        [DataRow("public volatile object field;", false)]
        [DataRow("public volatile static object field;", true)]
        public void PreserveOnFieldReset_NotNull(string fieldDeclaration, bool expectedPreserveOnFieldReset)
        {
            var fieldSymbol = TestHelper.GetFieldSymbol(fieldDeclaration);
            var result = ObjectConstraint.NotNull.PreserveOnFieldReset(fieldSymbol);
            result.Should().Be(expectedPreserveOnFieldReset);
        }
    }
}
