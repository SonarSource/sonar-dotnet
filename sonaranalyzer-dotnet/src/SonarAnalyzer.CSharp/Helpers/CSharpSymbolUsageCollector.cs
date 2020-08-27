/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

        private readonly Compilation compilation;
        private readonly HashSet<string> knownSymbolNames;

        public ISet<ISymbol> UsedSymbols { get; } = new HashSet<ISymbol>();

        [Flags]
        private enum SymbolAccess { None = 0, Read = 1, Write = 2, ReadWrite = Read | Write }

        public IDictionary<ISymbol, SymbolUsage> FieldSymbolUsages { get; } =
            new Dictionary<ISymbol, SymbolUsage>();

        public HashSet<string> DebuggerDisplayValues { get; } =
            new HashSet<string>();

        public Dictionary<IPropertySymbol, AccessorAccess> PropertyAccess { get; } =
            new Dictionary<IPropertySymbol, AccessorAccess>();

        public CSharpSymbolUsageCollector(Compilation compilation, IEnumerable<ISymbol> knownSymbols)
        {
            this.compilation = compilation;
            knownSymbolNames = knownSymbols.SelectMany(GetNames).ToHashSet();
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            var semanticModel = GetSemanticModel(node);
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

            static bool IsValueNameOrType(AttributeArgumentSyntax a) =>
                a.NameColon == null || // Value
                a.NameColon.Name.Identifier.ValueText == "Value" ||
                a.NameColon.Name.Identifier.ValueText == "Name" ||
                a.NameColon.Name.Identifier.ValueText == "Type";
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (HasKnownIdentifier(node))
            {
                var symbols = GetSymbols(node);
                TryStoreFieldAccess(node, symbols);
                UsedSymbols.UnionWith(symbols);
                TryStorePropertyAccess(node, symbols);
            }

            base.VisitIdentifierName(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            if (knownSymbolNames.Contains(node.Type.GetName()))
            {
                UsedSymbols.UnionWith(GetSymbols(node));
            }

            base.VisitObjectCreationExpression(node);
        }

        public override void VisitGenericName(GenericNameSyntax node)
        {
            if (HasKnownIdentifier(node))
            {
                UsedSymbols.UnionWith(GetSymbols(node));
            }

            base.VisitGenericName(node);
        }

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var symbols = GetSymbols(node);

            UsedSymbols.UnionWith(symbols);

            TryStorePropertyAccess(node, symbols);

            base.VisitElementAccessExpression(node);
        }

        public override void VisitConstructorInitializer(ConstructorInitializerSyntax node)
        {
            // In this case (":base()") we cannot check at the syntax level if the constructor name is in the list
            // of known names so we have to check for symbols.
            UsedSymbols.UnionWith(GetSymbols(node));

            base.VisitConstructorInitializer(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            // We are visiting a ctor with no initializer and the compiler will automatically
            // call the default constructor of the type if declared, or the base type if the
            // current type does not declare a default constructor.
            if (node.Initializer == null && IsKnownIdentifier(node.Identifier))
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

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            if (IsKnownIdentifier(node.Identifier))
            {
                var usage = GetFieldSymbolUsage(GetDeclaredSymbol(node));
                usage.Declaration = node;
                if (node.Initializer != null)
                {
                    usage.Initializer = node;
                }
            }

            base.VisitVariableDeclarator(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node.Initializer != null &&
                IsKnownIdentifier(node.Identifier))
            {
                var symbol = GetDeclaredSymbol(node);
                UsedSymbols.Add(symbol);
                StorePropertyAccess((IPropertySymbol)symbol, AccessorAccess.Set);
            }
            base.VisitPropertyDeclaration(node);
        }

        private SymbolAccess ParentAccessType(SyntaxNode node)
        {
            switch (node.Parent)
            {
                case ParenthesizedExpressionSyntax parenthesizedExpression:
                    // (node)
                    return ParentAccessType(parenthesizedExpression);
                case ExpressionStatementSyntax _:
                    // node;
                    return SymbolAccess.None;
                case InvocationExpressionSyntax invocation:
                    // node(_) : <unexpected>
                    return node == invocation.Expression ? SymbolAccess.Read : SymbolAccess.None;
                case MemberAccessExpressionSyntax memberAccess:
                    // _.node : node._
                    return node == memberAccess.Name ? ParentAccessType(memberAccess) : SymbolAccess.Read;
                case MemberBindingExpressionSyntax memberBinding:
                    // _?.node : node?._
                    return node == memberBinding.Name ? ParentAccessType(memberBinding) : SymbolAccess.Read;
                case AssignmentExpressionSyntax assignmentExpression:
                    // Ignoring distinction assignmentExpression.IsKind(SyntaxKind.SimpleAssignmentExpression) between
                    // "node = _" and "node += _" both are considered as Write and rely on the parent to know if its read.
                    //  node = _ : _ = node
                    return node == assignmentExpression.Left ? SymbolAccess.Write | ParentAccessType(assignmentExpression) : SymbolAccess.Read;
                case ArgumentSyntax argument:
                    if (argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword))
                    {
                        //  out Type node : out node
                        return SymbolAccess.Write;
                    }
                    else if (argument.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword))
                    {
                        //  ref node
                        return SymbolAccess.ReadWrite;
                    }
                    else
                    {
                        return SymbolAccess.Read;
                    }
                case ExpressionSyntax expressionSyntax when expressionSyntax.IsAnyKind(IncrementKinds):
                    // node++
                    return SymbolAccess.Write | ParentAccessType(expressionSyntax);
                case ArrowExpressionClauseSyntax arrowExpressionClause when arrowExpressionClause.Parent is MethodDeclarationSyntax arrowMethod:
                    return arrowMethod.ReturnType != null && arrowMethod.ReturnType.IsKnownType(KnownType.Void, GetSemanticModel(arrowMethod))
                        ? SymbolAccess.None
                        : SymbolAccess.Read;
                default:
                    return SymbolAccess.Read;
            }
        }

        /// <summary>
        /// Given a node, it tries to get the symbol or the candidate symbols (if the compiler cannot find the symbol,
        /// .e.g when the code cannot compile).
        /// </summary>
        /// <returns>List of symbols</returns>
        private ImmutableArray<ISymbol> GetSymbols<TSyntaxNode>(TSyntaxNode node)
            where TSyntaxNode : SyntaxNode
        {
            var symbolInfo = GetSemanticModel(node).GetSymbolInfo(node);

            return new[] { symbolInfo.Symbol }
                .Concat(symbolInfo.CandidateSymbols)
                .Select(GetOriginalDefinition)
                .WhereNotNull()
                .ToImmutableArray();

            static ISymbol GetOriginalDefinition(ISymbol candidateSymbol)
            {
                if (candidateSymbol is IMethodSymbol methodSymbol &&
                    methodSymbol.MethodKind == MethodKind.ReducedExtension)
                {
                    return methodSymbol.ReducedFrom?.OriginalDefinition;
                }
                return candidateSymbol?.OriginalDefinition;
            }
        }

        private void TryStorePropertyAccess(ExpressionSyntax node, IEnumerable<ISymbol> identifierSymbols)
        {
            var propertySymbols = identifierSymbols.OfType<IPropertySymbol>().ToList();
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
            if (node.IsInNameOfArgument(GetSemanticModel(node)))
            {
                return AccessorAccess.Both;
            }

            // Prop++ --> get/set
            return topmostSyntax.Parent.IsAnyKind(IncrementKinds)
                ? AccessorAccess.Both
                : AccessorAccess.Get;
        }

        private bool HasKnownIdentifier(SimpleNameSyntax nameSyntax) =>
            IsKnownIdentifier(nameSyntax.Identifier);

        private bool IsKnownIdentifier(SyntaxToken identifier) =>
            knownSymbolNames.Contains(identifier.ValueText);

        private ISymbol GetDeclaredSymbol(SyntaxNode syntaxNode) =>
            GetSemanticModel(syntaxNode).GetDeclaredSymbol(syntaxNode);

        private void TryStoreFieldAccess(IdentifierNameSyntax node, IEnumerable<ISymbol> symbols)
        {
            var access = ParentAccessType(node);
            var fieldSymbolUsagesList = GetFieldSymbolUsagesList(symbols);
            if (HasFlag(access, SymbolAccess.Read))
            {
                fieldSymbolUsagesList.ForEach(usage => usage.Readings.Add(node));
            }

            if (HasFlag(access, SymbolAccess.Write))
            {
                fieldSymbolUsagesList.ForEach(usage => usage.Writings.Add(node));
            }

            static bool HasFlag(SymbolAccess symbolAccess, SymbolAccess flag) => (symbolAccess & flag) != 0;
        }

        private List<SymbolUsage> GetFieldSymbolUsagesList(IEnumerable<ISymbol> symbols) =>
            symbols.Select(GetFieldSymbolUsage).ToList();

        private SymbolUsage GetFieldSymbolUsage(ISymbol symbol) =>
            FieldSymbolUsages.GetOrAdd(symbol, s => new SymbolUsage(s));

        private SemanticModel GetSemanticModel(SyntaxNode node) => compilation.GetSemanticModel(node.SyntaxTree);

        private static SyntaxNode GetTopmostSyntaxWithTheSameSymbol(SyntaxNode identifier)
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

        private static IMethodSymbol GetImplicitlyCalledConstructor(IMethodSymbol constructor)
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

        private static IMethodSymbol GetDefaultConstructor(INamedTypeSymbol namedType)
        {
            // See https://github.com/SonarSource/sonar-dotnet/issues/3155
            if (namedType != null && namedType.InstanceConstructors != null)
            {
                return namedType.InstanceConstructors.FirstOrDefault(IsDefaultConstructor);
            }
            return null;
        }

        private static bool IsDefaultConstructor(IMethodSymbol constructor) =>
            constructor.Parameters.Length == 0;

        private static IEnumerable<string> GetNames(ISymbol symbol)
        {
            if (symbol.IsConstructor())
            {
                yield return symbol.ContainingType.Name;
            }

            yield return symbol.Name;
        }
    }
}
