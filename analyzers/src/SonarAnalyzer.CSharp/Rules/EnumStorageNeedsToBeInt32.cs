﻿/*
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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EnumStorageNeedsToBeInt32 : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4022";
        private const string MessageFormat = "Change this enum storage to 'Int32'.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var enumDeclaration = (EnumDeclarationSyntax)c.Node;
                    var enumBaseType = enumDeclaration?.BaseList?.Types.FirstOrDefault()?.Type;

                    if (enumDeclaration != null &&
                        !IsDefaultOrLarger(enumBaseType, c.Model))
                    {
                        c.ReportIssue(rule, enumDeclaration.Identifier);
                    }
                },
                SyntaxKind.EnumDeclaration);
        }

        private static bool IsDefaultOrLarger(SyntaxNode syntaxNode, SemanticModel semanticModel)
        {
            if (syntaxNode == null)
            {
                return true;
            }

            var symbolType = semanticModel.GetSymbolInfo(syntaxNode).Symbol.GetSymbolType();
            return symbolType.IsAny(KnownType.System_Int32, KnownType.System_UInt32, KnownType.System_Int64, KnownType.System_UInt64);
        }
    }
}
