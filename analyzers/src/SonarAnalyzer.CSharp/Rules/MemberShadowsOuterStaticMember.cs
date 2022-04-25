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
                                CheckFieldPropertyEventOrMethod(property, containerClassSymbol, c, "property");
                                break;
                            case IFieldSymbol field:
                                CheckFieldPropertyEventOrMethod(field, containerClassSymbol, c, "field");
                                break;
                            case IEventSymbol @event:
                                CheckFieldPropertyEventOrMethod(@event, containerClassSymbol, c, "event");
                                break;
                            case IMethodSymbol { MethodKind: MethodKind.DeclareMethod or MethodKind.Ordinary } method:
                                CheckFieldPropertyEventOrMethod(method, containerClassSymbol, c, "method");
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
                .Any(x => x is INamedTypeSymbol symbol && symbol.TypeKind is TypeKind.Class or TypeKind.Delegate);

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

        private static void CheckFieldPropertyEventOrMethod<T>(T member,
                                                    INamedTypeSymbol containerClassSymbol,
                                                    SymbolAnalysisContext context,
                                                    string memberType) where T : ISymbol
        {
            var selfAndOutterClasses = GetSelfAndOuterClasses(containerClassSymbol);
            var shadowsOthMember = selfAndOutterClasses
                .SelectMany(x => x.GetMembers(member.Name))
                .Any(x => x.IsStatic || x is IFieldSymbol { IsConst: true });

            if (shadowsOthMember
                && member.FirstDeclaringReferenceIdentifier() is { } identifier
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
    }
}
