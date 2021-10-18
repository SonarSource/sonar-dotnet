/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class OptionExplicitOn : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S6146";
        private const string MessageFormat = "{0}";
        private const string StatementMessage = "Change this to 'Option Explicit On'.";
        private const string AssemblyMessageFormat = "Configure 'Option Explicit On' for assembly '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var statement = (OptionStatementSyntax)c.Node;
                    if (statement.NameKeyword.IsKind(SyntaxKind.ExplicitKeyword) && statement.ValueKeyword.IsKind(SyntaxKind.OffKeyword))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, statement.GetLocation(), StatementMessage));
                    }
                },
                SyntaxKind.OptionStatement);

            context.RegisterCompilationStartAction(cStart =>
                cStart.RegisterCompilationEndAction(c =>
                {
                    if (!c.Compilation.VB().Options.OptionExplicit)
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, null, string.Format(AssemblyMessageFormat, c.Compilation.AssemblyName)));
                    }
                }));
        }
    }
}
