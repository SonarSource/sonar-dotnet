/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.Diagnostics.CodeAnalysis;

namespace SonarAnalyzer.Extensions // FIXME: file-scoped namespace
{
    internal static class ISymbolExtensions
    {
        public static bool HasAttribute(this ISymbol symbol, KnownType type) =>
            symbol.GetAttributes(type).Any();

        public static SyntaxNode GetFirstSyntaxRef(this ISymbol symbol) =>
            symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

        public static bool IsAutoProperty(this ISymbol symbol) =>
            symbol.Kind == SymbolKind.Property && symbol.ContainingType.GetMembers().OfType<IFieldSymbol>().Any(x => symbol.Equals(x.AssociatedSymbol));

        public static bool IsTopLevelMain(this ISymbol symbol) =>
            symbol is IMethodSymbol { Name: TopLevelStatements.MainMethodImplicitName };

        public static bool IsGlobalNamespace(this ISymbol symbol) =>
            symbol is INamespaceSymbol { Name: "" };

        public static bool IsInSameAssembly(this ISymbol symbol, ISymbol anotherSymbol) =>
            symbol.ContainingAssembly.Equals(anotherSymbol.ContainingAssembly);

        public static bool HasNotNullAttribute(this ISymbol parameter) =>
            parameter.GetAttributes() is { Length: > 0 } attributes && attributes.Any(IsNotNullAttribute);

        // https://docs.microsoft.com/dotnet/api/microsoft.validatednotnullattribute
        // https://docs.microsoft.com/dotnet/csharp/language-reference/attributes/nullable-analysis#postconditions-maybenull-and-notnull
        // https://www.jetbrains.com/help/resharper/Reference__Code_Annotation_Attributes.html#NotNullAttribute
        private static bool IsNotNullAttribute(AttributeData attribute) =>
            attribute.HasAnyName("ValidatedNotNullAttribute", "NotNullAttribute");

        // https://github.com/dotnet/roslyn/blob/2a594fa2157a734a988f7b5dbac99484781599bd/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/ISymbolExtensions.cs#L93
        [ExcludeFromCodeCoverage]
        public static ImmutableArray<ISymbol> ExplicitOrImplicitInterfaceImplementations(this ISymbol symbol)
        {
            if (symbol.Kind is not SymbolKind.Method and not SymbolKind.Property and not SymbolKind.Event)
            {
                return ImmutableArray<ISymbol>.Empty;
            }

            var containingType = symbol.ContainingType;
            var query = from iface in containingType.AllInterfaces
                        from interfaceMember in iface.GetMembers()
                        let impl = containingType.FindImplementationForInterfaceMember(interfaceMember)
                        where symbol.Equals(impl)
                        select interfaceMember;
            return query.ToImmutableArray();
        }

        public static bool HasContainingType(this ISymbol method, KnownType containingType, bool checkDerivedTypes) =>
            checkDerivedTypes
                ? method.ContainingType.DerivesOrImplements(containingType)
                : method.ContainingType.Is(containingType);

        public static bool IsInType(this ISymbol symbol, KnownType type) =>
            symbol is not null && symbol.ContainingType.Is(type);

        public static bool IsInType(this ISymbol symbol, ITypeSymbol type) =>
            symbol?.ContainingType is not null && symbol.ContainingType.Equals(type);

        public static bool IsInType(this ISymbol symbol, ImmutableArray<KnownType> types) =>
            symbol is not null && symbol.ContainingType.IsAny(types);
    }
}
