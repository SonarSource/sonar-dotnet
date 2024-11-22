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
