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
    public sealed class StringOrIntegralTypesForIndexers : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3876";
        private const string MessageFormat = "Use string, integral, index or range type here, or refactor this indexer into a method.";

        private static readonly ImmutableArray<KnownType> AllowedIndexerTypes =
            new[] { KnownType.System_Object, KnownType.System_String, KnownType.System_Index, KnownType.System_Range }
            .Concat(KnownType.IntegralNumbers)
            .ToImmutableArray();

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                var indexerDeclaration = (IndexerDeclarationSyntax)c.Node;
                if (indexerDeclaration.ParameterList.Parameters.Count != 1)
                {
                    return;
                }

                var parameter = indexerDeclaration.ParameterList.Parameters.First();

                var parameterSymbol = c.SemanticModel.GetDeclaredSymbol(parameter);
                if (parameterSymbol.Type == null
                    || parameterSymbol.Type.TypeKind == TypeKind.Dynamic
                    || parameterSymbol.Type.IsAny(AllowedIndexerTypes)
                    || parameterSymbol.IsParams
                    || IsGenericTypeParameter(parameterSymbol))
                {
                    return;
                }

                c.ReportIssue(CreateDiagnostic(Rule, parameter.Type.GetLocation()));
            },
            SyntaxKind.IndexerDeclaration);

        private static bool IsGenericTypeParameter(IParameterSymbol parameterSymbol) =>
            parameterSymbol.ContainingType.ConstructedFrom.TypeParameters.Any(parameterSymbol.Type.Equals);
    }
}
