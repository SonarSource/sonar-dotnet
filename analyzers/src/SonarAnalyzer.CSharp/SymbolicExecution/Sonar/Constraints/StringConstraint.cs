/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.SymbolicExecution.Sonar.Constraints
{
    internal sealed class StringConstraint : SymbolicConstraint
    {
        public static readonly StringConstraint EmptyString = new(ConstraintKind.StringConstraintEmpty);
        public static readonly StringConstraint FullString = new(ConstraintKind.StringConstraintFull);
        public static readonly StringConstraint FullOrNullString = new(ConstraintKind.StringConstraintFullOrNull);
        public static readonly StringConstraint WhiteSpaceString = new(ConstraintKind.StringConstraintWhiteSpace);
        public static readonly StringConstraint NotWhiteSpaceString = new(ConstraintKind.StringConstraintNotWhiteSpace);
        public static readonly StringConstraint FullNotWhiteSpaceString = new(ConstraintKind.StringConstraintFullNotWhiteSpace);

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

        private StringConstraint(ConstraintKind kind) : base(kind) { }

        public static bool IsNotNull(StringConstraint constraint) =>
            constraint == StringConstraint.FullString
            || constraint == StringConstraint.EmptyString
            || constraint == StringConstraint.WhiteSpaceString
            || constraint == StringConstraint.FullNotWhiteSpaceString;
    }
}
