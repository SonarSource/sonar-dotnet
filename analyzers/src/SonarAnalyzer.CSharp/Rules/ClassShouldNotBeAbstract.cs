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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ClassShouldNotBeAbstract : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1694";
        private const string MessageFormat = "Convert this 'abstract' {0} to {1}.";
        private const string MessageToInterface = "an interface";
        private const string MessageToConcreteImplementation = "a concrete type with a protected constructor";

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    var symbol = (INamedTypeSymbol)c.Symbol;
                    if (symbol.Kind != SymbolKind.NamedType
                        || !symbol.IsClass()
                        || !symbol.IsAbstract
                        || HasInheritedAbstractMembers(symbol))
                    {
                        return;
                    }

                    if (!IsRecordWithParameters(symbol) && AbstractTypeShouldBeInterface(symbol))
                    {
                        Report(symbol, MessageToInterface, c);
                        return;
                    }

                    if (AbstractTypeShouldBeConcrete(symbol))
                    {
                        Report(symbol, MessageToConcreteImplementation, c);
                    }
                },
                SymbolKind.NamedType);

        private static bool HasInheritedAbstractMembers(INamedTypeSymbol symbol)
        {
            var baseTypes = symbol.BaseType.GetSelfAndBaseTypes().ToList();
            var abstractMethods = baseTypes.SelectMany(GetAllAbstractMethods);
            var baseTypesAndSelf = baseTypes.Concat(new[] { symbol }).ToList();
            var overrideMethods = baseTypesAndSelf.SelectMany(GetAllOverrideMethods);
            var overriddenMethods = overrideMethods.Select(m => m.OverriddenMethod);
            var stillAbstractMethods = abstractMethods.Except(overriddenMethods);

            return stillAbstractMethods.Any();
        }

        private static IEnumerable<IMethodSymbol> GetAllAbstractMethods(INamedTypeSymbol symbol) =>
            GetAllMethods(symbol).Where(m => m.IsAbstract);

        private static IEnumerable<IMethodSymbol> GetAllOverrideMethods(INamedTypeSymbol symbol) =>
            GetAllMethods(symbol).Where(m => m.IsOverride);

        private static void Report(INamedTypeSymbol symbol, string message, SymbolAnalysisContext context)
        {
            foreach (var declaringSyntaxReference in symbol.DeclaringSyntaxReferences)
            {
                var node = declaringSyntaxReference.GetSyntax();
                if (node is ClassDeclarationSyntax classDeclaration)
                {
                    context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), "class", message));
                }

                if (RecordDeclarationSyntaxWrapper.IsInstance(node))
                {
                    var wrapper = (RecordDeclarationSyntaxWrapper)node;
                    context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(Rule, wrapper.Identifier.GetLocation(), "record", message));
                }
            }
        }

        private static bool IsRecordWithParameters(ISymbol symbol) =>
            symbol.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() is { } node
                                                              && RecordDeclarationSyntaxWrapper.IsInstance(node)
                                                              && ((RecordDeclarationSyntaxWrapper)node).ParameterList is { } parameterList
                                                              && parameterList.Parameters.Count > 0);

        private static bool AbstractTypeShouldBeInterface(INamedTypeSymbol symbol)
        {
            var methods = GetAllMethods(symbol);
            return symbol.BaseType.Is(KnownType.System_Object)
                   && methods.Any()
                   && methods.All(method => method.IsAbstract);
        }

        private static bool AbstractTypeShouldBeConcrete(INamedTypeSymbol symbol)
        {
            var methods = GetAllMethods(symbol);
            return !methods.Any()
                   || methods.All(method => !method.IsAbstract);
        }

        private static IList<IMethodSymbol> GetAllMethods(INamedTypeSymbol symbol) =>
            symbol.GetMembers()
                  .OfType<IMethodSymbol>()
                  .Where(method => !method.IsImplicitlyDeclared && !ConstructorKinds.Contains(method.MethodKind))
                  .ToList();

        private static readonly ISet<MethodKind> ConstructorKinds = new HashSet<MethodKind>
        {
            MethodKind.Constructor,
            MethodKind.SharedConstructor
        };
    }
}
