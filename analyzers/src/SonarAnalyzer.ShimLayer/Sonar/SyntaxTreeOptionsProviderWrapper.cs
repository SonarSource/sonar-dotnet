/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace StyleCop.Analyzers.Lightup;

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
