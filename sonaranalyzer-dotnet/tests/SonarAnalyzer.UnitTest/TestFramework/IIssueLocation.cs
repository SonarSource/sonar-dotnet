/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

namespace SonarAnalyzer.UnitTest.TestFramework
{
    public interface IIssueLocation
    {
        /// <summary>
        /// Gets the value specifying whether this issue is a location reported by a diagnostic.
        /// </summary>
        bool IsPrimary { get; }

        /// <summary>
        /// Gets the 1-based line number in the source file.
        /// </summary>
        int LineNumber { get; }

        /// <summary>
        /// Gets the issue message, as reported by the diagnostic analyzer.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets the id of the issue this location belongs to.
        /// </summary>
        string IssueId { get; }

        /// <summary>
        /// Gets the start of the issue span, or null when not specified and should not be checked.
        /// </summary>
        int? Start { get; }

        /// <summary>
        /// Gets the length of the issue span, or null when not specified and should not be checked.
        /// </summary>
        int? Length { get; }
    }
}
