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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.Rules;

public abstract class SpecifyTimeoutOnRegexBase<TSyntaxKind> : HotspotDiagnosticAnalyzer
        where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6444";

    // NonBacktracking was added in .NET 7
    // See: https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regexoptions?view=net-7.0
    private const int NonBacktracking = 1024;

    private readonly string[] matchMethods =
    {
        nameof(Regex.IsMatch),
        nameof(Regex.Match),
        nameof(Regex.Matches),
        nameof(Regex.Replace),
        nameof(Regex.Split),
    };

    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

    protected virtual string MessageFormat => "Pass a timeout to limit the execution time.";

    private DiagnosticDescriptor Rule => Language.CreateDescriptor(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected SpecifyTimeoutOnRegexBase(IAnalyzerConfiguration config) : base(config) { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (!IsEnabled(c.Options))
                {
                    return;
                }

                if (IsCandidateCtor(c.Node)
                    && RegexMethodLacksTimeout(c.Node, c.SemanticModel))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, c.Node.GetLocation()));
                }
            },
            Language.SyntaxKind.ObjectCreationExpressions);

        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (!IsEnabled(c.Options))
                {
                    return;
                }

                if (IsRegexMatchMethod(Language.Syntax.NodeIdentifier(c.Node).GetValueOrDefault().Text)
                    && RegexMethodLacksTimeout(c.Node, c.SemanticModel))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, c.Node.GetLocation()));
                }
            },
            Language.SyntaxKind.InvocationExpression);
    }

    private bool RegexMethodLacksTimeout(SyntaxNode node, SemanticModel model) =>
        model.GetSymbolInfo(node).Symbol is IMethodSymbol method
        && method.ContainingType.Is(KnownType.System_Text_RegularExpressions_Regex)
        && (method.IsStatic || method.IsConstructor())
        && !ContainsMatchTimeout(method)
        && !NoBacktracking(method, node, model);

    private static bool ContainsMatchTimeout(IMethodSymbol method) =>
        method.Parameters.Any(x => x.Name == "matchTimeout");

    private bool NoBacktracking(IMethodSymbol method, SyntaxNode node, SemanticModel model) =>
        method.Parameters.SingleOrDefault(x => x.Name == "options") is { } parameter
        && Language.MethodParameterLookup(node, method).TryGetNonParamsSyntax(parameter, out var expression)
        && Language.FindConstantValue(model, expression) is int options
        && (options & NonBacktracking) == NonBacktracking;

    private bool IsCandidateCtor(SyntaxNode ctorNode) =>
        Language.Syntax.ArgumentExpressions(ctorNode).Count() < 3;

    private bool IsRegexMatchMethod(string name) =>
        matchMethods.Any(x => x.Equals(name, Language.NameComparison));
}
