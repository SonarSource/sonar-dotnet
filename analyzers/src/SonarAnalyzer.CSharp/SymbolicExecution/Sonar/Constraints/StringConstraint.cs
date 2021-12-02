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

namespace SonarAnalyzer.SymbolicExecution.Sonar.Constraints
{
    internal sealed class StringConstraint : SymbolicConstraint
    {
        public static readonly StringConstraint EmptyString = new();
        public static readonly StringConstraint FullString = new();
        public static readonly StringConstraint FullOrNullString = new();
        public static readonly StringConstraint WhiteSpaceString = new();
        public static readonly StringConstraint NotWhiteSpaceString = new();
        public static readonly StringConstraint FullNotWhiteSpaceString = new();

        // Currently FullOrNullString and NotWhiteSpaceString  is never set as a constraint. It is there to imply the opposite of EmptyString
        public override SymbolicConstraint Opposite
        {
            get
            {
                if (this == EmptyString)
                {
                    return FullOrNullString;
                }
                else if (this == WhiteSpaceString)
                {
                    return NotWhiteSpaceString;
                }
                else
                {
                    return null;
                }
            }
        }

        protected override string Name
        {
            get
            {
                if (this == EmptyString)
                {
                    return nameof(EmptyString);
                }
                else if (this == FullString)
                {
                    return nameof(FullString);
                }
                else if (this == FullOrNullString)
                {
                    return nameof(FullOrNullString);
                }
                else if (this == WhiteSpaceString)
                {
                    return nameof(WhiteSpaceString);
                }
                else if (this == NotWhiteSpaceString)
                {
                    return nameof(NotWhiteSpaceString);
                }
                else if (this == FullNotWhiteSpaceString)
                {
                    return nameof(FullNotWhiteSpaceString);
                }
                else
                {
                    throw new InvalidOperationException("Unexpected object state");
                }
            }
        }

        private StringConstraint() { }

        public static bool IsNotNullConstraint(StringConstraint constraint) =>  // FIXME: Rename
            constraint == StringConstraint.FullString
            || constraint == StringConstraint.EmptyString
            || constraint == StringConstraint.WhiteSpaceString
            || constraint == StringConstraint.FullNotWhiteSpaceString;
    }
}
