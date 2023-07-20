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

namespace SonarAnalyzer.Rules
{
    public abstract class OptionalParameterBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S2360";
        protected const string MessageFormat = "Use the overloading mechanism instead of the optional parameters.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class OptionalParameterBase<TLanguageKindEnum, TMethodSyntax, TParameterSyntax> : OptionalParameterBase
        where TLanguageKindEnum : struct
        where TMethodSyntax : SyntaxNode
        where TParameterSyntax: SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    var method = (TMethodSyntax)c.Node;
                    var symbol = c.SemanticModel.GetDeclaredSymbol(method);

                    if (symbol == null ||
                        !symbol.IsPubliclyAccessible() ||
                        symbol.GetInterfaceMember() != null ||
                        symbol.GetOverriddenMember() != null)
                    {
                        return;
                    }

                    var parameters = GetParameters(method);

                    foreach (var parameter in parameters.Where(p => IsOptional(p) && !HasAllowedAttribute(p, c.SemanticModel)))
                    {
                        var location = GetReportLocation(parameter);
                        c.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], location));
                    }
                },
                SyntaxKindsOfInterest.ToArray());
        }

        private static bool HasAllowedAttribute(TParameterSyntax parameterSyntax, SemanticModel semanticModel)
        {
            var parameterSymbol = semanticModel.GetDeclaredSymbol(parameterSyntax) as IParameterSymbol;

            return parameterSymbol.GetAttributes(KnownType.CallerInfoAttributes).Any();
        }

        protected abstract IEnumerable<TParameterSyntax> GetParameters(TMethodSyntax method);

        protected abstract bool IsOptional(TParameterSyntax parameter);

        protected abstract Location GetReportLocation(TParameterSyntax parameter);

        public abstract ImmutableArray<TLanguageKindEnum> SyntaxKindsOfInterest { get; }
    }
}
