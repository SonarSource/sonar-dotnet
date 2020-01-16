// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SonarAnalyzer.ShimLayer.CSharp
{
    using System;
    using Microsoft.CodeAnalysis;

    public static class ITypeSymbolExtensions
    {
        private static readonly Func<ITypeSymbol, bool> IsTupleTypeAccessor;
        private static readonly Func<ITypeSymbol, bool> IsRefLikeTypeAccessor;

        static ITypeSymbolExtensions()
        {
            IsTupleTypeAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<ITypeSymbol, bool>(typeof(ITypeSymbol), nameof(IsTupleType));
            IsRefLikeTypeAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<ITypeSymbol, bool>(typeof(ITypeSymbol), nameof(IsRefLikeType));
        }

        public static bool IsTupleType(this ITypeSymbol symbol)
        {
            return IsTupleTypeAccessor(symbol);
        }

        public static bool IsRefLikeType(this ITypeSymbol symbol)
        {
            return IsRefLikeTypeAccessor(symbol);
        }
    }
}
