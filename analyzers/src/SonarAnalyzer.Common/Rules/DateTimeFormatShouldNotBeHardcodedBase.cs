/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules;

public abstract class DateTimeFormatShouldNotBeHardcodedBase<TSyntaxKind, TInvocation> : DoNotCallMethodsBase<TSyntaxKind, TInvocation>
    where TSyntaxKind : struct
    where TInvocation : SyntaxNode
{
    private const string DiagnosticId = "S6585";
    private const string ToStringLiteral = "ToString";

    protected abstract bool IsMultiCharStringLiteral(TInvocation invocation, SemanticModel semanticModel);

    protected override string MessageFormat => "Do not hardcode the format specifier.";

    protected override IEnumerable<MemberDescriptor> CheckedMethods { get; } = new List<MemberDescriptor>
        {
            new(KnownType.System_DateTime, ToStringLiteral),
            new(KnownType.System_DateTimeOffset, ToStringLiteral),
            new(KnownType.System_DateOnly, ToStringLiteral),
            new(KnownType.System_TimeOnly, ToStringLiteral),
        };

    protected DateTimeFormatShouldNotBeHardcodedBase() : base(DiagnosticId) { }

    protected override bool ShouldReportOnMethodCall(TInvocation invocation, SemanticModel semanticModel, MemberDescriptor memberDescriptor, ISymbol methodCallSymbol) =>
        methodCallSymbol is IMethodSymbol methodSymbol
        && methodSymbol.Parameters.Count() >= 1
        && methodSymbol.Parameters[0].IsType(KnownType.System_String)
        && IsMultiCharStringLiteral(invocation, semanticModel);
}
