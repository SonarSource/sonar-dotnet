// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SonarAnalyzer.ShimLayer.CSharp
{
    using System;
    using Microsoft.CodeAnalysis;

    public static class ITypeParameterSymbolExtensions
    {
        private static readonly Func<ITypeParameterSymbol, bool> HasUnmanagedTypeConstraintAccessor;

        static ITypeParameterSymbolExtensions()
        {
            HasUnmanagedTypeConstraintAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<ITypeParameterSymbol, bool>(typeof(ITypeParameterSymbol), nameof(HasUnmanagedTypeConstraint));
        }

        public static bool HasUnmanagedTypeConstraint(this ITypeParameterSymbol symbol)
        {
            return HasUnmanagedTypeConstraintAccessor(symbol);
        }
    }
}
