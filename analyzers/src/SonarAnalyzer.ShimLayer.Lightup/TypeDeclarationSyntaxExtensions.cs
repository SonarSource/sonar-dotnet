// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup;

using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static partial class TypeDeclarationSyntaxExtensions
{
    private static readonly Func<TypeDeclarationSyntax, ParameterListSyntax?> ParameterListAccessor;
    private static readonly Func<TypeDeclarationSyntax, ParameterListSyntax?, TypeDeclarationSyntax> WithParameterListAccessor;

    static TypeDeclarationSyntaxExtensions()
    {
        ParameterListAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<TypeDeclarationSyntax, ParameterListSyntax>(typeof(TypeDeclarationSyntax), nameof(ParameterList));
        WithParameterListAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<TypeDeclarationSyntax, ParameterListSyntax?>(typeof(TypeDeclarationSyntax), nameof(ParameterList));

        var recordDeclaration = SyntaxWrapperHelper.GetWrappedType(typeof(RecordDeclarationSyntaxWrapper)); // Sonar
        RecordDeclarationParameterListAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<TypeDeclarationSyntax, ParameterListSyntax>(recordDeclaration, nameof(ParameterList)); // Sonar
    }

    public static TypeDeclarationSyntax WithParameterList(this TypeDeclarationSyntax syntax, ParameterListSyntax? parameterList)
    {
        return WithParameterListAccessor(syntax, parameterList);
    }
}
