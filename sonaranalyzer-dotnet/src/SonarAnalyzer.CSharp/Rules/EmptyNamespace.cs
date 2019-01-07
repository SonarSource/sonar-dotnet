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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class EmptyNamespace : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3261";
        private const string MessageFormat = "Remove this empty namespace.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            // in order to let the tests work properly, we do this in a tree action
            // https://github.com/dotnet/roslyn/issues/4745 was fixed in Roslyn 1.1,
            // so in the IDE it should already be fine.
            context.RegisterSyntaxTreeActionInNonGenerated(
                c =>
                {
                    var namespaces = c.Tree.GetCompilationUnitRoot().DescendantNodes()
                        .OfType<NamespaceDeclarationSyntax>()
                        .Where(ns=> !ns.Members.Any());

                    foreach (var ns in namespaces)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, ns.GetLocation()));
                    }
                });
        }
    }
}
