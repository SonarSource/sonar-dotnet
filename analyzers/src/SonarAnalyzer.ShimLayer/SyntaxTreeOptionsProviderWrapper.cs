// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup
{
    public readonly struct SyntaxTreeOptionsProviderWrapper
    {
        internal const string WrappedTypeName = "Microsoft.CodeAnalysis.SyntaxTreeOptionsProvider";
        private static readonly Type WrappedType;

        private static readonly TryGetValueAccessor<object, SyntaxTree, string, CancellationToken, ReportDiagnostic> TryGetDiagnosticValueAccessor;
        private static readonly TryGetValueAccessor<object, string, CancellationToken, ReportDiagnostic> TryGetGlobalDiagnosticValueAccessor;

        private readonly object node;

        static SyntaxTreeOptionsProviderWrapper()
        {
            WrappedType = WrapperHelper.GetWrappedType(typeof(SyntaxTreeOptionsProviderWrapper));

            TryGetDiagnosticValueAccessor = LightupHelpers.CreateTryGetValueAccessor<object, SyntaxTree, string, CancellationToken, ReportDiagnostic>(WrappedType, typeof(SyntaxTree),typeof(string), typeof(CancellationToken), nameof(TryGetDiagnosticValue));
            TryGetGlobalDiagnosticValueAccessor = LightupHelpers.CreateTryGetValueAccessor<object, string, CancellationToken, ReportDiagnostic>(WrappedType, typeof(string), typeof(CancellationToken), nameof(TryGetGlobalDiagnosticValue));
        }

        private SyntaxTreeOptionsProviderWrapper(object node) =>
            this.node = node;

        public static SyntaxTreeOptionsProviderWrapper FromObject(object node)
        {
            if (node is null)
            {
                return default;
            }
            else if (IsInstance(node))
            {
                return new SyntaxTreeOptionsProviderWrapper(node);
            }
            else
            {
                throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
            }
        }

        public static bool IsInstance(object obj) =>
            obj is not null && LightupHelpers.CanWrapObject(obj, WrappedType);

        public bool TryGetDiagnosticValue(SyntaxTree tree, string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
        {
            if (WrappedType is null)
            {
                severity = ReportDiagnostic.Default;
                return false;
            }
            else
            {
                return TryGetDiagnosticValueAccessor(node, tree, diagnosticId, cancellationToken, out severity);
            }
        }

        public bool TryGetGlobalDiagnosticValue(string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
        {
            if (WrappedType is null)
            {
                severity = ReportDiagnostic.Default;
                return false;
            }
            else
            {
                return TryGetGlobalDiagnosticValueAccessor(node, diagnosticId, cancellationToken, out severity);
            }
        }
    }
}
