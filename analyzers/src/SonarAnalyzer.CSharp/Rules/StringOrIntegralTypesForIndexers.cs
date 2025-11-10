/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules
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

                var parameterSymbol = c.Model.GetDeclaredSymbol(parameter);
                if (parameterSymbol.Type == null
                    || parameterSymbol.Type.TypeKind == TypeKind.Dynamic
                    || parameterSymbol.Type.IsAny(AllowedIndexerTypes)
                    || parameterSymbol.IsParams
                    || IsGenericTypeParameter(parameterSymbol))
                {
                    return;
                }

                c.ReportIssue(Rule, parameter.Type);
            },
            SyntaxKind.IndexerDeclaration);

        private static bool IsGenericTypeParameter(IParameterSymbol parameterSymbol) =>
            parameterSymbol.ContainingType.ConstructedFrom.TypeParameters.Any(parameterSymbol.Type.Equals);
    }
}
