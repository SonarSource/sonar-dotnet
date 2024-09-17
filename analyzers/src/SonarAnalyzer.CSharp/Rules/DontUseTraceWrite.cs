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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DontUseTraceWrite : DoNotCallMethodsBase<SyntaxKind, InvocationExpressionSyntax>
{
    private const string DiagnosticId = "S6670";

    protected override string MessageFormat => "Avoid using {0}, use instead methods that specify the trace event type.";

    protected override IEnumerable<MemberDescriptor> CheckedMethods => [
        new(KnownType.System_Diagnostics_Trace, "Write"),
        new(KnownType.System_Diagnostics_Trace, "WriteLine")
    ];

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override bool ShouldReportOnMethodCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, MemberDescriptor memberDescriptor)
    {
        var methodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invocation).Symbol;
        return !methodSymbol.Parameters.Any(x => x.Name == "category");
    }

    public DontUseTraceWrite() : base(DiagnosticId) { }
}
