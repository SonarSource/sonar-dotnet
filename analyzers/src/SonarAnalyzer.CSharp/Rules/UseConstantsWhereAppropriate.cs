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
    public sealed class UseConstantsWhereAppropriate : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3962";
        private const string MessageFormat = "Replace this 'static readonly' declaration with 'const'.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> RelevantTypes =
            ImmutableArray.Create(
                KnownType.System_Boolean,
                KnownType.System_Byte,
                KnownType.System_SByte,
                KnownType.System_Char,
                KnownType.System_Decimal,
                KnownType.System_Double,
                KnownType.System_Single,
                KnownType.System_Int32,
                KnownType.System_UInt32,
                KnownType.System_Int64,
                KnownType.System_UInt64,
                KnownType.System_Int16,
                KnownType.System_UInt16,
                KnownType.System_String
            );

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;
                    var firstVariableWithInitialization = fieldDeclaration.Declaration.Variables
                        .FirstOrDefault(v => v.Initializer != null);
                    if (firstVariableWithInitialization == null)
                    {
                        return;
                    }

                    var fieldSymbol = c.Model.GetDeclaredSymbol(firstVariableWithInitialization)
                        as IFieldSymbol;
                    if (!IsFieldRelevant(fieldSymbol))
                    {
                        return;
                    }

                    var constValue = c.Model.GetConstantValue(
                        firstVariableWithInitialization.Initializer.Value);
                    if (!constValue.HasValue)
                    {
                        return;
                    }

                    c.ReportIssue(rule, firstVariableWithInitialization.Identifier);
                },
                SyntaxKind.FieldDeclaration);
        }

        private static bool IsFieldRelevant(IFieldSymbol fieldSymbol)
        {
            return fieldSymbol != null &&
                   fieldSymbol.IsStatic &&
                   fieldSymbol.IsReadOnly &&
                   !fieldSymbol.IsPubliclyAccessible() &&
                   fieldSymbol.Type.IsAny(RelevantTypes);
        }
    }
}
