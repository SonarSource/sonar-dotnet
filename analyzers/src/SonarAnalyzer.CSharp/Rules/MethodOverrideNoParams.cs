/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MethodOverrideNoParams : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3262";
        private const string MessageFormat = "'params' should not be removed from an override.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var method = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(method);

                    if (methodSymbol is not null
                        && methodSymbol.IsOverride
                        && methodSymbol.OverriddenMethod is not null
                        && methodSymbol.OverriddenMethod.Parameters.Any(p => p.IsParams)
                        && !method.ParameterList.Parameters.Last().Modifiers.Any(SyntaxKind.ParamsKeyword))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, method.ParameterList.Parameters.Last().GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration);
    }
}
