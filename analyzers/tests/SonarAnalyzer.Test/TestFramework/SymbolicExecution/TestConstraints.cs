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

using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.Test.TestFramework.SymbolicExecution
{
    internal enum ConstraintKindTest
    {
        // Make sure to not conflict with the "real" ConstraintKind values:
        First = -1,
        Second = -2,
        Dummy = -3,
        Fake = -4,
    }

    internal abstract class TestConstraintBase : SymbolicConstraint
    {
        protected TestConstraintBase(ConstraintKindTest kind) : base((ConstraintKind)kind) { }

        public override string ToString() =>
            ((ConstraintKindTest)Kind).ToString("G");
    }

    internal sealed class TestConstraint : TestConstraintBase
    {
        public static readonly TestConstraint First = new(ConstraintKindTest.First);
        public static readonly TestConstraint Second = new(ConstraintKindTest.Second);

        public override SymbolicConstraint Opposite =>
            this == First ? Second : First;

        private TestConstraint(ConstraintKindTest kind) : base(kind) { }
    }

    internal sealed class DummyConstraint : TestConstraintBase
    {
        public static readonly DummyConstraint Dummy = new();

        public override SymbolicConstraint Opposite => throw new NotImplementedException();

        private DummyConstraint() : base(ConstraintKindTest.Dummy) { }
    }
}
