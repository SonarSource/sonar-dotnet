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
using SonarAnalyzer;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class AvoidExcessiveClassCoupling : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1200";
        private const string MessageFormat = "Split this class into smaller and more specialized ones to reduce its " +
            "dependencies on other classes from {0} to the maximum authorized {1} or less.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private const int ThresholdDefaultValue = 20;
        [RuleParameter("max", PropertyType.Integer,
            "Maximum number of classes a single class is allowed to depend upon", ThresholdDefaultValue)]
        public int Threshold { get; set; } = ThresholdDefaultValue;

        private static ISet<KnownType> typesExcludedFromCoupling = new HashSet<KnownType>
        {
            KnownType.System_Boolean,
            KnownType.System_Byte,
            KnownType.System_SByte,
            KnownType.System_Int16,
            KnownType.System_UInt16,
            KnownType.System_Int32,
            KnownType.System_UInt32,
            KnownType.System_Int64,
            KnownType.System_UInt64,
            KnownType.System_IntPtr,
            KnownType.System_UIntPtr,
            KnownType.System_Char,
            KnownType.System_Single,
            KnownType.System_Double,
            KnownType.System_String,
            KnownType.System_Object
        };

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;

                    if (c.IsTest() ||
                        classDeclaration.Identifier.IsMissing)
                    {
                        return;
                    }

                    var allFieldsCoupling = classDeclaration.Members
                        .OfType<FieldDeclarationSyntax>()
                        .SelectMany(field => CollectCoupledClasses(field, c.SemanticModel));

                    var allPropertiesCoupling = classDeclaration.Members
                        .OfType<BasePropertyDeclarationSyntax>()
                        .SelectMany(property => CollectCoupledClasses(property, c.SemanticModel));

                    var allMethodsCoupling = classDeclaration.Members
                        .OfType<BaseMethodDeclarationSyntax>()
                        .SelectMany(baseMethod => CollectCoupledClasses(baseMethod, c.SemanticModel));

                    var totalClassCoupling = Enumerable.Empty<ITypeSymbol>()
                        .Union(allFieldsCoupling)
                        .Union(allPropertiesCoupling)
                        .Union(allMethodsCoupling);
                    var classCouplingCount = FilterAndCountCoupling(totalClassCoupling);

                    if (classCouplingCount > Threshold)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, classDeclaration.Identifier.GetLocation(),
                            classCouplingCount, Threshold));
                    }
                }, SyntaxKind.ClassDeclaration);
        }

        private static IEnumerable<ITypeSymbol> CollectCoupledClasses(FieldDeclarationSyntax field,
            SemanticModel model)
        {
            return field.Declaration.Variables
                .SelectMany(variable => ExtractTypesFromVariableDeclarator(variable, model));
        }

        private static IEnumerable<ITypeSymbol> CollectCoupledClasses(BasePropertyDeclarationSyntax property,
            SemanticModel model)
        {
            var propertyReturnType = new[] { model.GetSymbolInfo(property.Type).Symbol as ITypeSymbol };

            var indexerProperty = property as IndexerDeclarationSyntax;
            if (indexerProperty?.ExpressionBody != null)
            {
                // Arrowed property
                return propertyReturnType
                    .Union(indexerProperty.ExpressionBody
                        .DescendantNodes()
                        .SelectMany(node => ExtractTypeSymbolsFromSyntaxNode(node, model)));
            }

            var basicProperty = property as PropertyDeclarationSyntax;
            if (basicProperty?.ExpressionBody != null)
            {
                // Arrowed property
                return propertyReturnType
                    .Union(basicProperty.ExpressionBody
                        .DescendantNodes()
                        .SelectMany(node => ExtractTypeSymbolsFromSyntaxNode(node, model)));
            }

            // Classic property OR event
            return propertyReturnType
                .Union(property.AccessorList.Accessors
                    .SelectMany(accessor => accessor.DescendantNodes()
                        .SelectMany(node => ExtractTypeSymbolsFromSyntaxNode(node, model))));
        }

        private static IEnumerable<ITypeSymbol> CollectCoupledClasses(BaseMethodDeclarationSyntax method,
            SemanticModel model)
        {
            return method.DescendantNodes()
                .SelectMany(node => ExtractTypeSymbolsFromSyntaxNode(node, model));
        }

        private static IEnumerable<ITypeSymbol> ExtractTypesFromVariableDeclarator(VariableDeclaratorSyntax variable,
            SemanticModel model)
        {
            if (variable.Initializer != null)
            {
                var typeInfo = model.GetTypeInfo(variable.Initializer.Value);
                yield return typeInfo.Type;
                yield return typeInfo.ConvertedType;
            }
            else
            {
                var variableReturnType = variable.FirstAncestorOrSelf<VariableDeclarationSyntax>()?.Type;
                if (variableReturnType != null)
                {
                    yield return model.GetSymbolInfo(variableReturnType).Symbol as ITypeSymbol;
                }
            }
        }

        private static IEnumerable<ITypeSymbol> ExtractTypeSymbolsFromSyntaxNode(SyntaxNode node, SemanticModel model)
        {
            var parameter = node as ParameterSyntax;
            if (parameter?.Type != null)
            {
                yield return model.GetSymbolInfo(parameter.Type).Symbol as ITypeSymbol;
            }

            if (node is VariableDeclarationSyntax variableDeclaration)
            {
                yield return model.GetSymbolInfo(variableDeclaration.Type).Symbol as ITypeSymbol;
            }

            if (node is ObjectCreationExpressionSyntax objectCreation)
            {
                var objectCreationTypeInfo = model.GetTypeInfo(objectCreation);
                yield return objectCreationTypeInfo.Type;
                yield return objectCreationTypeInfo.ConvertedType;
            }

            if (node is IdentifierNameSyntax identifierName)
            {
                yield return model.GetSymbolInfo(identifierName).Symbol as ITypeSymbol;
            }
        }

        private static int FilterAndCountCoupling(IEnumerable<ITypeSymbol> types)
        {
            return types.Distinct().Count(IsCountedType);
        }

        private static bool IsCountedType(ITypeSymbol type)
        {
            return type != null &&
                type.TypeKind != TypeKind.Enum &&
                type.TypeKind != TypeKind.Pointer &&
                !type.IsAny(typesExcludedFromCoupling);
        }
    }
}