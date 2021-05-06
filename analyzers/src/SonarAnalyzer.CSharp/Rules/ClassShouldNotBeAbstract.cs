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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ClassShouldNotBeAbstract : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1694";
        private const string MessageFormat = "Convert this 'abstract' class to {0}.";
        private const string MessageToInterface = "an interface";
        private const string MessageToConcreteClass = "a concrete class with a protected constructor";

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    var symbol = c.Symbol as INamedTypeSymbol;
                    if (!symbol.IsClass()
                        || !symbol.IsAbstract
                        || ClassHasInheritedAbstractMembers(symbol))
                    {
                        return;
                    }

                    if (AbstractClassShouldBeInterface(symbol))
                    {
                        ReportClass(symbol, MessageToInterface, c);
                        return;
                    }

                    if (AbstractClassShouldBeConcreteClass(symbol))
                    {
                        ReportClass(symbol, MessageToConcreteClass, c);
                    }
                },
                SymbolKind.NamedType);

        private static bool ClassHasInheritedAbstractMembers(INamedTypeSymbol classSymbol)
        {
            var baseTypes = classSymbol.BaseType.GetSelfAndBaseTypes().ToList();
            var abstractMethods = baseTypes.SelectMany(GetAllAbstractMethods);
            var baseTypesAndSelf = baseTypes.Concat(new[] { classSymbol }).ToList();
            var overrideMethods = baseTypesAndSelf.SelectMany(GetAllOverrideMethods);
            var overriddenMethods = overrideMethods.Select(m => m.OverriddenMethod);
            var stillAbstractMethods = abstractMethods.Except(overriddenMethods);

            return stillAbstractMethods.Any();
        }

        private static IEnumerable<IMethodSymbol> GetAllAbstractMethods(INamedTypeSymbol classSymbol) =>
            GetAllMethods(classSymbol).Where(m => m.IsAbstract);

        private static IEnumerable<IMethodSymbol> GetAllOverrideMethods(INamedTypeSymbol classSymbol) =>
            GetAllMethods(classSymbol).Where(m => m.IsOverride);

        private static void ReportClass(INamedTypeSymbol symbol, string message, SymbolAnalysisContext context)
        {
            foreach (var declaringSyntaxReference in symbol.DeclaringSyntaxReferences)
            {
                if (declaringSyntaxReference.GetSyntax() is ClassDeclarationSyntax classDeclaration)
                {
                    context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), message));
                }
            }
        }

        private static bool AbstractClassShouldBeInterface(INamedTypeSymbol classSymbol)
        {
            var methods = GetAllMethods(classSymbol);
            return classSymbol.BaseType.Is(KnownType.System_Object)
                   && methods.Any()
                   && methods.All(method => method.IsAbstract);
        }

        private static bool AbstractClassShouldBeConcreteClass(INamedTypeSymbol classSymbol)
        {
            var methods = GetAllMethods(classSymbol);
            return !methods.Any()
                   || methods.All(method => !method.IsAbstract);
        }

        private static IList<IMethodSymbol> GetAllMethods(INamedTypeSymbol classSymbol) =>
            classSymbol.GetMembers()
                       .OfType<IMethodSymbol>()
                       .Where(method => !method.IsImplicitlyDeclared || !ConstructorKinds.Contains(method.MethodKind))
                       .ToList();

        private static readonly ISet<MethodKind> ConstructorKinds = new HashSet<MethodKind>
        {
            MethodKind.Constructor,
            MethodKind.SharedConstructor
        };
    }
}
