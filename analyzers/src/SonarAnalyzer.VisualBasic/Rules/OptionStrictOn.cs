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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class OptionStrictOn : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S6145";
        private const string MessageFormat = "{0}";
        private const string StatementMessage = "Change this to 'Option Strict On'.";
        private const string AssemblyMessageFormat = "Configure 'Option Strict On' for assembly '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
            {
                var statement = (OptionStatementSyntax)c.Node;
                if (statement.NameKeyword.IsKind(SyntaxKind.StrictKeyword) && !statement.ValueKeyword.IsKind(SyntaxKind.OnKeyword))
                {
                    c.ReportIssue(Rule, statement, StatementMessage);
                }
            },
            SyntaxKind.OptionStatement);

            context.RegisterCompilationStartAction(cStart =>
                cStart.RegisterCompilationEndAction(c =>
                {
                    if (c.Compilation.VB().Options.OptionStrict != OptionStrict.On)
                    {
                        c.ReportIssue(Rule, (Location)null, string.Format(AssemblyMessageFormat, c.Compilation.AssemblyName));
                    }
                }));
        }
    }
}
