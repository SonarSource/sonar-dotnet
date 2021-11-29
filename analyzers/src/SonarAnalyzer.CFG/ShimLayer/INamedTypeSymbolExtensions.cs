// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace StyleCop.Analyzers.Lightup
{
    public static class INamedTypeSymbolExtensions
    {
        public const string MainMethodImplicitName = "<Main>$";

        private static readonly HashSet<string> ProgramClassImplicitName = new HashSet<string> { "Program", "<Program>$" };
        private static readonly Func<INamedTypeSymbol, INamedTypeSymbol> TupleUnderlyingTypeAccessor;
        private static readonly Func<INamedTypeSymbol, ImmutableArray<IFieldSymbol>> TupleElementsAccessor;
        private static readonly Func<INamedTypeSymbol, bool> IsSerializableAccessor;

        static INamedTypeSymbolExtensions()
        {
            TupleUnderlyingTypeAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<INamedTypeSymbol, INamedTypeSymbol>(typeof(INamedTypeSymbol), nameof(TupleUnderlyingType));
            TupleElementsAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<INamedTypeSymbol, ImmutableArray<IFieldSymbol>>(typeof(INamedTypeSymbol), nameof(TupleElements));
            IsSerializableAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<INamedTypeSymbol, bool>(typeof(INamedTypeSymbol), nameof(IsSerializable));
        }

        public static INamedTypeSymbol TupleUnderlyingType(this INamedTypeSymbol symbol) => TupleUnderlyingTypeAccessor(symbol);

        public static ImmutableArray<IFieldSymbol> TupleElements(this INamedTypeSymbol symbol) => TupleElementsAccessor(symbol);

        public static bool IsSerializable(this INamedTypeSymbol symbol) => IsSerializableAccessor(symbol);

        public static bool IsTopLevelProgram(this INamedTypeSymbol symbol) =>
            ProgramClassImplicitName.Contains(symbol.Name)
            && symbol.ContainingNamespace.IsGlobalNamespace
            && symbol.GetMembers(MainMethodImplicitName).Any();
    }
}
