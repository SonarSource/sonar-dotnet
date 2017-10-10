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

using System;
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
    public class MemberShadowsOuterStaticMember : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3218";
        private const string MessageFormat = "Rename this {0} to not shadow the outer class' member with the same name.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(
                c =>
                {
                    var innerClassSymbol = (INamedTypeSymbol)c.Symbol;
                    var containerClassSymbol = innerClassSymbol.ContainingType;
                    if (!innerClassSymbol.IsClass() ||
                        !containerClassSymbol.IsClass())
                    {
                        return;
                    }

                    var members = innerClassSymbol.GetMembers().Where(member => !member.IsImplicitlyDeclared);
                    foreach (var member in members)
                    {
                        var property = member as IPropertySymbol;
                        if (property != null)
                        {
                            CheckProperty(c, containerClassSymbol, property);
                            continue;
                        }

                        var field = member as IFieldSymbol;
                        if (field != null)
                        {
                            CheckField(c, containerClassSymbol, field);
                            continue;
                        }

                        var @event = member as IEventSymbol;
                        if (@event != null)
                        {
                            CheckEvent(c, containerClassSymbol, @event);
                            continue;
                        }

                        var method = member as IMethodSymbol;
                        if (method != null)
                        {
                            CheckMethod(c, containerClassSymbol, method);
                            continue;
                        }

                        var namedType = member as INamedTypeSymbol;
                        if (namedType != null)
                        {
                            CheckNamedType(c, containerClassSymbol, namedType);
                        }
                    }
                },
                SymbolKind.NamedType);
        }

        private static void CheckNamedType(SymbolAnalysisContext context, INamedTypeSymbol containerClassSymbol,
            INamedTypeSymbol namedType)
        {
            var shadowsClassOrDelegate = GetSelfAndOuterClasses(containerClassSymbol)
                .SelectMany(c => c.GetMembers(namedType.Name))
                .OfType<INamedTypeSymbol>()
                .Any(nt => nt.Is(TypeKind.Class) || nt.Is(TypeKind.Delegate));

            if (!shadowsClassOrDelegate)
            {
                return;
            }

            foreach (var reference in namedType.DeclaringSyntaxReferences)
            {
                var syntax = reference.GetSyntax();

                var delegateSyntax = syntax as DelegateDeclarationSyntax;
                if (delegateSyntax != null)
                {
                    context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule, delegateSyntax.Identifier.GetLocation(), "delegate"));
                    continue;
                }

                var classSyntax = syntax as ClassDeclarationSyntax;
                if (classSyntax != null)
                {
                    context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule, classSyntax.Identifier.GetLocation(), "class"));
                }
            }
        }

        private static void CheckMethod(SymbolAnalysisContext context, INamedTypeSymbol containerClassSymbol, IMethodSymbol method)
        {
            CheckEventOrMethod(method, containerClassSymbol, context,
                e =>
                {
                    var reference = e.DeclaringSyntaxReferences.FirstOrDefault();
                    var syntax = reference?.GetSyntax() as MethodDeclarationSyntax;
                    return syntax?.Identifier.GetLocation();
                },
                "method");
        }

        private static void CheckEvent(SymbolAnalysisContext context, INamedTypeSymbol containerClassSymbol, IEventSymbol @event)
        {
            CheckEventOrMethod(@event, containerClassSymbol, context,
                e =>
                {
                    var reference = e.DeclaringSyntaxReferences.FirstOrDefault();
                    if (reference == null)
                    {
                        return null;
                    }

                    var variableSyntax = reference.GetSyntax() as VariableDeclaratorSyntax;
                    if (variableSyntax != null)
                    {
                        return variableSyntax.Identifier.GetLocation();
                    }

                    var eventSyntax = reference.GetSyntax() as EventDeclarationSyntax;
                    return eventSyntax?.Identifier.GetLocation();
                },
                "event");
        }

        private static void CheckField(SymbolAnalysisContext context, INamedTypeSymbol containerClassSymbol, IFieldSymbol field)
        {
            CheckFieldOrProperty(field, containerClassSymbol, context,
                f =>
                {
                    var reference = f.DeclaringSyntaxReferences.FirstOrDefault();
                    var syntax = reference?.GetSyntax() as VariableDeclaratorSyntax;
                    return syntax?.Identifier.GetLocation();
                },
                "field");
        }

        private static void CheckProperty(SymbolAnalysisContext context, INamedTypeSymbol containerClassSymbol, IPropertySymbol property)
        {
            CheckFieldOrProperty(property, containerClassSymbol, context, p =>
            {
                var reference = p.DeclaringSyntaxReferences.FirstOrDefault();
                if (reference == null)
                {
                    return null;
                }
                var syntax = reference.GetSyntax() as PropertyDeclarationSyntax;
                if (syntax == null)
                {
                    return null;
                }
                return syntax.Identifier.GetLocation();
            }, "property");
        }

        private static void CheckFieldOrProperty<T>(T propertyOrField, INamedTypeSymbol containerClassSymbol,
            SymbolAnalysisContext context, Func<T, Location> locationSelector, string memberType) where T : ISymbol
        {
            var shadowsProperty = GetSelfAndOuterClasses(containerClassSymbol)
                .SelectMany(c => c.GetMembers(propertyOrField.Name))
                .OfType<IPropertySymbol>()
                .Any(prop => prop.IsStatic);
            var shadowsField = GetSelfAndOuterClasses(containerClassSymbol)
                .SelectMany(c => c.GetMembers(propertyOrField.Name))
                .OfType<IFieldSymbol>()
                .Any(field => field.IsStatic || field.IsConst);

            if (shadowsProperty || shadowsField)
            {
                var location = locationSelector(propertyOrField);
                if (location != null)
                {
                    context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule, location, memberType));
                }
            }
        }

        private static void CheckEventOrMethod<T>(T eventOrMethod, INamedTypeSymbol containerClassSymbol,
            SymbolAnalysisContext context, Func<T, Location> locationSelector, string memberType) where T : ISymbol
        {
            var shadowsMethod = GetSelfAndOuterClasses(containerClassSymbol)
                .SelectMany(c=> c.GetMembers(eventOrMethod.Name))
                .OfType<IMethodSymbol>()
                .Any(method => method.IsStatic);
            var shadowsEvent = GetSelfAndOuterClasses(containerClassSymbol)
                .SelectMany(c => c.GetMembers(eventOrMethod.Name))
                .OfType<IEventSymbol>()
                .Any(@event => @event.IsStatic);

            if (shadowsMethod || shadowsEvent)
            {
                var location = locationSelector(eventOrMethod);
                if (location != null)
                {
                    context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule, location, memberType));
                }
            }
        }

        private static IEnumerable<INamedTypeSymbol> GetSelfAndOuterClasses(INamedTypeSymbol symbol)
        {
            var classes = new List<INamedTypeSymbol>();
            var currentClass = symbol;
            while(currentClass.IsClass())
            {
                classes.Add(currentClass);
                currentClass = currentClass.ContainingType;
            }
            return classes;
        }
    }
}
