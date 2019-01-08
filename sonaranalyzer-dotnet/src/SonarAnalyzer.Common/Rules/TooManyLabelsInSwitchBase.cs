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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class TooManyLabelsInSwitchBase<TSyntaxKind, TSwitchStatementSyntax> : ParameterLoadingDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TSwitchStatementSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S1479";
        private const int DefaultValueMaximum = 30;

        [RuleParameter("maximum", PropertyType.Integer, "Maximum number of case", DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        protected abstract DiagnosticDescriptor Rule { get; }

        protected abstract TSyntaxKind[] SyntaxKinds { get; }

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected abstract SyntaxNode GetExpression(TSwitchStatementSyntax statement);

        protected abstract int GetSectionsCount(TSwitchStatementSyntax statement);

        protected abstract Location GetKeywordLocation(TSwitchStatementSyntax statement);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var switchNode = (TSwitchStatementSyntax)c.Node;
                    var type = c.SemanticModel.GetTypeInfo(GetExpression(switchNode)).Type;

                    if (type == null ||
                        type.TypeKind == TypeKind.Enum)
                    {
                        return;
                    }

                    var sectionsCount = GetSectionsCount(switchNode);
                    if (sectionsCount > Maximum)
                    {
                        c.ReportDiagnosticWhenActive(
                            Diagnostic.Create(Rule, GetKeywordLocation(switchNode), Maximum, sectionsCount));
                    }
                },
                SyntaxKinds);
        }
    }
}
