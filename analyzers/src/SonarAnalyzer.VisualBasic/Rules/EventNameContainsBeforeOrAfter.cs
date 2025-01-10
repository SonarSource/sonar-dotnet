/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class EventNameContainsBeforeOrAfter : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2349";
        private const string MessageFormat = "Rename this event to remove the '{0}' {1}.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const string PrefixLiteral = "prefix";
        private const string SuffixLiteral = "suffix";
        private const string AfterLiteral = "after";
        private const string BeforeLiteral = "before";

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
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

                    c.ReportIssue(rule, eventStatement.Identifier, matched, part);
                },
                SyntaxKind.EventStatement);
        }
    }
}
