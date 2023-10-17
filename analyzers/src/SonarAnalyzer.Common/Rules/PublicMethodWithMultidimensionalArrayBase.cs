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
    public abstract class PublicMethodWithMultidimensionalArrayBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S2368";
        protected const string MessageFormat = "Make this method private or simplify its parameters to not use multidimensional/jagged arrays.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class PublicMethodWithMultidimensionalArrayBase<TLanguageKindEnum, TMethodSyntax> : PublicMethodWithMultidimensionalArrayBase
        where TLanguageKindEnum : struct
        where TMethodSyntax : SyntaxNode
    {
        protected abstract ILanguageFacade<TLanguageKindEnum> LanguageFacade { get; }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    if (c.SemanticModel.GetDeclaredSymbol(c.Node) is IMethodSymbol methodSymbol &&
                        methodSymbol.GetInterfaceMember() == null &&
                        !methodSymbol.IsOverride &&
                        methodSymbol.IsPubliclyAccessible() &&
                        MethodHasMultidimensionalArrayParameters(methodSymbol) &&
                        LanguageFacade.Syntax.NodeIdentifier(c.Node) is { } identifier)
                    {
                        c.ReportIssue(Diagnostic.Create(SupportedDiagnostics[0], identifier.GetLocation()));
                    }
                },
                SyntaxKindsOfInterest.ToArray());
        }

        private static bool MethodHasMultidimensionalArrayParameters(IMethodSymbol methodSymbol) =>
            methodSymbol.Parameters.Any(param => param.Type is IArrayTypeSymbol arrayType
                                                 && (arrayType.Rank > 1
                                                     || IsJaggedArrayParam(param, arrayType)));

        private static bool IsJaggedArrayParam(IParameterSymbol param, IArrayTypeSymbol arrayType) =>
            param.IsParams
                ? arrayType.ElementType is IArrayTypeSymbol { ElementType: IArrayTypeSymbol }
                : arrayType.ElementType is IArrayTypeSymbol;

        public abstract ImmutableArray<TLanguageKindEnum> SyntaxKindsOfInterest { get; }
    }
}
