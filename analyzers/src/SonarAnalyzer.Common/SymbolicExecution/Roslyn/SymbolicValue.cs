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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    public class SymbolicValue : IEquatable<SymbolicValue>
    {
        private readonly int identifier;    // This is debug information that is intentionally excluded from GetHashCode and Equals
        private readonly Lazy<Dictionary<Type, SymbolicConstraint>> constraints = new(() => new());  // SymbolicValue can have only one constraint instance of specific type at a time

        public SymbolicValue(SymbolicValueCounter counter) =>
            identifier = counter.NextIdentifier();

        public override string ToString()
        {
            var ret = new StringBuilder();
            ret.Append("SV_").Append(identifier);
            if (constraints.Value.Any())
            {
                ret.Append(": ").Append(constraints.Value.Values.JoinStr(", ", x => x.ToString()));
            }
            return ret.ToString();
        }

        public void SetConstraint(SymbolicConstraint constraint) =>
            constraints.Value[constraint.GetType()] = constraint;

        public void RemoveConstraint(SymbolicConstraint constraint)
        {
            if (HasConstraint(constraint))
            {
                constraints.Value.Remove(constraint.GetType());
            }
        }

        public bool HasConstraint<T>() where T : SymbolicConstraint =>
            constraints.Value.ContainsKey(typeof(T));

        public bool HasConstraint(SymbolicConstraint constraint) =>
            constraints.Value.TryGetValue(constraint.GetType(), out var current) && constraint == current;

        public override int GetHashCode() => 0; // We can't calculate stable hash code. This class is not supposed to be used as a key in sets and dictionaries.

        public override bool Equals(object obj) =>
            Equals(obj as SymbolicValue);

        public virtual bool Equals(SymbolicValue other) =>
            other is not null
            && other.constraints.IsValueCreated == constraints.IsValueCreated
            && (!constraints.IsValueCreated || other.constraints.Value.DictionaryEquals(constraints.Value));
    }
}
