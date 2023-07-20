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
    public sealed class MethodOverrideChangedDefaultValue : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1006";
        private const string MessageFormat = "{0} the default parameter value {1}.";
        internal const string MessageAdd = "defined in the overridden method";
        internal const string MessageRemove = "to match the signature of overridden method";
        internal const string MessageUseSame = "defined in the overridden method";
        internal const string MessageRemoveExplicit = "from this explicit interface implementation";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var method = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(method);

                    var overriddenMember = methodSymbol.GetOverriddenMember() ?? methodSymbol.GetInterfaceMember();
                    if (methodSymbol == null ||
                        overriddenMember == null)
                    {
                        return;
                    }

                    for (var i = 0; i < methodSymbol.Parameters.Length; i++)
                    {
                        var overridingParameter = methodSymbol.Parameters[i];
                        var overriddenParameter = overriddenMember.Parameters[i];

                        var parameterSyntax = method.ParameterList.Parameters[i];

                        ReportParameterIfNeeded(c, overridingParameter, overriddenParameter, parameterSyntax, methodSymbol.ExplicitInterfaceImplementations.Any());
                    }
                },
                SyntaxKind.MethodDeclaration);
        }

        private static void ReportParameterIfNeeded(SonarSyntaxNodeReportingContext context, IParameterSymbol overridingParameter, IParameterSymbol overriddenParameter,
            ParameterSyntax parameterSyntax, bool isExplicitImplementation)
        {
            if (isExplicitImplementation)
            {
                if (overridingParameter.HasExplicitDefaultValue)
                {
                    context.ReportIssue(CreateDiagnostic(rule, parameterSyntax.Default.GetLocation(), "Remove", MessageRemoveExplicit));
                }

                return;
            }

            if (overridingParameter.HasExplicitDefaultValue &&
                !overriddenParameter.HasExplicitDefaultValue)
            {
                context.ReportIssue(CreateDiagnostic(rule, parameterSyntax.Default.GetLocation(), "Remove", MessageRemove));
                return;
            }

            if (!overridingParameter.HasExplicitDefaultValue &&
                overriddenParameter.HasExplicitDefaultValue)
            {
                context.ReportIssue(CreateDiagnostic(rule, parameterSyntax.Identifier.GetLocation(), "Add", MessageAdd));
                return;
            }

            if (overridingParameter.HasExplicitDefaultValue &&
                overriddenParameter.HasExplicitDefaultValue &&
                !Equals(overridingParameter.ExplicitDefaultValue, overriddenParameter.ExplicitDefaultValue))
            {
                context.ReportIssue(CreateDiagnostic(rule, parameterSyntax.Default.Value.GetLocation(), "Use", MessageUseSame));
            }
        }
    }
}
