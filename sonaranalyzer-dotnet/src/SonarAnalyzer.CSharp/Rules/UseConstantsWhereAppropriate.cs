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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class UseConstantsWhereAppropriate : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3962";
        private const string MessageFormat = "Replace this 'static readonly' declaration with 'const'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
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
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;
                    var firstVariableWithInitialization = fieldDeclaration.Declaration.Variables
                        .FirstOrDefault(v => v.Initializer != null);
                    if (firstVariableWithInitialization == null)
                    {
                        return;
                    }

                    var fieldSymbol = c.SemanticModel.GetDeclaredSymbol(firstVariableWithInitialization)
                        as IFieldSymbol;
                    if (!IsFieldRelevant(fieldSymbol))
                    {
                        return;
                    }

                    var constValue = c.SemanticModel.GetConstantValue(
                        firstVariableWithInitialization.Initializer.Value);
                    if (!constValue.HasValue)
                    {
                        return;
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule,
                        firstVariableWithInitialization.Identifier.GetLocation()));
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
