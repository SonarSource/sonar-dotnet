/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.ControlFlowGraph;
using SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;
using SymbolWithInitializer = System.Collections.Generic.KeyValuePair<Microsoft.CodeAnalysis.ISymbol, Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax>;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MemberInitializerRedundant : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3604";
        private const string InstanceMemberMessage = "Remove the member initializer, all constructors set an initial value for the member.";
        private const string StaticMemberMessage = "Remove the static member initializer, a static constructor or module initializer sets an initial value for the member.";

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, "{0}", RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (TypeDeclarationSyntax)c.Node;
                    var symbol = c.SemanticModel.GetDeclaredSymbol(declaration);
                    var members = symbol?.GetMembers();
                    if (members == null || members.Value.Length == 0)
                    {
                        return;
                    }

                    InitializationVerifierFactory.InstanceMemberInitializationVerifier.Report(c, declaration, members);
                    InitializationVerifierFactory.StaticMemberInitializationVerifier.Report(c, declaration, members);
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKindEx.RecordDeclaration);

        private static class InitializationVerifierFactory
        {
            // Verifies the initializations done in instance constructors.
            public static readonly InitializationVerifier<ConstructorDeclarationSyntax> InstanceMemberInitializationVerifier =
                new InitializationVerifier<ConstructorDeclarationSyntax>
                {
                    // we are only interested in explicit constructors
                    InitializerFilter = method => method.MethodKind == MethodKind.Constructor
                                                  && !method.IsStatic
                                                  && !method.IsImplicitlyDeclared,
                    FieldFilter = field => !field.Modifiers.Any(IsStaticOrConst),
                    PropertyFilter = property => !property.Modifiers.Any(IsStaticOrConst),
                    IsSymbolFirstSetInInitializers = (fieldOrPropertySymbol, constructorDeclarations) =>
                        // the instance member should be initialized in ALL instance constructors
                        // otherwise, initializing it inline makes sense and the rule should not report
                        constructorDeclarations.All(constructor =>
                        {
                            if (constructor.Node.Initializer != null
                                && constructor.Node.Initializer.ThisOrBaseKeyword.IsKind(SyntaxKind.ThisKeyword))
                            {
                                // Calls another ctor, which is also checked.
                                return true;
                            }

                            return IsSymbolFirstSetInCfg(fieldOrPropertySymbol, constructor.Node, constructor.SemanticModel);
                        }),
                    DiagnosticMessage = InstanceMemberMessage
                };

            // Verifies the initializations done in static constructors and module initializers (C# 9).
            public static readonly InitializationVerifier<BaseMethodDeclarationSyntax> StaticMemberInitializationVerifier =
                new InitializationVerifier<BaseMethodDeclarationSyntax>
                {
                    InitializerFilter = method => method.MethodKind == MethodKind.StaticConstructor
                                                  || method.IsModuleInitializer(),
                    // only static members are interesting (const ones can only be initialized once)
                    FieldFilter = field => field.Modifiers.Any(IsStatic),
                    PropertyFilter = property => property.Modifiers.Any(IsStatic),
                    IsSymbolFirstSetInInitializers = (fieldOrPropertySymbol, initializerDeclarations) =>
                        // there can be only one static constructor
                        // all module initializers are executed when the type is created, so it is enough if one initializes the member
                        initializerDeclarations.Any(x => IsSymbolFirstSetInCfg(fieldOrPropertySymbol, x.Node, x.SemanticModel)),
                    DiagnosticMessage = StaticMemberMessage
                };

            private static bool IsStaticOrConst(SyntaxToken token) =>
                token.IsKind(SyntaxKind.StaticKeyword) || token.IsKind(SyntaxKind.ConstKeyword);

            private static bool IsStatic(SyntaxToken token) => token.IsKind(SyntaxKind.StaticKeyword);

            /// <summary>
            /// Returns true if the member is overwritten without being read in the instance constructor.
            /// Returns false if the member is not set in the constructor, or if it is read before being set.
            /// </summary>
            private static bool IsSymbolFirstSetInCfg(ISymbol classMember, BaseMethodDeclarationSyntax constructorOrInitializer, SemanticModel semanticModel)
            {
                var body = (CSharpSyntaxNode)constructorOrInitializer.Body ?? constructorOrInitializer.ExpressionBody();
                if (!CSharpControlFlowGraph.TryGet(body, semanticModel, out var cfg))
                {
                    return false;
                }

                var checker = new MemberInitializerRedundancyChecker(cfg, classMember, semanticModel);
                return checker.CheckAllPaths();
            }
        }

        private class InitializationVerifier<TInitializerDeclaration> where TInitializerDeclaration : SyntaxNode
        {
            public Func<IMethodSymbol, bool> InitializerFilter;
            public Func<BaseFieldDeclarationSyntax, bool> FieldFilter;
            public Func<PropertyDeclarationSyntax, bool> PropertyFilter;
            public Func<ISymbol, List<NodeSymbolAndSemanticModel<TInitializerDeclaration, IMethodSymbol>>, bool> IsSymbolFirstSetInInitializers;
            public string DiagnosticMessage;

            public void Report(SyntaxNodeAnalysisContext c, TypeDeclarationSyntax declaration, IEnumerable<ISymbol> typeMembers)
            {
                var typeInitializers = typeMembers.OfType<IMethodSymbol>().Where(InitializerFilter).ToList();
                if (typeInitializers.Count == 0)
                {
                    return;
                }

                // only retrieve the member symbols (an expensive call) if there are explicit class initializers
                var initializedMembers = GetInitializedMembers(c.SemanticModel, declaration);
                if (initializedMembers.Count == 0)
                {
                    return;
                }

                var initializerDeclarations = GetInitializerDeclarations<TInitializerDeclaration>(c, typeInitializers);
                foreach (var memberSymbol in initializedMembers.Keys)
                {
                    if (IsSymbolFirstSetInInitializers(memberSymbol, initializerDeclarations))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, initializedMembers[memberSymbol].GetLocation(), DiagnosticMessage));
                    }
                }
            }

            // Retrieves the class members which are initialized - instance or static ones, depending on the given filter.
            // The returned dictionary has as key the member symbol and as value the initialization syntax.
            private Dictionary<ISymbol, EqualsValueClauseSyntax> GetInitializedMembers(SemanticModel semanticModel,
                                                                                   TypeDeclarationSyntax declaration)
            {
                var candidateFields = GetInitializedFieldLikeDeclarations<FieldDeclarationSyntax, IFieldSymbol>(declaration, FieldFilter, semanticModel, f => f.Type);
                var candidateProperties = GetInitializedPropertyDeclarations(declaration, PropertyFilter, semanticModel);
                var candidateEvents = GetInitializedFieldLikeDeclarations<EventFieldDeclarationSyntax, IEventSymbol>(declaration, FieldFilter, semanticModel, f => f.Type);
                var allMembers = candidateFields.Select(t => new SymbolWithInitializer(t.Symbol, t.Initializer))
                    .Concat(candidateEvents.Select(t => new SymbolWithInitializer(t.Symbol, t.Initializer)))
                    .Concat(candidateProperties.Select(t => new SymbolWithInitializer(t.Symbol, t.Initializer)))
                    .ToDictionary(t => t.Key, t => t.Value);
                return allMembers;
            }

            private static List<NodeSymbolAndSemanticModel<TSyntax, IMethodSymbol>> GetInitializerDeclarations<TSyntax>(SyntaxNodeAnalysisContext context, List<IMethodSymbol> constructorSymbols)
                where TSyntax : SyntaxNode =>
                constructorSymbols
                    .Select(x => new NodeSymbolAndSemanticModel<TSyntax, IMethodSymbol>(null, x.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as TSyntax, x))
                    .Where(x => x.Node != null)
                    .Select(x => new NodeSymbolAndSemanticModel<TSyntax, IMethodSymbol>(x.Node.EnsureCorrectSemanticModelOrDefault(context.SemanticModel), x.Node, x.Symbol))
                    .Where(x => x.SemanticModel != null)
                    .ToList();

            private static IEnumerable<DeclarationTuple<IPropertySymbol>> GetInitializedPropertyDeclarations(TypeDeclarationSyntax declaration,
                                                                                                             Func<PropertyDeclarationSyntax, bool> declarationFilter,
                                                                                                             SemanticModel semanticModel) =>
                declaration.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .Where(p => declarationFilter(p) &&
                        p.Initializer != null &&
                        p.IsAutoProperty())
                    .Select(p =>
                        new DeclarationTuple<IPropertySymbol>
                        {
                            Initializer = p.Initializer,
                            SemanticModel = semanticModel,
                            Symbol = semanticModel.GetDeclaredSymbol(p)
                        })
                    .Where(t =>
                        t.Symbol != null &&
                        !MemberInitializedToDefault.IsDefaultValueInitializer(t.Initializer, t.Symbol.Type));

            private static IEnumerable<DeclarationTuple<TSymbol>> GetInitializedFieldLikeDeclarations<TDeclarationType, TSymbol>(TypeDeclarationSyntax declaration,
                                                                                                                                 Func<TDeclarationType, bool> declarationFilter,
                                                                                                                                 SemanticModel semanticModel,
                                                                                                                                 Func<TSymbol, ITypeSymbol> typeSelector)
                where TDeclarationType : BaseFieldDeclarationSyntax
                where TSymbol : class, ISymbol =>
                declaration.Members
                    .OfType<TDeclarationType>()
                    .Where(declarationFilter)
                    .SelectMany(fd => fd.Declaration.Variables
                        .Where(v => v.Initializer != null)
                        .Select(v =>
                            new DeclarationTuple<TSymbol>
                            {
                                Initializer = v.Initializer,
                                SemanticModel = semanticModel,
                                Symbol = semanticModel.GetDeclaredSymbol(v) as TSymbol
                            }))
                    .Where(t =>
                        t.Symbol != null &&
                        !MemberInitializedToDefault.IsDefaultValueInitializer(t.Initializer, typeSelector(t.Symbol)));
        }

        private class DeclarationTuple<TSymbol>
            where TSymbol : ISymbol
        {
            public EqualsValueClauseSyntax Initializer { get; set; }
            public SemanticModel SemanticModel { get; set; }
            public TSymbol Symbol { get; set; }
        }

        private class MemberInitializerRedundancyChecker : CfgAllPathValidator
        {
            private readonly ISymbol memberToCheck;
            private readonly SemanticModel semanticModel;

            public MemberInitializerRedundancyChecker(IControlFlowGraph cfg, ISymbol memberToCheck, SemanticModel semanticModel)
                : base(cfg)
            {
                this.memberToCheck = memberToCheck;
                this.semanticModel = semanticModel;
            }

            // Returns true if the block contains assignment before access
            protected override bool IsBlockValid(Block block)
            {
                foreach (var instruction in block.Instructions)
                {
                    switch (instruction.Kind())
                    {
                        case SyntaxKind.IdentifierName:
                        case SyntaxKind.SimpleMemberAccessExpression:
                            {
                                var memberAccess = GetPossibleMemberAccessParent(instruction);

                                if (memberAccess != null &&
                                    TryGetReadWriteFromMemberAccess(memberAccess, out var isRead))
                                {
                                    return !isRead;
                                }
                            }
                            break;
                        case SyntaxKind.SimpleAssignmentExpression:
                            {
                                var assignment = (AssignmentExpressionSyntax)instruction;
                                if (IsMatchingMember(assignment.Left.RemoveParentheses()))
                                {
                                    return true;
                                }
                            }
                            break;
                        default:
                            // continue search
                            break;
                    }
                }

                return false;
            }

            // Returns true if the block contains access before assignment
            protected override bool IsBlockInvalid(Block block)
            {
                foreach (var instruction in block.Instructions)
                {
                    switch (instruction.Kind())
                    {
                        case SyntaxKind.IdentifierName:
                        case SyntaxKind.SimpleMemberAccessExpression:
                            {
                                var memberAccess = GetPossibleMemberAccessParent(instruction);

                                if (memberAccess != null &&
                                    TryGetReadWriteFromMemberAccess(memberAccess, out var isRead))
                                {
                                    return isRead;
                                }
                            }
                            break;
                        case SyntaxKind.SimpleAssignmentExpression:
                            {
                                var assignment = (AssignmentExpressionSyntax)instruction;
                                if (IsMatchingMember(assignment.Left))
                                {
                                    return false;
                                }
                            }
                            break;

                        case SyntaxKind.AnonymousMethodExpression:
                        case SyntaxKind.ParenthesizedLambdaExpression:
                        case SyntaxKind.SimpleLambdaExpression:
                        case SyntaxKind.QueryExpression:
                            {
                                if (IsMemberUsedInsideLambda(instruction))
                                {
                                    return true;
                                }
                            }
                            break;
                        default:
                            // continue search
                            break;
                    }
                }

                return false;
            }

            private bool TryGetReadWriteFromMemberAccess(ExpressionSyntax expression, out bool isRead)
            {
                isRead = false;

                var parenthesized = expression.GetSelfOrTopParenthesizedExpression();

                if (!IsMatchingMember(expression))
                {
                    return false;
                }

                if (IsOutArgument(parenthesized))
                {
                    isRead = false;
                    return true;
                }

                if (IsReadAccess(parenthesized, this.semanticModel))
                {
                    isRead = true;
                    return true;
                }

                return false;
            }

            private static bool IsOutArgument(ExpressionSyntax parenthesized) =>
                parenthesized.Parent is ArgumentSyntax argument
                && argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword);

            private static bool IsReadAccess(ExpressionSyntax parenthesized, SemanticModel semanticModel) =>
                !IsBeingAssigned(parenthesized)
                && !parenthesized.IsInNameOfArgument(semanticModel);

            private bool IsMemberUsedInsideLambda(SyntaxNode instruction) =>
                instruction.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Select(i => GetPossibleMemberAccessParent(i))
                    .Any(i => IsMatchingMember(i));

            private static ExpressionSyntax GetPossibleMemberAccessParent(SyntaxNode node)
            {
                if (node is MemberAccessExpressionSyntax memberAccess)
                {
                    return memberAccess;
                }

                if (node is IdentifierNameSyntax identifier)
                {
                    return GetPossibleMemberAccessParent(identifier);
                }

                return null;
            }

            private static ExpressionSyntax GetPossibleMemberAccessParent(IdentifierNameSyntax identifier)
            {
                if (identifier.Parent is MemberAccessExpressionSyntax memberAccess)
                {
                    return memberAccess;
                }

                if (identifier.Parent is MemberBindingExpressionSyntax memberBinding)
                {
                    return (ExpressionSyntax)memberBinding.Parent;
                }

                return identifier;
            }

            private static bool IsBeingAssigned(ExpressionSyntax expression) =>
                expression.Parent is AssignmentExpressionSyntax assignment
                && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
                && assignment.Left == expression;

            private bool IsMatchingMember(ExpressionSyntax expression)
            {
                IdentifierNameSyntax identifier = null;

                if (expression.IsKind(SyntaxKind.IdentifierName))
                {
                    identifier = (IdentifierNameSyntax)expression;
                }

                if (expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                {
                    identifier = memberAccess.Name as IdentifierNameSyntax;
                }

                if (expression is ConditionalAccessExpressionSyntax conditionalAccess &&
                    conditionalAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                {
                    identifier = (conditionalAccess.WhenNotNull as MemberBindingExpressionSyntax)?.Name as IdentifierNameSyntax;
                }

                if (identifier == null)
                {
                    return false;
                }

                var assignedSymbol = semanticModel.GetSymbolInfo(identifier).Symbol;

                return memberToCheck.Equals(assignedSymbol);
            }
        }
    }
}
