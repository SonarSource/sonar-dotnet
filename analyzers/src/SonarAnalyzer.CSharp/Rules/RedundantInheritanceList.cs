﻿/*
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
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RedundantInheritanceList : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1939";
        internal const string RedundantIndexKey = "redundantIndex";
        private const string MessageEnum = "'int' should not be explicitly used as the underlying type.";
        private const string MessageObjectBase = "'Object' should not be explicitly extended.";
        private const string MessageAlreadyImplements = "'{0}' implements '{1}' so '{1}' can be removed from the inheritance list.";
        private const string MessageFormat = "{0}";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                if (c.IsRedundantPositionalRecordContext() || c.Node is not BaseTypeDeclarationSyntax { BaseList: { Types: { Count: > 0 } } })
                {
                    return;
                }
                switch (c.Node)
                {
                    case EnumDeclarationSyntax enumDeclaration:
                        ReportRedundantBaseType(c, enumDeclaration, KnownType.System_Int32, MessageEnum);
                        break;
                    case InterfaceDeclarationSyntax interfaceDeclaration:
                        ReportRedundantInterfaces(c, interfaceDeclaration);
                        break;
                    case TypeDeclarationSyntax nonInterfaceDeclaration:
                        ReportRedundantBaseType(c, nonInterfaceDeclaration, KnownType.System_Object, MessageObjectBase);
                        ReportRedundantInterfaces(c, nonInterfaceDeclaration);
                        break;
                }
            },
            SyntaxKind.EnumDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.RecordClassDeclaration,
            SyntaxKindEx.RecordStructDeclaration);

        private static void ReportRedundantBaseType(SyntaxNodeAnalysisContext context, BaseTypeDeclarationSyntax typeDeclaration, KnownType redundantType, string message)
        {
            var baseTypeSyntax = typeDeclaration.BaseList.Types.First().Type;
            if (context.SemanticModel.GetSymbolInfo(baseTypeSyntax).Symbol is ITypeSymbol baseTypeSymbol
                && baseTypeSymbol.Is(redundantType))
            {
                var location = GetLocationWithToken(baseTypeSyntax, typeDeclaration.BaseList.Types);
                context.ReportIssue(Diagnostic.Create(Rule, location, DiagnosticsProperties(redundantIndex: 0), message));
            }
        }

        private static void ReportRedundantInterfaces(SyntaxNodeAnalysisContext context, BaseTypeDeclarationSyntax typeDeclaration)
        {
            var declaredType = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
            if (declaredType is null)
            {
                return;
            }

            var baseList = typeDeclaration.BaseList;
            var interfaceTypesWithAllInterfaces = GetImplementedInterfaceMappings(baseList, context.SemanticModel);

            for (var i = 0; i < baseList.Types.Count; i++)
            {
                var baseType = baseList.Types[i];
                if (context.SemanticModel.GetSymbolInfo(baseType.Type).Symbol is INamedTypeSymbol interfaceType
                    && interfaceType.IsInterface()
                    && TryGetCollidingDeclaration(declaredType, interfaceType, interfaceTypesWithAllInterfaces, out var collidingDeclaration))
                {
                    var location = GetLocationWithToken(baseType.Type, baseList.Types);
                    var message = string.Format(MessageAlreadyImplements,
                        collidingDeclaration.ToMinimalDisplayString(context.SemanticModel, baseType.Type.SpanStart),
                        interfaceType.ToMinimalDisplayString(context.SemanticModel, baseType.Type.SpanStart));

                    context.ReportIssue(Diagnostic.Create(Rule, location, DiagnosticsProperties(redundantIndex: i), message));
                }
            }
        }

        private static ILookup<INamedTypeSymbol, INamedTypeSymbol> GetImplementedInterfaceMappings(BaseListSyntax baseList, SemanticModel semanticModel) =>
            baseList.Types
                    .Select(baseType => semanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol)
                    .WhereNotNull()
                    .Distinct()
                    .ToLookup(x => x.AllInterfaces.AsEnumerable());

        private static bool TryGetCollidingDeclaration(INamedTypeSymbol declaredType,
                                                       INamedTypeSymbol interfaceType,
                                                       ILookup<INamedTypeSymbol, INamedTypeSymbol> interfaceMappings,
                                                       out INamedTypeSymbol collidingDeclaration)
        {
            var collisionMapping = interfaceMappings.FirstOrDefault(x => x.Key.IsInterface() && x.Contains(interfaceType));
            if (collisionMapping?.Key is not null)
            {
                collidingDeclaration = collisionMapping.Key;
                return true;
            }

            var baseClassMapping = interfaceMappings.FirstOrDefault(x => x.Key.IsClass());
            if (baseClassMapping?.Key is null)
            {
                collidingDeclaration = null;
                return false;
            }

            var canBeRemoved = CanInterfaceBeRemovedBasedOnMembers(declaredType, interfaceType);
            collidingDeclaration = canBeRemoved ? baseClassMapping.Key : null;
            return canBeRemoved;
        }

        private static bool CanInterfaceBeRemovedBasedOnMembers(INamedTypeSymbol declaredType, INamedTypeSymbol interfaceType)
        {
            var allMembersOfInterface = AllInterfacesAndSelf(interfaceType)
                .SelectMany(x => x.GetMembers())
                .ToList();

            if (!allMembersOfInterface.Any())
            {
                return false;
            }

            foreach (var interfaceMember in allMembersOfInterface)
            {
                if (declaredType.FindImplementationForInterfaceMember(interfaceMember) is { } classMember
                    && (classMember.ContainingType.Equals(declaredType)
                        || !classMember.ContainingType.Interfaces.Any(x => AllInterfacesAndSelf(x).Contains(interfaceType))))
                {
                    return false;
                }
            }
            return true;

            static IEnumerable<INamedTypeSymbol> AllInterfacesAndSelf(INamedTypeSymbol interfaceType) =>
                interfaceType.AllInterfaces.Concat(new[] { interfaceType });
        }

        private static Location GetLocationWithToken(TypeSyntax type, SeparatedSyntaxList<BaseTypeSyntax> baseTypes)
        {
            var span = baseTypes.Count == 1 || baseTypes.First().Type != type
                ? TextSpan.FromBounds(type.GetFirstToken().GetPreviousToken().Span.Start, type.Span.End)
                : TextSpan.FromBounds(type.SpanStart, type.GetLastToken().GetNextToken().Span.End);

            return Location.Create(type.SyntaxTree, span);
        }

        private static ImmutableDictionary<string, string> DiagnosticsProperties(int redundantIndex)
        {
            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            builder.Add(RedundantIndexKey, redundantIndex.ToString());
            return builder.ToImmutable();
        }
    }
}
