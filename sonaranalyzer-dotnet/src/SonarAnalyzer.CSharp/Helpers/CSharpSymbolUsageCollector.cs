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
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Helpers
{
    /// <summary>
    /// Collects all symbol usages from a class declaration. Ignores symbols whose names are not present
    /// in the knownSymbolNames collection for performance reasons.
    /// </summary>
    internal class CSharpSymbolUsageCollector : CSharpSyntaxWalker
    {
        private static readonly ISet<SyntaxKind> IncrementKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.PostIncrementExpression,
            SyntaxKind.PreIncrementExpression,
            SyntaxKind.PostDecrementExpression,
            SyntaxKind.PreDecrementExpression
        };

        private readonly Func<SyntaxNode, SemanticModel> getSemanticModel;
        private readonly HashSet<string> knownSymbolNames;

        public HashSet<ISymbol> UsedSymbols { get; } =
            new HashSet<ISymbol>();

        public HashSet<string> DebuggerDisplayValues { get; } =
            new HashSet<string>();

        public Dictionary<IPropertySymbol, AccessorAccess> PropertyAccess { get; } =
            new Dictionary<IPropertySymbol, AccessorAccess>();

        public CSharpSymbolUsageCollector(Func<SyntaxTree, bool, SemanticModel> getSemanticModel, HashSet<string> knownSymbolNames)
        {
            this.getSemanticModel = node => getSemanticModel(node.SyntaxTree, false);
            this.knownSymbolNames = knownSymbolNames;
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            var semanticModel = this.getSemanticModel(node);
            var symbol = semanticModel.GetSymbolInfo(node).Symbol;
            if (symbol != null &&
                symbol.ContainingType.Is(KnownType.System_Diagnostics_DebuggerDisplayAttribute) &&
                node.ArgumentList != null)
            {
                var arguments = node.ArgumentList.Arguments
                    .Where(IsValueNameOrType)
                    .Select(a => semanticModel.GetConstantValue(a.Expression))
                    .Where(o => o.HasValue)
                    .Select(o => o.Value)
                    .OfType<string>();

                DebuggerDisplayValues.UnionWith(arguments);
            }

            base.VisitAttribute(node);

            bool IsValueNameOrType(AttributeArgumentSyntax a) =>
                a.NameColon == null || // Value
                a.NameColon.Name.Identifier.ValueText == "Value" ||
                a.NameColon.Name.Identifier.ValueText == "Name" ||
                a.NameColon.Name.Identifier.ValueText == "Type";
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbols = GetSymbols(node, IsKnownIdentifier).ToList();
            UsedSymbols.UnionWith(symbols);

            TryStorePropertyAccess(node, symbols);

            base.VisitIdentifierName(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            UsedSymbols.UnionWith(GetSymbols(node, x => true));
            base.VisitObjectCreationExpression(node);
        }

        public override void VisitGenericName(GenericNameSyntax node)
        {
            UsedSymbols.UnionWith(GetSymbols(node, IsKnownIdentifier));
            base.VisitGenericName(node);
        }

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var symbols = GetSymbols(node, x => true).ToList();
            UsedSymbols.UnionWith(symbols);

            TryStorePropertyAccess(node, symbols);

            base.VisitElementAccessExpression(node);
        }

        public override void VisitConstructorInitializer(ConstructorInitializerSyntax node)
        {
            UsedSymbols.UnionWith(GetSymbols(node, x => true));
            base.VisitConstructorInitializer(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            // We are visiting a ctor with no initializer and the compiler will automatically
            // call the default constructor of the type if declared, or the base type if the
            // current type does not declare a default constructor.
            if (node.Initializer == null)
            {
                var constructor = (IMethodSymbol)GetDeclaredSymbol(node);
                var implicitlyCalledConstructor = GetImplicitlyCalledConstructor(constructor);
                if (implicitlyCalledConstructor != null)
                {
                    UsedSymbols.Add(implicitlyCalledConstructor);
                }
            }

            base.VisitConstructorDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node.Initializer != null)
            {
                var symbol = GetDeclaredSymbol(node);
                UsedSymbols.Add(symbol);
                StorePropertyAccess((IPropertySymbol)symbol, AccessorAccess.Set);
            }
            base.VisitPropertyDeclaration(node);
        }

        private IMethodSymbol GetImplicitlyCalledConstructor(IMethodSymbol constructor)
        {
            // In case there is no other explicitly called constructor in a constructor declaration
            // the compiler will automatically put a call to the current class' default constructor,
            // or if the declaration is the default constructor or there is no default constructor,
            // the compiler will put a call the base class' default constructor.
            if (IsDefaultConstructor(constructor))
            {
                // Call default ctor of base type
                return GetDefaultConstructor(constructor.ContainingType.BaseType);
            }
            else
            {
                // Call default ctor of current type, or if undefined - default ctor of base type
                return GetDefaultConstructor(constructor.ContainingType)
                    ?? GetDefaultConstructor(constructor.ContainingType.BaseType);
            }
        }

        private static IMethodSymbol GetDefaultConstructor(INamedTypeSymbol namedType) =>
            namedType.InstanceConstructors.FirstOrDefault(IsDefaultConstructor);

        private static bool IsDefaultConstructor(IMethodSymbol constructor) =>
            constructor.Parameters.Length == 0;

        private IEnumerable<ISymbol> GetSymbols<TSyntaxNode>(TSyntaxNode node, Func<TSyntaxNode, bool> condition)
            where TSyntaxNode : SyntaxNode
        {
            return condition(node)
                ? GetCandidateSymbols(this.getSemanticModel(node).GetSymbolInfo(node))
                : Enumerable.Empty<ISymbol>();

            IEnumerable<ISymbol> GetCandidateSymbols(SymbolInfo symbolInfo)
            {
                return new[] { symbolInfo.Symbol }
                    .Concat(symbolInfo.CandidateSymbols)
                    .Select(GetOriginalDefinition)
                    .WhereNotNull();

                ISymbol GetOriginalDefinition(ISymbol candidateSymbol)
                {
                    if (candidateSymbol is IMethodSymbol methodSymbol &&
                        methodSymbol.MethodKind == MethodKind.ReducedExtension)
                    {
                        return methodSymbol.ReducedFrom?.OriginalDefinition;
                    }
                    return candidateSymbol?.OriginalDefinition;
                }
            }
        }

        private void TryStorePropertyAccess(ExpressionSyntax node, List<ISymbol> identifierSymbols)
        {
            var propertySymbols = identifierSymbols.OfType<IPropertySymbol>();
            if (propertySymbols.Any())
            {
                var access = EvaluatePropertyAccesses(node);
                foreach (var propertySymbol in propertySymbols)
                {
                    StorePropertyAccess(propertySymbol, access);
                }
            }
        }

        private void StorePropertyAccess(IPropertySymbol propertySymbol, AccessorAccess access)
        {
            if (PropertyAccess.ContainsKey(propertySymbol))
            {
                PropertyAccess[propertySymbol] |= access;
            }
            else
            {
                PropertyAccess[propertySymbol] = access;
            }
        }

        private AccessorAccess EvaluatePropertyAccesses(ExpressionSyntax node)
        {
            var topmostSyntax = GetTopmostSyntaxWithTheSameSymbol(node);

            if (topmostSyntax.Parent is AssignmentExpressionSyntax assignmentExpression)
            {
                if (assignmentExpression.IsKind(SyntaxKind.SimpleAssignmentExpression))
                {
                    // Prop = value --> set
                    // value = Prop --> get
                    return assignmentExpression.Left == topmostSyntax
                        ? AccessorAccess.Set
                        : AccessorAccess.Get;
                }
                else
                {
                    // Prop += value --> get/set
                    return AccessorAccess.Both;
                }
            }

            // nameof(Prop) --> get/set
            if (node.IsInNameofCall(this.getSemanticModel(node)))
            {
                return AccessorAccess.Both;
            }

            // Prop++ --> get/set
            return topmostSyntax.Parent.IsAnyKind(IncrementKinds)
                ? AccessorAccess.Both
                : AccessorAccess.Get;
        }

        private SyntaxNode GetTopmostSyntaxWithTheSameSymbol(SyntaxNode identifier)
        {
            // All of the cases below could be parts of invocation or other expressions
            switch (identifier.Parent)
            {
                case MemberAccessExpressionSyntax memberAccess when memberAccess.Name == identifier:
                    // this.identifier or a.identifier or ((a)).identifier, but not identifier.other
                    return memberAccess.GetSelfOrTopParenthesizedExpression();
                case MemberBindingExpressionSyntax memberBinding when memberBinding.Name == identifier:
                    // this?.identifier or a?.identifier or ((a))?.identifier, but not identifier?.other
                    return memberBinding.Parent.GetSelfOrTopParenthesizedExpression();
                default:
                    // identifier or ((identifier))
                    return identifier.GetSelfOrTopParenthesizedExpression();
            }
        }

        private bool IsKnownIdentifier(SimpleNameSyntax nameSyntax) =>
            this.knownSymbolNames.Contains(nameSyntax.Identifier.ValueText);

        private ISymbol GetDeclaredSymbol(SyntaxNode syntaxNode) =>
            this.getSemanticModel(syntaxNode).GetDeclaredSymbol(syntaxNode);
    }
}
