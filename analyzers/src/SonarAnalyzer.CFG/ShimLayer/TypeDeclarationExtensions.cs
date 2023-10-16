// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class TypeDeclarationExtensions
    {
        private static readonly Func<TypeDeclarationSyntax, ParameterListSyntax> ParameterListAccessor;
        private static readonly Func<TypeDeclarationSyntax, ParameterListSyntax> RecordDeclarationParameterListAccessor;

        static TypeDeclarationExtensions()
        {
            ParameterListAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<TypeDeclarationSyntax, ParameterListSyntax>(typeof(TypeDeclarationSyntax), nameof(ParameterList));
            var recordDeclaration = typeof(ClassDeclarationSyntax).Assembly.GetType("Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax", throwOnError: false);
            if (recordDeclaration is not null)
            {
                RecordDeclarationParameterListAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<TypeDeclarationSyntax, ParameterListSyntax>(recordDeclaration, nameof(ParameterList));
            }
        }

        public static ParameterListSyntax ParameterList(this TypeDeclarationSyntax syntax)
        {
            if (syntax.Kind() is SyntaxKindEx.RecordClassDeclaration or SyntaxKindEx.RecordStructDeclaration)
            {
                return RecordDeclarationParameterListAccessor(syntax);
            }
            return ParameterListAccessor(syntax);
        }
    }
}
