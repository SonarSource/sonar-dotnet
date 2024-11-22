/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace StyleCop.Analyzers.Lightup;

public readonly struct CompilationOptionsWrapper
{
    internal const string WrappedTypeName = "Microsoft.CodeAnalysis.CompilationOptions";
    private static readonly Type WrappedType;

    private static readonly Func<CompilationOptions, object> SyntaxTreeOptionsProviderAccessor;
    private readonly CompilationOptions node;

    public SyntaxTreeOptionsProviderWrapper SyntaxTreeOptionsProvider =>
        node is null || WrappedType is null ? default : SyntaxTreeOptionsProviderWrapper.FromObject(SyntaxTreeOptionsProviderAccessor(node));

    static CompilationOptionsWrapper()
    {
        WrappedType = WrapperHelper.GetWrappedType(typeof(CompilationOptionsWrapper));
        SyntaxTreeOptionsProviderAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<CompilationOptions, object>(WrappedType, nameof(SyntaxTreeOptionsProvider));
    }

    private CompilationOptionsWrapper(CompilationOptions node) =>
        this.node = node;

    public static CompilationOptionsWrapper FromObject(CompilationOptions node) =>
        node is null ? default : new(node);

    public static bool IsInstance(object obj) =>
        obj is not null && LightupHelpers.CanWrapObject(obj, WrappedType);
}
