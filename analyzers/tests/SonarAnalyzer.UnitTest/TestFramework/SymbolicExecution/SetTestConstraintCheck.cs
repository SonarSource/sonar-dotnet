/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution
{
    internal class SetTestConstraintCheck : SymbolicCheck
    {
        public override ProgramState PreProcess(SymbolicContext context)
        {
            if (context.Operation.Instance is ILocalReferenceOperation local && context.Operation.IsImplicit)
            {
                var sv = new SymbolicValue(new SymbolicValueCounter()); // ToDo: Improve check design
                sv.SetConstraint(local.Local.Name switch
                {
                    "first" => TestConstraint.First,
                    "second" => TestConstraint.Second,
                    _ => throw new InvalidOperationException("Unexpected local variable name: " + local.Local.Name)
                });
                return context.State.SetSymbolValue(local.Local, sv);
            }
            else
            {
                return context.State;
            }
        }
    }
}
