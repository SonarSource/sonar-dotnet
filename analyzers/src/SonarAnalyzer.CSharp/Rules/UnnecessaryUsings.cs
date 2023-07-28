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

using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnnecessaryUsings : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1128";
        private const string MessageFormat = "Remove this unnecessary 'using'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    // When using top level statements, we are called twice for the same compilation unit. The second call has the containing symbol kind equal to `Method`.
                    if (c.IsRedundantPositionalRecordContext())
                    {
                        return;
                    }
                    var diagnostics = c.SemanticModel.GetDiagnostics(cancellationToken: c.Cancel);
                    var root = c.Node.SyntaxTree.GetRoot();
                    foreach (var diagnostic in diagnostics)
                    {
                        if (diagnostic.Id == "CS8019" && root.FindNode(diagnostic.Location.SourceSpan) is UsingDirectiveSyntax usingDirective)
                        {
                            c.ReportIssue(Diagnostic.Create(Rule, usingDirective.GetLocation()));
                        }
                    }
                    return;
                },
                SyntaxKind.CompilationUnit);
    }
}
