/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class MutableFieldsShouldNotBe : SonarDiagnosticAnalyzer
    {
        protected const string MessageFormat = "Use an immutable collection or reduce the accessibility of this field.";

        private static readonly ISet<KnownType> InvalidMutableTypes = new HashSet<KnownType>
        {
            KnownType.System_Collections_Generic_ICollection_T,
            KnownType.System_Array
        };

        private static readonly ISet<KnownType> AllowedTypes = new HashSet<KnownType>
        {
            KnownType.System_Collections_ObjectModel_ReadOnlyCollection_T,
            KnownType.System_Collections_ObjectModel_ReadOnlyDictionary_TKey_TValue,
            KnownType.System_Collections_Immutable_IImmutableArray_T,
            KnownType.System_Collections_Immutable_IImmutableDictionary_TKey_TValue,
            KnownType.System_Collections_Immutable_IImmutableList_T,
            KnownType.System_Collections_Immutable_IImmutableSet_T,
            KnownType.System_Collections_Immutable_IImmutableStack_T,
            KnownType.System_Collections_Immutable_IImmutableQueue_T
        };

        protected abstract ISet<SyntaxKind> InvalidModifiers { get; }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(CheckForIssue, SyntaxKind.FieldDeclaration);
        }

        private void CheckForIssue(SyntaxNodeAnalysisContext analysisContext)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)analysisContext.Node;
            if (!IsFieldToAnalyze(fieldDeclaration))
            {
                return;
            }

            var symbolInfo = analysisContext.SemanticModel.GetSymbolInfo(fieldDeclaration.Declaration.Type);
            if (IsInvalidMutableType(symbolInfo.Symbol))
            {
                analysisContext.ReportDiagnostic(Diagnostic.Create(Rule, fieldDeclaration.Declaration.Variables[0].GetLocation()));
            }
        }

        private bool IsFieldToAnalyze(FieldDeclarationSyntax fieldDeclaration)
        {
            return fieldDeclaration.Modifiers.Count(m => InvalidModifiers.Contains(m.Kind())) == InvalidModifiers.Count;
        }

        private bool IsInvalidMutableType(ISymbol symbol)
        {
            var namedTypeSymbol = symbol as INamedTypeSymbol;
            if (namedTypeSymbol != null)
            {
                return IsOrDerivesOrImplementsAny(namedTypeSymbol.ConstructedFrom, InvalidMutableTypes) &&
                    !IsOrDerivesOrImplementsAny(namedTypeSymbol.ConstructedFrom, AllowedTypes);
            }

            var typeSymbol = symbol as ITypeSymbol;
            if (typeSymbol != null)
            {
                return IsOrDerivesOrImplementsAny(typeSymbol, InvalidMutableTypes) &&
                    !IsOrDerivesOrImplementsAny(typeSymbol, AllowedTypes);
            }

            return false;
        }

        private static bool IsOrDerivesOrImplementsAny(ITypeSymbol typeSymbol, ISet<KnownType> knownTypes)
        {
            return typeSymbol.IsAny(knownTypes) ||
                typeSymbol.DerivesOrImplementsAny(knownTypes);
        }
    }
}
