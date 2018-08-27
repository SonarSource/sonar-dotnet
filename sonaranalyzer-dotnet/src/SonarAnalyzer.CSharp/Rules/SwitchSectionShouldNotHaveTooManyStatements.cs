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

using System.Collections.Generic;
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
    public sealed class SwitchSectionShouldNotHaveTooManyStatements : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1151";
        private const string MessageFormat = "Reduce this 'switch/case' number of lines from {0} to at most {1}, " +
            "for example by extracting code into a method.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const int ThresholdDefaultValue = 8;
        [RuleParameter("max", PropertyType.Integer, "Maximum number of lines of code.", ThresholdDefaultValue)]
        public int Threshold { get; set; } = ThresholdDefaultValue;

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var switchSection = (SwitchSectionSyntax)c.Node;

                    if (switchSection.IsMissing ||
                        switchSection.Labels.Count <= 0)
                    {
                        return;
                    }

                    var statementsCount = switchSection.Statements.SelectMany(GetSubStatements).Count();
                    if (statementsCount > Threshold)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, switchSection.Labels.First().GetLocation(),
                            statementsCount, Threshold));
                    }
                },
                SyntaxKind.SwitchSection);
        }

        private IEnumerable<StatementSyntax> GetSubStatements(StatementSyntax statement) =>
            statement.DescendantNodesAndSelf()
                .OfType<StatementSyntax>()
                .Where(s => !(s is BlockSyntax));
    }
}
