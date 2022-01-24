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

using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class RoslynCfgComparerBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S-COMPARE";
        private const string MessageFormat = "CFG Comparer";

        private static readonly DiagnosticDescriptor rule = new DiagnosticDescriptor(DiagnosticId, DiagnosticId, MessageFormat, "Debug", DiagnosticSeverity.Warning, true, null, null, DiagnosticDescriptorBuilder.MainSourceScopeTag);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        internal abstract string LanguageVersion(Compilation c);
        internal abstract string MethodName(SyntaxNodeAnalysisContext c);

        protected void ProcessBaseMethod(SyntaxNodeAnalysisContext c)
        {
            var sourceFileName = Path.GetFileNameWithoutExtension(c.Node.GetLocation().GetLineSpan().Path);
            var languageVersion = LanguageVersion(c.Compilation);
            var root = Path.GetFullPath(Path.GetDirectoryName(GetType().Assembly.Location) + @$"\..\..\..\..\RoslynData\{sourceFileName}\");
            var methodName = MethodName(c);
            Directory.CreateDirectory(root);
            var serialized = CFG.CfgSerializer.Serialize(SonarAnalyzer.CFG.Roslyn.ControlFlowGraph.Create(c.Node, c.SemanticModel), methodName);
            File.WriteAllText(root + $"CFG.{languageVersion}.{methodName}.txt",
                $@"// http://viz-js.com/
// https://edotor.net/?engine=dot#{System.Net.WebUtility.UrlEncode(serialized).Replace("+", "%20")}

/*
{c.Node}
*/

{serialized}");
        }
    }
}
