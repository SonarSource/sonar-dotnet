﻿/*
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

using SonarAnalyzer.CFG.LiveVariableAnalysis;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.CFG.Sonar;

namespace SonarAnalyzer.CFG;

public static partial class CfgSerializer
{
    public static string Serialize(IControlFlowGraph cfg, string title = "SonarCfg")
    {
        var writer = new DotWriter();
        new SonarCfgWalker(writer).Visit(cfg, title);
        return writer.ToString();
    }

    public static string Serialize(ControlFlowGraph cfg, string title = "RoslynCfg")
    {
        var writer = new DotWriter();
        new RoslynCfgWalker(writer, new RoslynCfgIdProvider()).Visit(cfg, title);
        return writer.ToString();
    }

    public static string Serialize(RoslynLiveVariableAnalysis lva, string title = "RoslynCfgLva")
    {
        var writer = new DotWriter();
        new RoslynLvaWalker(lva, writer, new RoslynCfgIdProvider()).Visit(lva.Cfg, title);
        return writer.ToString();
    }
}
