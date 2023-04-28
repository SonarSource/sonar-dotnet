// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup
{
    public readonly struct CompilationOptionsWrapper
    {
        internal const string WrappedTypeName = "Microsoft.CodeAnalysis.CompilationOptions";
        private static readonly Type WrappedType;

        private static readonly Func<CompilationOptions, object> SyntaxTreeOptionsProviderAccessor;
        private readonly CompilationOptions node;

        static CompilationOptionsWrapper()
        {
            WrappedType = WrapperHelper.GetWrappedType(typeof(CompilationOptionsWrapper));
            SyntaxTreeOptionsProviderAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<CompilationOptions, object>(WrappedType, nameof(SyntaxTreeOptionsProvider));
        }

        private CompilationOptionsWrapper(CompilationOptions node) =>
            this.node = node;

        public SyntaxTreeOptionsProviderWrapper SyntaxTreeOptionsProvider =>
            node is null || WrappedType is null ? default : SyntaxTreeOptionsProviderWrapper.FromObject(SyntaxTreeOptionsProviderAccessor(node));

        public static CompilationOptionsWrapper FromObject(CompilationOptions node) =>
            node is null ? default : new(node);

        public static bool IsInstance(object obj) =>
            obj is not null && LightupHelpers.CanWrapObject(obj, WrappedType);
    }
}
