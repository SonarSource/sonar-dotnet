/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Reflection;

namespace SonarAnalyzer.ShimLayer.Common;

internal static class TypeRegister
{
    // This may need to be extended to other assemblies if needed. See TypeLoader.LoadBaseline and .LoadLatest
    private static readonly Assembly[] Assemblies = [typeof(SyntaxNode).Assembly, typeof(CSharpSyntaxNode).Assembly];

    public static Type LatestType(string name, string fallbackName = null)
    {
        return Load(name) ?? Load(fallbackName);

        static Type Load(string name) =>
            name is null ? null : Assemblies.Select(x => x.GetType(name)).FirstOrDefault(x => x is not null);
    }
}
