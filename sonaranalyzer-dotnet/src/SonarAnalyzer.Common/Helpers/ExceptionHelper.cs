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

using System;

namespace SonarAnalyzer.Helpers
{
    public static class ExceptionHelper
    {
        /// <summary>
        /// Create a one line exception message that contains all the exception information.
        /// Usage: Roslyn/MSBuild is currently cutting exception message at the end of the line instead
        /// of displaying the full message. As a workaround, we replace the line ending with ' ## '.
        /// See https://github.com/dotnet/roslyn/issues/1455 and https://github.com/dotnet/roslyn/issues/24346
        /// </summary>
        public static string OneLineReportToPreventRoslynTruncation(string message)
        {
            return message.Replace(Environment.NewLine, " ## ");
        }
    }
}
