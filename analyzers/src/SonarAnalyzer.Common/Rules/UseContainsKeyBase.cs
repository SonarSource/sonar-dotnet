/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules;

public abstract class UseContainsKeyBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S4581"; // TODO: replace

    protected override string MessageFormat => "Use ContainsKey() instead.";

    protected UseContainsKeyBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, c =>
        {
            if (IsKeysProperty(Language.Syntax.NodeIdentifier(c.Node).Value.Text)
                && FollowedByContains(c.Node)
                && c.SemanticModel.GetSymbolInfo(c.Node).Symbol is IPropertySymbol property
                && property.IsInType(KnownType.System_Collections_Generic_Dictionary_TKey_TValue))
            {
                c.ReportIssue(Diagnostic.Create(Rule, CandidateNext(c.Node).GetLocation()));
            }
        },
        Language.SyntaxKind.IdentifierName);

    private bool IsKeysProperty(string name) =>
        nameof(Dictionary<int, int>.Keys).Equals(name, Language.NameComparison);

    private bool IsContainsMethod(string name) =>
        nameof(Enumerable.Contains).Equals(name, Language.NameComparison);

    private bool FollowedByContains(SyntaxNode node) =>
        CandidateNext(node) is { } next
        && IsContainsMethod(Language.Syntax.NodeIdentifier(next).Value.Text);

    private static SyntaxNode CandidateNext(SyntaxNode node) =>
        node.Parent.Parent.ChildNodes().SkipWhile(x => x != node.Parent).Skip(1).FirstOrDefault();
}
