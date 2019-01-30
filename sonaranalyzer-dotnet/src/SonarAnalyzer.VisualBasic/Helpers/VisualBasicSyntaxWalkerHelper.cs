/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
 * mailto:contact@sonarsource.com
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.Helpers
{
    public static class VisualBasicSyntaxWalkerHelper
    {
        public static bool SafeVisit(this VisualBasicSyntaxWalker syntaxWalker, SyntaxNode syntaxNode)
        {
            try
            {
                syntaxWalker.Visit(syntaxNode);
                return true;
            }
            catch (InsufficientExecutionStackException)
            {
                // Roslyn walker overflows the stack when the depth of the call is around 2050.
                // See https://github.com/SonarSource/sonar-dotnet/issues/2115
                return false;
            }
        }
    }
}
