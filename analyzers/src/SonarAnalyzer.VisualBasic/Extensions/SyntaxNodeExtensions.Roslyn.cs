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

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.VisualBasic.Extensions;

[ExcludeFromCodeCoverage]
internal static class SyntaxNodeExtensions
{
    // Copied and converted from
    // https://github.com/dotnet/roslyn/blob/5a1cc5f83e4baba57f0355a685a5d1f487bfac66/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/VisualBasic/Extensions/SyntaxNodeExtensions.vb#L16
    public static bool IsParentKind(this SyntaxNode node, SyntaxKind kind)
    {
        return node != null && node.Parent.IsKind(kind);
    }
}
