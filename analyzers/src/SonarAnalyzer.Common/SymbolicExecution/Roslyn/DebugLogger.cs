/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
using SonarAnalyzer.CFG;
using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

#pragma warning disable S106    // Standard outputs should not be used directly to log anything

[ExcludeFromCodeCoverage]
internal class DebugLogger
{
    private const string DebugSymbol = "DEBUG";
    private const string Separator = "----------";

    private readonly bool isActive;

#if DEBUG
    public DebugLogger() =>
        isActive = Debugger.IsAttached; // Hardcode this locally to true, if you want every UT to log the output. Do not merge such a change.
#endif

    [Conditional(DebugSymbol)]
    public void Log(ControlFlowGraph cfg)
    {
        if (isActive)
        {
            Console.WriteLine(Separator);
            Console.WriteLine(CfgSerializer.Serialize(cfg));
        }
    }

    [Conditional(DebugSymbol)]
    public void Log(object value, string title = null, bool withIndent = false)
    {
        if (isActive)
        {
            var prefix = new string(' ', withIndent ? 4 : 0);
            Console.WriteLine($"{prefix}{Separator} {title} {Separator}");
            foreach (var line in value.ToString().Replace("\r\n", "\n").Split('\n'))
            {
                Console.WriteLine(prefix + line);
            }
        }
    }
}
