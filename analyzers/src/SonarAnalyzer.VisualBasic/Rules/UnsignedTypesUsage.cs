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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class UnsignedTypesUsage : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2374";
        private const string MessageFormat = "Change this unsigned type to '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var typeSyntax = (TypeSyntax)c.Node;
                    if (typeSyntax.Parent is QualifiedNameSyntax)
                    {
                        return;
                    }

                    var typeSymbol = c.Model.GetSymbolInfo(typeSyntax).Symbol as ITypeSymbol;
                    if (typeSymbol.IsAny(KnownType.UnsignedIntegers))
                    {
                        c.ReportIssue(rule, typeSyntax, SignedPairs[typeSymbol.SpecialType]);
                    }
                },
                SyntaxKind.PredefinedType,
                SyntaxKind.IdentifierName,
                SyntaxKind.QualifiedName);
        }

        private static readonly IDictionary<SpecialType, string> SignedPairs =
            new Dictionary<SpecialType, string>
            {
                {SpecialType.System_UInt16, "Short"},
                {SpecialType.System_UInt32, "Integer"},
                {SpecialType.System_UInt64, "Long"}
            };
    }
}
