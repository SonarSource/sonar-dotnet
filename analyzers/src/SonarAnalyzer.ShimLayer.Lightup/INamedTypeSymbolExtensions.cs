// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup;

public static class INamedTypeSymbolExtensions
{
    private static readonly Func<INamedTypeSymbol, INamedTypeSymbol> TupleUnderlyingTypeAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<INamedTypeSymbol, INamedTypeSymbol>(typeof(INamedTypeSymbol), "TupleUnderlyingType");
    private static readonly Func<INamedTypeSymbol, ImmutableArray<IFieldSymbol>> TupleElementsAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<INamedTypeSymbol, ImmutableArray<IFieldSymbol>>(typeof(INamedTypeSymbol), "TupleElements");
    private static readonly Func<INamedTypeSymbol, bool> IsSerializableAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<INamedTypeSymbol, bool>(typeof(INamedTypeSymbol), "IsSerializable");
    private static readonly Func<INamedTypeSymbol, bool> IsExtensionAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<INamedTypeSymbol, bool>(typeof(INamedTypeSymbol), "IsExtension");

    extension(INamedTypeSymbol symbol)
    {
        public INamedTypeSymbol TupleUnderlyingType => TupleUnderlyingTypeAccessor(symbol);
        public ImmutableArray<IFieldSymbol> TupleElements => TupleElementsAccessor(symbol);
        public bool IsSerializable => IsSerializableAccessor(symbol);
        public bool IsExtension => IsExtensionAccessor(symbol);
    }
}
