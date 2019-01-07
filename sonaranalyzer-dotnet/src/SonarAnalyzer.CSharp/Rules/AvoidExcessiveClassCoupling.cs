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

using System.Collections.Generic;
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
    public sealed class AvoidExcessiveClassCoupling : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1200";
        private const string MessageFormat = "Split this {0} into smaller and more specialized ones to reduce its " +
            "dependencies on other classes from {1} to the maximum authorized {2} or less.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const int ThresholdDefaultValue = 30;
        [RuleParameter("max", PropertyType.Integer,
            "Maximum number of classes a single class is allowed to depend upon", ThresholdDefaultValue)]
        public int Threshold { get; set; } = ThresholdDefaultValue;

        private static readonly ImmutableArray<KnownType> ignoredTypes =
            ImmutableArray.Create(
                KnownType.Void,
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
                KnownType.System_Object,
                KnownType.System_Threading_Tasks_Task,
                KnownType.System_Threading_Tasks_Task_T,
                KnownType.System_Threading_Tasks_ValueTask_TResult,
                KnownType.System_Action,
                KnownType.System_Action_T,
                KnownType.System_Action_T1_T2,
                KnownType.System_Action_T1_T2_T3,
                KnownType.System_Action_T1_T2_T3_T4,
                KnownType.System_Func_TResult,
                KnownType.System_Func_T_TResult,
                KnownType.System_Func_T1_T2_TResult,
                KnownType.System_Func_T1_T2_T3_TResult,
                KnownType.System_Func_T1_T2_T3_T4_TResult,
                KnownType.System_Lazy
            );

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var typeDeclaration = (TypeDeclarationSyntax)c.Node;

                    if (typeDeclaration.Identifier.IsMissing)
                    {
                        return;
                    }

                    var type = c.SemanticModel.GetDeclaredSymbol(typeDeclaration);

                    var collector = new TypeDependencyCollector(c.SemanticModel, typeDeclaration);
                    collector.SafeVisit(typeDeclaration);

                    var dependentTypes = collector.DependentTypes
                        .SelectMany(ExpandGenericTypes)
                        .Distinct()
                        .Where(IsTrackedType)
                        .Where(t => t != type)
                        .ToList();

                    if (dependentTypes.Count > Threshold)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, typeDeclaration.Identifier.GetLocation(),
                            typeDeclaration.Keyword.ValueText, dependentTypes.Count, Threshold));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration);
        }

        private static bool IsTrackedType(INamedTypeSymbol namedType) =>
            namedType.TypeKind != TypeKind.Enum &&
            !namedType.IsAny(ignoredTypes);

        /// <summary>
        /// Returns all type symbols that are linked to the provided type symbol - generic constraints,
        /// replaced generic types (bounded types), etc. For example:
        /// void Foo(Dictionary<string, int>) will return Dictionary<T>, string and int
        /// void Foo<T>(List<T>) where T : IDisposable will return List<T> and Disposable
        /// void Foo<T>(Dictionary<string,List<T>>) where T : IEnumerable<int> should return Dictionary, string, List, IEnumerable and int
        /// </summary>
        private static IEnumerable<INamedTypeSymbol> ExpandGenericTypes(INamedTypeSymbol namedType)
        {
            var originalDefinition = new[] { namedType.OriginalDefinition };
            if (!namedType.IsGenericType)
            {
                return originalDefinition;
            }
            return namedType.IsUnboundGenericType
                ? originalDefinition.Union(namedType.TypeParameters.SelectMany(GetConstraintTypes))
                : originalDefinition.Union(namedType.TypeArguments.OfType<INamedTypeSymbol>().SelectMany(ExpandGenericTypes));
        }

        private static IEnumerable<INamedTypeSymbol> GetConstraintTypes(ITypeParameterSymbol typeParameter) =>
            typeParameter.ConstraintTypes.OfType<INamedTypeSymbol>().SelectMany(ExpandGenericTypes);

        private class TypeDependencyCollector : CSharpSyntaxWalker
        {
            private readonly SemanticModel model;
            private readonly TypeDeclarationSyntax originalTypeDeclaration;

            public ISet<INamedTypeSymbol> DependentTypes { get; } = new HashSet<INamedTypeSymbol>();

            private void AddDependentType(INamedTypeSymbol type)
            {
                if (type != null)
                {
                    DependentTypes.Add(type);
                }
            }

            private void AddDependentType(TypeSyntax typeSyntax)
            {
                if (typeSyntax != null)
                {
                    AddDependentType(this.model.GetSymbolInfo(typeSyntax).Symbol as INamedTypeSymbol);
                }
            }

            private void AddDependentType(TypeInfo typeInfo)
            {
                AddDependentType(typeInfo.Type as INamedTypeSymbol);
                AddDependentType(typeInfo.ConvertedType as INamedTypeSymbol);
            }

            public TypeDependencyCollector(SemanticModel model, TypeDeclarationSyntax originalTypeDeclaration)
            {
                this.model = model;
                this.originalTypeDeclaration = originalTypeDeclaration;
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                // don't drill down in child classes, but walk the original
                if (node == this.originalTypeDeclaration)
                {
                    base.VisitClassDeclaration(node);
                }
            }

            public override void VisitStructDeclaration(StructDeclarationSyntax node)
            {
                // don't drill down in child structs, but walk the original
                if (node == this.originalTypeDeclaration)
                {
                    base.VisitStructDeclaration(node);
                }
            }

            public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
            {
                AddDependentType(node.Type);
                base.VisitIndexerDeclaration(node);
            }

            public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                if (node.Initializer != null)
                {
                    AddDependentType(this.model.GetTypeInfo(node.Initializer.Value));
                }
                else
                {
                    AddDependentType(node.FirstAncestorOrSelf<VariableDeclarationSyntax>()?.Type);
                }
                base.VisitVariableDeclarator(node);
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                AddDependentType(node.ReturnType);
                base.VisitMethodDeclaration(node);
            }

            public override void VisitTypeConstraint(TypeConstraintSyntax node)
            {
                AddDependentType(node.Type);
                base.VisitTypeConstraint(node);
            }

            public override void VisitParameter(ParameterSyntax node)
            {
                AddDependentType(node.Type);
                base.VisitParameter(node);
            }

            public override void VisitEventDeclaration(EventDeclarationSyntax node)
            {
                AddDependentType(node.Type);
                base.VisitEventDeclaration(node);
            }

            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                AddDependentType(node.Type);
                base.VisitPropertyDeclaration(node);
            }

            public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
            {
                AddDependentType(this.model.GetTypeInfo(node));
                base.VisitObjectCreationExpression(node);
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                AddDependentType(node);
                base.VisitIdentifierName(node);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                // We don't use the helper method CSharpSyntaxHelper.IsNameof because it will do some extra
                // semantic checks to ensure this is the real `nameof` and not a user made method.
                // Here we prefer to favor fast results over accuracy (at worst we have FNs not FPs).
                var isNameof = node.Expression.IsKind(SyntaxKind.IdentifierName) &&
                    ((IdentifierNameSyntax)node.Expression).Identifier.ToString() == CSharpSyntaxHelper.NameOfKeywordText;

                if (!isNameof)
                {
                    base.VisitInvocationExpression(node);
                }
            }
        }
    }
}
