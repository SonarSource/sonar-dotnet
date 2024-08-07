// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /*
    internal static class TypeDeclarationExtensions
    {
        private static readonly Func<TypeDeclarationSyntax, ParameterListSyntax> ParameterListAccessor;
        private static readonly Func<TypeDeclarationSyntax, ParameterListSyntax> RecordDeclarationParameterListAccessor;

        static TypeDeclarationExtensions()
        {
            // TypeDeclarationSyntax.ParameterList was introduced in Roslyn 4.6 (C#12)
            ParameterListAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<TypeDeclarationSyntax, ParameterListSyntax>(typeof(TypeDeclarationSyntax), nameof(ParameterList));
            // In earlier versions, the ParameterList was only available on the derived RecordDeclarationSyntax (starting from version 3.7)
            // To work with version 3.7 to version 4.6 we need to special case the record declaration and access
            // the parameter list from the derived RecordDeclarationSyntax.
            var recordDeclaration = SyntaxWrapperHelper.GetWrappedType(typeof(RecordDeclarationSyntaxWrapper));
            RecordDeclarationParameterListAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<TypeDeclarationSyntax, ParameterListSyntax>(recordDeclaration, nameof(ParameterList));
        }

        public static ParameterListSyntax ParameterList(this TypeDeclarationSyntax syntax) =>
            syntax.Kind() is SyntaxKindEx.RecordDeclaration or SyntaxKindEx.RecordStructDeclaration
                ? RecordDeclarationParameterListAccessor(syntax)
                : ParameterListAccessor(syntax);
    }
    */
}
