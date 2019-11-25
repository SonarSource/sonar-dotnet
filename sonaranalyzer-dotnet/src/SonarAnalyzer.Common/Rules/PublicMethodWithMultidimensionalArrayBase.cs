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
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class PublicMethodWithMultidimensionalArrayBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S2368";
        protected const string MessageFormat = "Make this method private or simplify its parameters to not use multidimensional arrays.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class PublicMethodWithMultidimensionalArrayBase<TLanguageKindEnum, TMethodSyntax> : PublicMethodWithMultidimensionalArrayBase
        where TLanguageKindEnum : struct
        where TMethodSyntax : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var method = (TMethodSyntax)c.Node;

                    if (!(c.SemanticModel.GetDeclaredSymbol(method) is IMethodSymbol methodSymbol) ||
                        methodSymbol.GetInterfaceMember() != null ||
                        methodSymbol.GetOverriddenMember() != null ||
                        !methodSymbol.IsPubliclyAccessible() ||
                        !MethodHasMultidimensionalArrayParameters(methodSymbol))
                    {
                        return;
                    }

                    var identifier = GetIdentifier(method);
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], identifier.GetLocation()));
                },
                SyntaxKindsOfInterest.ToArray());
        }

        private static bool MethodHasMultidimensionalArrayParameters(IMethodSymbol methodSymbol)
        {
            return methodSymbol.Parameters
                .Select(param => param.Type as IArrayTypeSymbol)
                .WhereNotNull()
                .Any(type => type.Rank > 1 || type.ElementType is IArrayTypeSymbol);
        }

        protected abstract SyntaxToken GetIdentifier(TMethodSyntax method);

        public abstract ImmutableArray<TLanguageKindEnum> SyntaxKindsOfInterest { get; }
    }
}
