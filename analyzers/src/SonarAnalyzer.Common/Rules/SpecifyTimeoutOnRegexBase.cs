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
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules;

public abstract class SpecifyTimeoutOnRegexBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
{
    internal const string DiagnosticId = "S4581"; // TODO

    private const int NonBacktracking = 1024;

    protected override string MessageFormat => "Pass a timeout to limit the execution time.";

    protected SpecifyTimeoutOnRegexBase() : base(DiagnosticId) { }
    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterSyntaxNodeActionInNonGenerated(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (IsCandidateCtor(c.Node)
                    && RegexMethodLacksTimeout(c.Node, c.SemanticModel))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation()));
                }
            },
            Language.SyntaxKind.ObjectCreationExpressions);

        context.RegisterSyntaxNodeActionInNonGenerated(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (IsRegexMatchMethod(Language.Syntax.NodeIdentifier(c.Node).Value.Text)
                    && RegexMethodLacksTimeout(c.Node, c.SemanticModel))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation()));
                }
            },
            Language.SyntaxKind.IdentifierName);
    }

    private bool RegexMethodLacksTimeout(SyntaxNode node, SemanticModel model) =>
        model.GetSymbolInfo(node).Symbol is IMethodSymbol method
        && method.ContainingType.Is(KnownType.System_Text_RegularExpressions_Regex)
        && (method.IsStatic || method.IsConstructor())
        && !ContainsMatchTimeout(method)
        && !NoBacktracking(method, node, model);

    private static bool ContainsMatchTimeout(IMethodSymbol method) =>
        method.Parameters.Any(x => x.Type.Is(KnownType.System_TimeSpan));

    private bool NoBacktracking(IMethodSymbol method, SyntaxNode node, SemanticModel model) =>
        RegexOptionsSpecified(method)
        && ArgumentExpressions(method, node)
            .Select(arg => Language.FindConstantValue(model, arg))
            .OfType<int>()
            .Any(x => (x & NonBacktracking) == NonBacktracking);

    private IEnumerable<SyntaxNode> ArgumentExpressions(IMethodSymbol method, SyntaxNode node) =>
           Language.Syntax.ArgumentExpressions(method.IsConstructor()
           ? node
           : node.Parent.Parent.ChildNodes().Skip(1).FirstOrDefault());

    private static bool RegexOptionsSpecified(IMethodSymbol method) =>
        method.Parameters.Any(x => x.Type.Is(KnownType.System_Text_RegularExpressions_RegexOptions));

    private bool IsCandidateCtor(SyntaxNode ctorNode) =>
        Language.Syntax.ArgumentExpressions(ctorNode).Count() < 3;

    private bool IsRegexMatchMethod(string name) =>
        MatchMehods.Any(method => method.Equals(name, Language.NameComparison));

    private static readonly string[] MatchMehods = new[]
    {
        nameof(Regex.IsMatch),
        nameof(Regex.Match),
        nameof(Regex.Matches),
        nameof(Regex.Replace),
        nameof(Regex.Split),
    };
}
