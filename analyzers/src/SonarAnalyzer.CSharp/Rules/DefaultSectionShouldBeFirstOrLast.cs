/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DefaultSectionShouldBeFirstOrLast : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4524";
        private const string MessageFormat = "Move this 'default:' case to the beginning or end of this 'switch' statement.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var switchSyntax = (SwitchStatementSyntax)c.Node;
                    var defaultLabelSectionIndex = switchSyntax.GetDefaultLabelSectionIndex();

                    if (defaultLabelSectionIndex > 0 &&                                 // default is not first...
                        defaultLabelSectionIndex != (switchSyntax.Sections.Count -1))   // nor last
                    {
                        var defaultLabelLocation = switchSyntax.Sections[defaultLabelSectionIndex]
                            .Labels
                            .First(label => label.IsKind(SyntaxKind.DefaultSwitchLabel))
                            .GetLocation();
                        c.ReportIssue(rule, defaultLabelLocation);
                    }
                },
                SyntaxKind.SwitchStatement);
        }
    }
}
