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


namespace SonarAnalyzer.SyntaxTrackers
{
    public delegate bool ConstantValuePredicate(object constantValue);

    /// <summary>
    /// Verifies the initialization of an object, whether one or more properties have been correctly set when the object was initialized.
    /// </summary>
    /// A correct initialization could consist of:
    /// - EITHER invoking the constructor with specific parameters
    /// - OR invoking the constructor and then setting some specific properties on the created object
    public class CSharpObjectInitializationTracker
    {
        /// <summary>
        /// Tests if the provided <paramref name="constantValue"/> is equal to allowed value.
        /// </summary>
        /// <returns>True when <paramref name="constantValue"/> is an allowed value, otherwise false.</returns>
        private readonly ConstantValuePredicate constantValuePredicate;

        public bool IsAllowedConstantValue(object constantValue) => constantValuePredicate(constantValue);

        public CSharpObjectInitializationTracker(ConstantValuePredicate isAllowedConstantValue)
        {
            constantValuePredicate = isAllowedConstantValue;
        }

    }
}
