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

using System;
using System.Collections.Immutable;
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
    public sealed class PreferJaggedArraysOverMultidimensional : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3967";
        private const string MessageFormat = "Change this multidimensional array to a jagged array.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => AnalyzeNode<VariableDeclarationSyntax>(c,
                    (semanticModel, variable) => semanticModel.GetDeclaredSymbol(variable.Variables[0]).GetSymbolType(),
                    variable => variable.Variables[0].Identifier.GetLocation()),
                SyntaxKind.VariableDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => AnalyzeNode<ParameterSyntax>(c,
                    (semanticModel, parameter) => semanticModel.GetDeclaredSymbol(parameter)?.Type,
                    parameter => parameter.Identifier.GetLocation()),
                SyntaxKind.Parameter);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => AnalyzeNode<MethodDeclarationSyntax>(c,
                    (semanticModel, method) => semanticModel.GetDeclaredSymbol(method)?.ReturnType,
                    method => method.Identifier.GetLocation()),
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => AnalyzeNode<PropertyDeclarationSyntax>(c,
                    (semanticModel, property) => semanticModel.GetDeclaredSymbol(property)?.Type,
                    property => property.Identifier.GetLocation()),
                SyntaxKind.PropertyDeclaration);
        }

        private static void AnalyzeNode<TSyntax>(SyntaxNodeAnalysisContext context,
            Func<SemanticModel, TSyntax, ITypeSymbol> getTypeSymbol, Func<TSyntax, Location> getLocation)
            where TSyntax : SyntaxNode
        {
            var syntax = (TSyntax)context.Node;
            var typeSymbol = getTypeSymbol(context.SemanticModel, syntax);
            if (typeSymbol == null)
            {
                return;
            }

            if (IsMultiDimensionalArray(typeSymbol))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, getLocation(syntax)));
            }
        }

        private static bool IsMultiDimensionalArray(ITypeSymbol type)
        {
            var currentType = type;
            while (currentType.TypeKind == TypeKind.Array)
            {
                var arrayType = (IArrayTypeSymbol)currentType;

                if (arrayType.Rank > 1)
                {
                    return true;
                }

                currentType = arrayType.ElementType;
            }

            return false;
        }
    }
}
