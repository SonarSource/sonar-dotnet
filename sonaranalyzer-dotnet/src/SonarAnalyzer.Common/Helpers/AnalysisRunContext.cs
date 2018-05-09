/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    internal class AnalysisRunContext : IAnalysisRunContext
    {
        public AnalysisRunContext(SyntaxTree syntaxTree, IEnumerable<DiagnosticDescriptor> supportedDiagnostics)
        {
            SyntaxTree = syntaxTree;
            SupportedDiagnostics = supportedDiagnostics ?? Enumerable.Empty<DiagnosticDescriptor>();
        }

        public SyntaxTree SyntaxTree { get; }

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics { get; }

        // Real bug found in Roslyn - see https://github.com/dotnet/roslyn/pull/21258
        public static bool TryRedirect(AssemblyName name, byte[] token, int major, int minor, int build, int revision)
        {
            var version = new Version(major, minor, revision, build);
            if (KeysEqual(name.GetPublicKeyToken(), token) && name.Version < version)
            {
                name.Version = version;
                return true;
            }

            return false;
        }

        private static bool KeysEqual(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            for (var i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

    }
}
