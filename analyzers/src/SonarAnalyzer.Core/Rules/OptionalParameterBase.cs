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
                    var symbol = c.Model.GetDeclaredSymbol(method);

                    if (symbol == null ||
                        !symbol.IsPubliclyAccessible() ||
                        symbol.GetInterfaceMember() != null ||
                        symbol.GetOverriddenMember() != null)
                    {
                        return;
                    }

                    var parameters = GetParameters(method);

                    foreach (var parameter in parameters.Where(p => IsOptional(p) && !HasAllowedAttribute(p, c.Model)))
                    {
                        var location = GetReportLocation(parameter);
                        c.ReportIssue(SupportedDiagnostics[0], location);
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
