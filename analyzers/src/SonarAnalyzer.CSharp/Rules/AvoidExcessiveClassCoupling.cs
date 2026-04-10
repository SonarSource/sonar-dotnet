/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

                    var type = c.Model.GetDeclaredSymbol(typeDeclaration);
                    var collector = new TypeDependencyCollector(c.Model, typeDeclaration);
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
        /// Returns all type symbols reachable from the provided named type:
        /// the original generic definition, all containing types (recursively expanded),
        /// and all type arguments (recursively expanded) or constraint types for unbound generics.
        /// For example:
        /// Dictionary&lt;string, int&gt; returns Dictionary&lt;TKey,TValue&gt;, string, int
        /// List&lt;T&gt; where T : IDisposable returns List&lt;T&gt;, IDisposable
        /// Outer&lt;int&gt;.Inner returns Outer&lt;T&gt;.Inner, Outer&lt;T&gt;, int
        /// </summary>
        private static IEnumerable<INamedTypeSymbol> ExpandGenericTypes(INamedTypeSymbol namedType)
        {
            yield return namedType.OriginalDefinition;
            if (namedType.ContainingType is { } containing)
            {
                foreach (var expanded in ExpandGenericTypes(containing))
                {
                    yield return expanded;
                }
            }

            if (!namedType.IsGenericType)
            {
                yield break;
            }

            var typeArgs = namedType.IsUnboundGenericType
                ? namedType.TypeParameters.SelectMany(ConstraintTypes)
                : namedType.TypeArguments.OfType<INamedTypeSymbol>().SelectMany(ExpandGenericTypes);
            foreach (var expanded in typeArgs)
            {
                yield return expanded;
            }

            static IEnumerable<INamedTypeSymbol> ConstraintTypes(ITypeParameterSymbol typeParameter) =>
                typeParameter.ConstraintTypes.OfType<INamedTypeSymbol>().SelectMany(ExpandGenericTypes);
        }

        private sealed class TypeDependencyCollector : SafeCSharpSyntaxWalker
        {
            private readonly SemanticModel model;
            private readonly TypeDeclarationSyntax originalTypeDeclaration;

            public ISet<INamedTypeSymbol> DependentTypes { get; } = new HashSet<INamedTypeSymbol>();

            public TypeDependencyCollector(SemanticModel model, TypeDeclarationSyntax originalTypeDeclaration)
            {
                this.model = model;
                this.originalTypeDeclaration = originalTypeDeclaration;
            }

            // This override centralises all traversal guards:
            // - TypeSyntax subtrees are skipped entirely; type dependencies are collected at the parent level
            //   via node.TypeSyntax() + AddDependentType, avoiding O(n²) GetSymbolInfo calls on every identifier.
            // - Nested type declarations are not drilled into; each type is analysed independently.
            //   VisitRecordDeclaration / VisitRecordStructDeclaration are not available in this Roslyn version,
            //   so all type-declaration kinds are handled here uniformly via the facade's TypeDeclaration array.
            public override void Visit(SyntaxNode node)
            {
                if (node is TypeSyntax)
                {
                    return;
                }

                if (node != originalTypeDeclaration && CSharpFacade.Instance.SyntaxKind.TypeDeclaration.Contains(node.Kind()))
                {
                    return;
                }

                if (node.TypeSyntax() is { } typeSyntax)
                {
                    AddDependentType(typeSyntax);
                }

                base.Visit(node);
            }

            public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                if (node.Initializer is not null)
                {
                    AddDependentType(model.GetTypeInfo(node.Initializer.Value));
                }
                base.VisitVariableDeclarator(node);
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                // Only call GetSymbolInfo if the left-hand chain is a pure name chain (identifiers and member
                // accesses terminating at a TypeSyntax). Invocation results, conditionals, array elements,
                // this/base etc. can never be type references, so we skip the semantic lookup entirely.
                if (!IsSimpleNameChain(node.Expression))
                {
                    base.VisitMemberAccessExpression(node);
                    return;
                }

                if (model.GetSymbolInfo(node.Expression).Symbol is INamedTypeSymbol symbol)
                {
                    AddDependentType(symbol);
                    return;
                }
                base.VisitMemberAccessExpression(node);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (!node.IsNameof(model))
                {
                    // GenericNameSyntax is a TypeSyntax, so Visit skips it. We must handle type arguments explicitly.
                    if (node.Expression switch
                    {
                        GenericNameSyntax x => x,
                        MemberAccessExpressionSyntax { Name: GenericNameSyntax x } => x,
                        _ => null,
                    } is { TypeArgumentList.Arguments: { } typeArgs })
                    {
                        foreach (var typeArg in typeArgs)
                        {
                            AddDependentType(typeArg);
                        }
                    }
                    base.VisitInvocationExpression(node);
                }
            }

            // Returns true if the expression is a chain of MemberAccessExpressionSyntax nodes whose
            // leftmost element is a TypeSyntax (IdentifierName, QualifiedName, AliasQualifiedName, etc.).
            // This means the chain could be a qualified type reference like Console, NS.MyClass, Outer.Inner.
            // Any other terminal node (ThisExpression, BaseExpression, InvocationExpression, conditional, etc.)
            // cannot be a type, so GetSymbolInfo does not need to be called.
            private static bool IsSimpleNameChain(ExpressionSyntax expression)
            {
                var current = expression;
                while (current is MemberAccessExpressionSyntax ma)
                {
                    current = ma.Expression;
                }
                return current is TypeSyntax;
            }

            private void AddDependentType(TypeSyntax typeSyntax)
            {
                if (typeSyntax?.Unwrap() is not PredefinedTypeSyntax and { } unwrapped)
                {
                    AddDependentType(model.GetSymbolInfo(unwrapped).Symbol);
                }
            }

            private void AddDependentType(TypeInfo typeInfo)
            {
                AddDependentType(typeInfo.Type);
                AddDependentType(typeInfo.ConvertedType);
            }

            private bool AddDependentType(ISymbol symbol) =>
                symbol switch
                {
                    INamedTypeSymbol named => DependentTypes.Add(named),
                    IArrayTypeSymbol array => AddDependentType(array.ElementType),
                    IAliasSymbol alias => AddDependentType(alias.Target),
                    IPointerTypeSymbol pointer => AddDependentType(pointer.PointedAtType),
                    _ => false
                };
        }
    }
}
