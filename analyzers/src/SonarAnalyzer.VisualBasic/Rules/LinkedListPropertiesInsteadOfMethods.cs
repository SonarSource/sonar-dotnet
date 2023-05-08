﻿/*
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

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class LinkedListPropertiesInsteadOfMethods : LinkedListPropertiesInsteadOfMethodsBase<SyntaxKind, InvocationExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override bool TryGetOperands(InvocationExpressionSyntax invocation, out SyntaxNode left, out SyntaxNode right) =>
        invocation.TryGetOperands(out left, out right);

    protected override bool HasAnyArguments(InvocationExpressionSyntax invocation) => false;

    protected override bool IsCorrectType(InvocationExpressionSyntax invocation, SyntaxNode left, SemanticModel model) =>
        invocation is { ArgumentList: { Arguments: { Count: 1 } arg } }
        && model.GetTypeInfo(arg.First().GetExpression()).Type is { } type
        && type.DerivesFrom(KnownType.System_Collections_Generic_LinkedList_T);

    protected override void ReportIssue(SonarSyntaxNodeReportingContext node, string methodName) =>
        node.ReportIssue(Diagnostic.Create(Rule, node.Node.GetIdentifier()?.GetLocation(), methodName));
}
