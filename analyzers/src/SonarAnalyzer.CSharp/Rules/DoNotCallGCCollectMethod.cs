/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotCallGCCollectMethod : DoNotCallMethodsCSharpBase
{
    private const string DiagnosticId = "S1215";

    protected override string MessageFormat => "Refactor the code to remove this use of '{0}'.";

    protected override IEnumerable<MemberDescriptor> CheckedMethods { get; } =
    [
        new(KnownType.System_GC, nameof(GC.Collect)),
        new(KnownType.System_GC, nameof(GC.GetTotalMemory)),
    ];

    public DoNotCallGCCollectMethod() : base(DiagnosticId) { }

    protected override bool ShouldReportOnMethodCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, MemberDescriptor memberDescriptor) =>
        !IsGetTotalMemoryFalse(memberDescriptor, invocation); // Do not report on GC.TotalMemory(false)

    private static bool IsGetTotalMemoryFalse(MemberDescriptor memberDescriptor, InvocationExpressionSyntax invocation) =>
        memberDescriptor.Name is nameof(GC.GetTotalMemory) && invocation.ArgumentList.Arguments.First().Expression.IsKind(SyntaxKind.FalseLiteralExpression);
}
