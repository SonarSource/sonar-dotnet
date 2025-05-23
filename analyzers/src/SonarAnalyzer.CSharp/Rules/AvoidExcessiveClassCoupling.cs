﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.Common.Walkers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidExcessiveClassCoupling : ParametrizedDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1200";
        private const string MessageFormat = "Split this {0} into smaller and more specialized ones to reduce its dependencies on other types from {1} to the maximum authorized {2} or less.";
        private const int ThresholdDefaultValue = 30;

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, isEnabledByDefault: false);
        private static readonly ImmutableArray<KnownType> IgnoredTypes =
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
                KnownType.System_Lazy);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        [RuleParameter("max", PropertyType.Integer, "Maximum number of types a single type is allowed to depend upon", ThresholdDefaultValue)]
        public int Threshold { get; set; } = ThresholdDefaultValue;

        protected override void Initialize(SonarParametrizedAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var typeDeclaration = (TypeDeclarationSyntax)c.Node;
                    if (typeDeclaration.Identifier.IsMissing || c.IsRedundantPositionalRecordContext())
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
                        .Where(t => !t.Equals(type))
                        .ToList();

                    if (dependentTypes.Count > Threshold)
                    {
                        c.ReportIssue(Rule, typeDeclaration.Identifier, typeDeclaration.Keyword.ValueText, dependentTypes.Count.ToString(), Threshold.ToString());
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKindEx.RecordDeclaration,
                SyntaxKindEx.RecordStructDeclaration);

        private static bool IsTrackedType(INamedTypeSymbol namedType) =>
            namedType.TypeKind != TypeKind.Enum && !namedType.IsAny(IgnoredTypes);

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

        private sealed class TypeDependencyCollector : SafeCSharpSyntaxWalker
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
                    AddDependentType(model.GetSymbolInfo(typeSyntax).Symbol as INamedTypeSymbol);
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

            // This override is needed because VisitRecordDeclaration is not available due to the Roslyn version.
            public override void Visit(SyntaxNode node)
            {
                if (node.IsKind(SyntaxKindEx.RecordDeclaration))
                {
                    if (node == originalTypeDeclaration)
                    {
                        base.Visit(node);
                    }
                }
                else
                {
                    base.Visit(node);
                }
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                // don't drill down in child classes, but walk the original
                if (node == originalTypeDeclaration)
                {
                    base.VisitClassDeclaration(node);
                }
            }

            public override void VisitStructDeclaration(StructDeclarationSyntax node)
            {
                // don't drill down in child structs, but walk the original
                if (node == originalTypeDeclaration)
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
                    AddDependentType(model.GetTypeInfo(node.Initializer.Value));
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
                AddDependentType(model.GetTypeInfo(node));
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
                var isNameof = node.Expression.IsKind(SyntaxKind.IdentifierName)
                    && ((IdentifierNameSyntax)node.Expression).Identifier.ToString() == CSharpSyntaxHelper.NameOfKeywordText;

                if (!isNameof)
                {
                    base.VisitInvocationExpression(node);
                }
            }
        }
    }
}
