/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

namespace SonarAnalyzer.SymbolicExecution.Constraints
{
    public sealed class StringConstraint : SymbolicValueConstraint
    {
        public static readonly StringConstraint EmptyString = new StringConstraint();
        public static readonly StringConstraint FullString = new StringConstraint();
        public static readonly StringConstraint FullOrNullString = new StringConstraint();

        public static readonly StringConstraint WhiteSpaceString = new StringConstraint();
        public static readonly StringConstraint NotWhiteSpaceString = new StringConstraint();
        public static readonly StringConstraint FullNotWhiteSpaceString = new StringConstraint();

        public static bool IsNotNullConstraint(StringConstraint constraint) =>
            constraint == StringConstraint.FullString
            || constraint == StringConstraint.EmptyString
            || constraint == StringConstraint.WhiteSpaceString
            || constraint == StringConstraint.FullNotWhiteSpaceString;

        // Currently FullOrNullString and NotWhiteSpaceString  is never set as a constraint. It is there to imply the opposite of EmptyString
        public override SymbolicValueConstraint OppositeForLogicalNot
        {
            get
            {
                if (this == EmptyString)
                {
                    return FullOrNullString;
                }
                if (this == WhiteSpaceString)
                {
                    return NotWhiteSpaceString;
                }
                return null;
            }
        }

        public override string ToString()
        {
            if (this == EmptyString)
            {
                return "EmptyString";
            }

            if (this == FullString)
            {
                return "FullString";
            }

            if (this == FullOrNullString)
            {
                return "FullOrNullString";
            }

            if (this == WhiteSpaceString)
            {
                return "FullOrNullString";
            }

            if (this == NotWhiteSpaceString)
            {
                return "FullOrNullString";
            }

            if (this == FullNotWhiteSpaceString)
            {
                return "FullNotWhiteSpaceString";
            }

            return "null";
        }
    }
}
