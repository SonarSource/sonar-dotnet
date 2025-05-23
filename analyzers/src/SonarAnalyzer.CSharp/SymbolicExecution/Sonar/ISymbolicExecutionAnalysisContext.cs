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

namespace SonarAnalyzer.SymbolicExecution.Sonar
{
    // The implementing class is responsible to encapsulate the generated data during method analysis (like diagnostics
    // or cached nodes) and clear it and the end.
    public interface ISymbolicExecutionAnalysisContext : IDisposable
    {
        // Some of the rules can return good results even if the tree was only partially visited; others need to completely
        // walk the tree in order to avoid false positives.
        // After the exploded graph was visited, a context could get in a state with partial results if a maximum number
        // of steps was reached or an exception was thrown during analysis.
        bool SupportsPartialResults { get; }

        IEnumerable<Diagnostic> GetDiagnostics();
    }
}
