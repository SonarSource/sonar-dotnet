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
    public sealed class EventNameContainsBeforeOrAfter : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2349";
        private const string MessageFormat = "Rename this event to remove the '{0}' {1}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const string PrefixLiteral = "prefix";
        private const string SuffixLiteral = "suffix";
        private const string AfterLiteral = "after";
        private const string BeforeLiteral = "before";

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var eventStatement = (EventStatementSyntax)c.Node;
                    var name = eventStatement.Identifier.ValueText;

                    string part;
                    string matched;

                    if (name.StartsWith(BeforeLiteral, System.StringComparison.OrdinalIgnoreCase))
                    {
                        part = PrefixLiteral;
                        matched = name.Substring(0, BeforeLiteral.Length);
                    }
                    else if (name.StartsWith(AfterLiteral, System.StringComparison.OrdinalIgnoreCase))
                    {
                        part = PrefixLiteral;
                        matched = name.Substring(0, AfterLiteral.Length);
                    }
                    else if (name.EndsWith(BeforeLiteral, System.StringComparison.OrdinalIgnoreCase))
                    {
                        part = SuffixLiteral;
                        matched = name.Substring(name.Length - 1 - BeforeLiteral.Length);
                    }
                    else if (name.EndsWith(AfterLiteral, System.StringComparison.OrdinalIgnoreCase))
                    {
                        part = SuffixLiteral;
                        matched = name.Substring(name.Length - 1 - AfterLiteral.Length);
                    }
                    else
                    {
                        return;
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, eventStatement.Identifier.GetLocation(), matched, part));
                },
                SyntaxKind.EventStatement);
        }
    }
}
