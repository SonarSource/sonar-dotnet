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
using System.Collections.Immutable;
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
        protected abstract DiagnosticDescriptor Rule { get; }

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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
            if (IsImmutableOrValidMutableType(symbolInfo.Symbol))
            {
                return;
            }

            var fieldFirstVariable = fieldDeclaration.Declaration.Variables[0];
            var fieldInitializer = fieldFirstVariable.Initializer;
            var fieldInitializerSymbol = GetFieldInitializerSymbol(fieldInitializer?.Value, analysisContext.SemanticModel);

            if (fieldDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ReadOnlyKeyword)) &&
                IsValidReadOnlyInitializer(fieldInitializerSymbol, fieldInitializer?.Value))
            {
                return;
            }

            analysisContext.ReportDiagnostic(Diagnostic.Create(Rule,fieldFirstVariable.GetLocation()));
        }

        private bool IsFieldToAnalyze(FieldDeclarationSyntax fieldDeclaration)
        {
            return fieldDeclaration.Modifiers.Count(m => InvalidModifiers.Contains(m.Kind())) == InvalidModifiers.Count;
        }

        private bool IsImmutableOrValidMutableType(ISymbol symbol)
        {
            var namedTypeSymbol = symbol as INamedTypeSymbol;
            if (namedTypeSymbol != null)
            {
                return !namedTypeSymbol.ConstructedFrom.DerivesOrImplementsAny(InvalidMutableTypes) ||
                    namedTypeSymbol.ConstructedFrom.DerivesOrImplementsAny(AllowedTypes);
            }

            var typeSymbol = symbol as ITypeSymbol;
            if (typeSymbol != null)
            {
                return !typeSymbol.DerivesOrImplementsAny(InvalidMutableTypes) ||
                    typeSymbol.DerivesOrImplementsAny(AllowedTypes);
            }

            return true;
        }

        private static ISymbol GetFieldInitializerSymbol(ExpressionSyntax initializer, SemanticModel model)
        {
            if (initializer == null)
            {
                return null;
            }

            var symbolInfo = model.GetSymbolInfo(initializer);

            return symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();
        }

        private static bool IsValidReadOnlyInitializer(ISymbol symbol, ExpressionSyntax equalsValue)
        {
            var methodSymbol = symbol as IMethodSymbol;
            var equalsLiteral = equalsValue as LiteralExpressionSyntax;

            return (methodSymbol != null && methodSymbol.ReturnType.DerivesOrImplementsAny(AllowedTypes)) ||
                   (equalsLiteral != null && equalsValue.IsKind(SyntaxKind.NullLiteralExpression));
        }
    }
}
