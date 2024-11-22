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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseUnixEpoch : UseUnixEpochBase<SyntaxKind, LiteralExpressionSyntax, MemberAccessExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override bool IsDateTimeKindUtc(MemberAccessExpressionSyntax memberAccess) =>
        memberAccess.NameIs("Utc") && memberAccess.Expression.NameIs("DateTimeKind");

    protected override bool IsGregorianCalendar(SyntaxNode node) =>
        node is ObjectCreationExpressionSyntax objectCreation && objectCreation.Type.NameIs("GregorianCalendar");

    protected override bool IsZeroTimeOffset(SyntaxNode node) =>
        node switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.NameIs("Zero") && memberAccess.Expression.NameIs("TimeSpan"),
            ObjectCreationExpressionSyntax objectCreation => objectCreation.Type.NameIs("TimeSpan")
                                                             && objectCreation?.ArgumentList != null && objectCreation.ArgumentList.Arguments.Count is 1
                                                             && objectCreation.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax literal
                                                             && IsValueEqualTo(literal, 0),
            _ => false
        };
}
