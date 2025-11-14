/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class OptionExplicitOn : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S6146";
        private const string MessageFormat = "{0}";
        private const string StatementMessage = "Change this to 'Option Explicit On'.";
        private const string AssemblyMessageFormat = "Configure 'Option Explicit On' for assembly '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
                {
                    var statement = (OptionStatementSyntax)c.Node;
                    if (statement.NameKeyword.IsKind(SyntaxKind.ExplicitKeyword) && statement.ValueKeyword.IsKind(SyntaxKind.OffKeyword))
                    {
                        c.ReportIssue(Rule, statement, StatementMessage);
                    }
                },
                SyntaxKind.OptionStatement);

            context.RegisterCompilationStartAction(cStart =>
                cStart.RegisterCompilationEndAction(c =>
                {
                    if (!c.Compilation.VB().Options.OptionExplicit)
                    {
                        c.ReportIssue(Rule, (Location)null, string.Format(AssemblyMessageFormat, c.Compilation.AssemblyName));
                    }
                }));
        }
    }
}
