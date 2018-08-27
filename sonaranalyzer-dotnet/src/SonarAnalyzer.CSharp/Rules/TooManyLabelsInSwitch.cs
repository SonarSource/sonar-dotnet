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

using System.Collections.Immutable;
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
    public class TooManyLabelsInSwitch : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1479";
        private const string MessageFormat = "Consider reworking this 'switch' to reduce the number of 'case' from {1} to at most {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const int DefaultValueMaximum = 30;

        [RuleParameter("maximum", PropertyType.Integer, "Maximum number of case", DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var switchNode = (SwitchStatementSyntax)c.Node;
                    var type = c.SemanticModel.GetTypeInfo(switchNode.Expression).Type;

                    if (type == null ||
                        type.TypeKind == TypeKind.Enum)
                    {
                        return;
                    }

                    var numberOfSections = switchNode.Sections.Count;
                    if (numberOfSections > Maximum)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, switchNode.SwitchKeyword.GetLocation(), Maximum, numberOfSections));
                    }
                },
                SyntaxKind.SwitchStatement);
        }
    }
}
