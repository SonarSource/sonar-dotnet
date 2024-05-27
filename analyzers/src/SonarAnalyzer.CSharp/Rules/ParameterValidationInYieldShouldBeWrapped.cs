/*
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

using SonarAnalyzer.Common.Walkers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ParameterValidationInYieldShouldBeWrapped : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4456";
        private const string MessageFormat = "Split this method into two, one handling parameters check and the other " +
           "handling the iterator.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;

                    var walker = new ParameterValidationInYieldWalker(c.SemanticModel);
                    walker.SafeVisit(methodDeclaration);

                    if (walker.HasYieldStatement &&
                        walker.ArgumentExceptionLocations.Any())
                    {
                        c.ReportIssue(Rule, methodDeclaration.Identifier, walker.ArgumentExceptionLocations);
                    }
                },
                SyntaxKind.MethodDeclaration);

        private sealed class ParameterValidationInYieldWalker : ParameterValidationInMethodWalker
        {
            public bool HasYieldStatement { get; private set; }

            public ParameterValidationInYieldWalker(SemanticModel semanticModel)
                : base(semanticModel)
            {
            }

            public override void VisitYieldStatement(YieldStatementSyntax node)
            {
                HasYieldStatement = true;
                base.VisitYieldStatement(node);
            }
        }
    }
}
