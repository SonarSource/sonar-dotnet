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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
    public sealed class StringOrIntegralTypesForIndexers : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3876";
        private const string MessageFormat = "Use string or an integral type here, or refactor this indexer into a method.";

        private static readonly ISet<KnownType> allowedIndexerTypes = new HashSet<KnownType>
        {
            KnownType.System_Object,
            KnownType.System_String
        };

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        static StringOrIntegralTypesForIndexers()
        {
            allowedIndexerTypes.UnionWith(KnownType.IntegralNumbers);
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var indexerDeclaration = (IndexerDeclarationSyntax)c.Node;
                if (indexerDeclaration.ParameterList.Parameters.Count != 1)
                {
                    return;
                }

                var parameter = indexerDeclaration.ParameterList.Parameters.First();

                var parameterSymbol = c.SemanticModel.GetDeclaredSymbol(parameter);
                if (parameterSymbol.Type == null ||
                    parameterSymbol.Type.TypeKind == TypeKind.Dynamic ||
                    parameterSymbol.Type.IsAny(allowedIndexerTypes) ||
                    parameterSymbol.IsParams ||
                    IsGenericTypeParameter(parameterSymbol))
                {
                    return;
                }

                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, parameter.Type.GetLocation()));
            },
            SyntaxKind.IndexerDeclaration);
        }

        private static bool IsGenericTypeParameter(IParameterSymbol parameterSymbol)
        {
            return parameterSymbol.ContainingType.ConstructedFrom.TypeParameters.Any(parameterSymbol.Type.Equals);
        }
    }
}
