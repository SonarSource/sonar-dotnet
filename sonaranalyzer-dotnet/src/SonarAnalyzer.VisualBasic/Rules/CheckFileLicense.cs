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

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class CheckFileLicense : CheckFileLicenseBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);


        internal const string HeaderFormatDefaultValue =
@" ' SonarQube, open source software quality management tool.
 ' Copyright (C) 2008-2013 SonarSource
 ' mailto:contact AT sonarsource DOT com
 '
 ' SonarQube is free software; you can redistribute it and/or
 ' modify it under the terms of the GNU Lesser General Public
 ' License as published by the Free Software Foundation; either
 ' version 3 of the License, or (at your option) any later version.
 '
 ' SonarQube is distributed in the hope that it will be useful,
 ' but WITHOUT ANY WARRANTY; without even the implied warranty of
 ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 ' Lesser General Public License for more details.
 '
 ' You should have received a copy of the GNU Lesser General Public License
 ' along with this program; if not, write to the Free Software Foundation,
 ' Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
";


        [RuleParameter(HeaderFormatRuleParameterKey, PropertyType.String, "Expected copyright and license header.",
            HeaderFormatDefaultValue)]
        public override string HeaderFormat { get; set; } = HeaderFormatDefaultValue;

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxTreeActionInNonGenerated(
                stac =>
                {
                    if (HeaderFormat == null)
                    {
                        return;
                    }

                    if (IsRegularExpression && !IsRegexPatternValid(HeaderFormat))
                    {
                        throw new InvalidOperationException($"Invalid regular expression: {HeaderFormat}");
                    }

                    var firstNode = stac.Tree.GetRoot().ChildTokens().FirstOrDefault().Parent;
                    if (!HasValidLicenseHeader(firstNode))
                    {
                        var properties = CreateDiagnosticProperties();
                        stac.ReportDiagnosticWhenActive(Diagnostic.Create(rule, Location.Create(stac.Tree,
                            TextSpan.FromBounds(0, 0)), properties));
                    }
                });
        }
    }
}
