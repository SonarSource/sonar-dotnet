﻿/*
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

using System.Runtime.CompilerServices;
using SonarAnalyzer.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution
{
    internal class TestConstraint : SymbolicConstraint
    {
        public static readonly TestConstraint First = new("First");
        public static readonly TestConstraint Second = new("Second");

        public override SymbolicConstraint Opposite =>
            this == First ? Second : First;

        protected override string Name { get; }

        private TestConstraint(string name) =>
            Name = name;
    }

    internal class DummyConstraint : SymbolicConstraint
    {
        public static readonly DummyConstraint Dummy = new();

        public override SymbolicConstraint Opposite => throw new System.NotImplementedException();
        protected override string Name { get; } = "Dummy";

        private DummyConstraint() { }
    }

    internal class PreserveOnFieldResetConstraint : SymbolicConstraint
    {
        private readonly Func<IFieldSymbol, bool> preserveOnFieldReset;
        public static readonly PreserveOnFieldResetConstraint AlwaysPreserve = new PreserveOnFieldResetConstraint(nameof(AlwaysPreserve), _ => true);
        public static readonly PreserveOnFieldResetConstraint AlwaysReset = new PreserveOnFieldResetConstraint(nameof(AlwaysReset), _ => false);

        public override SymbolicConstraint Opposite => null;
        protected override string Name { get; }

        public PreserveOnFieldResetConstraint(string name, Func<IFieldSymbol, bool> preserveOnFieldReset)
        {
            Name = name;
            this.preserveOnFieldReset = preserveOnFieldReset;
        }

        public override bool PreserveOnFieldReset(IFieldSymbol field) =>
            preserveOnFieldReset(field);

        public static PreserveOnFieldResetConstraint DistingushConstraint<TDiscriminator>(Func<IFieldSymbol, bool> preserveOnFieldReset) =>
            new DistingushableConstraint<TDiscriminator>(preserveOnFieldReset);

        private class DistingushableConstraint<TDiscriminator> : PreserveOnFieldResetConstraint
        {
            public DistingushableConstraint(Func<IFieldSymbol, bool> preserveOnFieldReset) : base(typeof(TDiscriminator).Name, preserveOnFieldReset) { }
        }
    }
}
