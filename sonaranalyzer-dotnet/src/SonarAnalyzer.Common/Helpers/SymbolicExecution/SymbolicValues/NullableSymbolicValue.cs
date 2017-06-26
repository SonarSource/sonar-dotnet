/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

namespace SonarAnalyzer.Helpers.FlowAnalysis.Common
{
    public class NullableSymbolicValue : SymbolicValue
    {
        public static readonly NullableSymbolicValue Unknown = new NullableSymbolicValue();
        public static readonly NullableSymbolicValue Empty = new NullableSymbolicValue();

        public SymbolicValue WrappedValue { get; }

        private NullableSymbolicValue(SymbolicValue wrappedValue = null)
        {
            WrappedValue = wrappedValue;
        }

        public static NullableSymbolicValue Create(SymbolicValue wrappedValue)
        {
            return new NullableSymbolicValue(wrappedValue);
        }

        public override string ToString()
        {
            if (identifier != null)
            {
                return $"NULLABLE_SV_{identifier}";
            }

            return WrappedValue?.ToString() ?? base.ToString();
        }
    }
}
