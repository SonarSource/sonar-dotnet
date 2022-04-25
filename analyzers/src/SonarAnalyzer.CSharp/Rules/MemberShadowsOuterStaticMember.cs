/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MemberShadowsOuterStaticMember : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3218";
        private const string MessageFormat = "Rename this {0} to not shadow the outer class' member with the same name.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(c =>
                {
                    var innerClassSymbol = (INamedTypeSymbol)c.Symbol;
                    var containerClassSymbol = innerClassSymbol.ContainingType;
                    if (!innerClassSymbol.IsClassOrStruct() || !containerClassSymbol.IsClassOrStruct())
                    {
                        return;
                    }

                    foreach (var member in innerClassSymbol.GetMembers().Where(x => !x.IsImplicitlyDeclared))
                    {
                        switch (member)
                        {
                            case IPropertySymbol property:
                                CheckFieldOrProperty(property, containerClassSymbol, c, "property");
                                break;
                            case IFieldSymbol field:
                                CheckFieldOrProperty(field, containerClassSymbol, c, "field");
                                break;
                            case IEventSymbol @event:
                                CheckEventOrMethod(@event, containerClassSymbol, c, "event");
                                break;
                            case IMethodSymbol method:
                                CheckEventOrMethod(method, containerClassSymbol, c, "method");
                                break;
                            case INamedTypeSymbol namedType:
                                CheckNamedType(c, containerClassSymbol, namedType);
                                break;
                        }
                    }
                },
                SymbolKind.NamedType);
        }

        private static void CheckNamedType(SymbolAnalysisContext context, INamedTypeSymbol containerClassSymbol, INamedTypeSymbol namedType)
        {
            var shadowsClassOrDelegate = GetSelfAndOuterClasses(containerClassSymbol)
                .SelectMany(x => x.GetMembers(namedType.Name))
                .OfType<INamedTypeSymbol>()
                .Any(x => x.Is(TypeKind.Class) || x.Is(TypeKind.Delegate));

            if (!shadowsClassOrDelegate)
            {
                return;
            }

            var kindName = namedType.TypeKind == TypeKind.Delegate ? "delegate" : "class";
            foreach (var identifier in namedType.DeclaringReferenceIdentifiers())
            {
                context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(Rule, identifier.GetLocation(), kindName));
            }
        }

        private static void CheckFieldOrProperty<T>(T propertyOrField,
                                                    INamedTypeSymbol containerClassSymbol,
                                                    SymbolAnalysisContext context,
                                                    string memberType) where T : ISymbol
        {
            var selfAndOutterClasses = GetSelfAndOuterClasses(containerClassSymbol);
            var shadowsPropertyOrField = selfAndOutterClasses
                .SelectMany(x => x.GetMembers(propertyOrField.Name))
                .Any(x => IsStaticOrConst(x));

            if (shadowsPropertyOrField
                && propertyOrField.FirstDeclaringReferenceIdentifier() is { } identifier
                && identifier.GetLocation() is { Kind: LocationKind.SourceFile } location)
            {
                context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(Rule, location, memberType));
            }
        }

        private static void CheckEventOrMethod<T>(T eventOrMethod,
                                                  INamedTypeSymbol containerClassSymbol,
                                                  SymbolAnalysisContext context,
                                                  string memberType) where T : ISymbol
        {
            if (eventOrMethod is IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove })
            {
                return;
            }

            var selfAndOutterClasses = GetSelfAndOuterClasses(containerClassSymbol);
            var shadowsMethodOrEvent = selfAndOutterClasses
                .SelectMany(x => x.GetMembers(eventOrMethod.Name))
                .Any(x => IsStaticOrConst(x));

            if (shadowsMethodOrEvent
                && eventOrMethod.FirstDeclaringReferenceIdentifier() is { } identifier
                && identifier.GetLocation() is { Kind: LocationKind.SourceFile } location)
            {
                context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(Rule, location, memberType));
            }
        }

        private static IReadOnlyList<INamedTypeSymbol> GetSelfAndOuterClasses(INamedTypeSymbol symbol)
        {
            var classes = new List<INamedTypeSymbol>();
            var currentClass = symbol;
            while (currentClass.IsClassOrStruct())
            {
                classes.Add(currentClass);
                currentClass = currentClass.ContainingType;
            }
            return classes;
        }

        private static bool IsStaticOrConst(ISymbol symbol)
            => symbol switch
            {
                IFieldSymbol field => field.IsStatic || field.IsConst,
                { } x => x.IsStatic,
            };
    }
}
