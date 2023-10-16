// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    public static class TypeDeclarationExtensions
    {
        private static readonly Func<TypeDeclarationSyntax, ParameterListSyntax> ParameterListAccessor;

        static TypeDeclarationExtensions()
        {
            ParameterListAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<TypeDeclarationSyntax, ParameterListSyntax>(typeof(TypeDeclarationSyntax), nameof(ParameterList));
        }

        public static ParameterListSyntax ParameterList(this TypeDeclarationSyntax syntax)
        {
            return ParameterListAccessor(syntax);
        }
    }
}
