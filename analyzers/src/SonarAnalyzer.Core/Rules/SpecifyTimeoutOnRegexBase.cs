/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SonarAnalyzer.Core.Rules;

public abstract class SpecifyTimeoutOnRegexBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6444";

    // Process-wide regex timeout key. See https://learn.microsoft.com/dotnet/api/system.text.regularexpressions.regex.matchtimeout#remarks
    private const string RegexDefaultMatchTimeout = "REGEX_DEFAULT_MATCH_TIMEOUT";

    // NonBacktracking was added in .NET 7
    // See: https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regexoptions?view=net-7.0
    private const int NonBacktracking = 1024;

    private readonly string[] matchMethods =
    [
        nameof(Regex.IsMatch),
        nameof(Regex.Match),
        nameof(Regex.Matches),
        nameof(Regex.Replace),
        nameof(Regex.Split),
        "EnumerateSplits",  // https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.enumeratesplits?view=net-9.0
        "EnumerateMatches", // https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.enumeratematches?view=net-9.0
    ];

    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected virtual string MessageFormat => "Pass a timeout to limit the execution time.";

    // Reporting is deferred to a compilation end action, so the descriptor must carry the CompilationEnd tag.
    private DiagnosticDescriptor Rule => Language.CreateDescriptor(DiagnosticId, MessageFormat, isCompilationEnd: true);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(start =>
            {
                var candidates = new ConcurrentBag<SyntaxNode>();

                // StrongBox<bool> instead of a plain bool: this flag is captured by several node-action lambdas and
                // also passed by parameter to helper methods that read or write it. A plain bool parameter would be
                // copied by value at each call site, so a write from one action would never be seen by the others.
                // Boxing it on the heap gives every action a reference to the same mutable cell.
                var hasDefaultMatchTimeout = new StrongBox<bool>(false);

                start.RegisterNodeAction(
                    Language.GeneratedCodeRecognizer,
                    c => CollectCandidateCtor(c, candidates, hasDefaultMatchTimeout),
                    Language.SyntaxKind.ObjectCreationExpressions);

                start.RegisterNodeAction(
                    Language.GeneratedCodeRecognizer,
                    c => CollectCandidateInvocation(c, candidates, hasDefaultMatchTimeout),
                    Language.SyntaxKind.InvocationExpression);

                // If a process-wide REGEX_DEFAULT_MATCH_TIMEOUT is configured anywhere in the compilation, a
                // default timeout applies, so no issue is raised. Detected either as an AppDomain.SetData call
                // or as the key referenced as a string. Source ordering is intentionally ignored.
                start.RegisterNodeAction(
                    Language.GeneratedCodeRecognizer,
                    c =>
                    {
                        if (IsDefaultMatchTimeoutSetData(c.Model, c.Node))
                        {
                            // Volatile.Read/Write on .Value here and at every other access below: without it, the
                            // JIT/CPU is free to cache or hoist reads of this field into a register, or reorder/delay
                            // when a write becomes visible to other cores - so a thread could keep observing a stale
                            // value even though another thread already wrote true. SonarDiagnosticAnalyzer enables
                            // concurrent node-action execution by default, so the action that sets this flag and the
                            // actions that read it can genuinely run on different threads. The flag is only ever set
                            // to true (never back to false), so there's no lost-update race regardless of write order.
                            Volatile.Write(ref hasDefaultMatchTimeout.Value, true);
                        }
                    },
                    Language.SyntaxKind.InvocationExpression);

                start.RegisterNodeAction(
                    Language.GeneratedCodeRecognizer,
                    c =>
                    {
                        if (IsDefaultMatchTimeoutKey(c.Model, c.Node))
                        {
                            Volatile.Write(ref hasDefaultMatchTimeout.Value, true);
                        }
                    },
                    Language.SyntaxKind.StringLiteralExpressions);

                start.RegisterCompilationEndAction(c => ReportCandidates(c, candidates, hasDefaultMatchTimeout));
            });

    private void CollectCandidateCtor(SonarSyntaxNodeReportingContext c, ConcurrentBag<SyntaxNode> candidates, StrongBox<bool> hasDefaultMatchTimeout)
    {
        if (!Volatile.Read(ref hasDefaultMatchTimeout.Value) && IsCandidateCtor(c.Node) && RegexMethodLacksTimeout(c.Node, c.Model))
        {
            candidates.Add(c.Node);
        }
    }

    private void CollectCandidateInvocation(SonarSyntaxNodeReportingContext c, ConcurrentBag<SyntaxNode> candidates, StrongBox<bool> hasDefaultMatchTimeout)
    {
        if (!Volatile.Read(ref hasDefaultMatchTimeout.Value) && IsRegexMatchMethod(Language.Syntax.NodeIdentifier(c.Node).GetValueOrDefault().Text) && RegexMethodLacksTimeout(c.Node, c.Model))
        {
            candidates.Add(c.Node);
        }
    }

    // Precise detection: AppDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", ...). The method name is matched
    // syntactically first to skip the semantic model lookup on the hot path. ParameterLookup handles named
    // arguments; IsDefaultMatchTimeoutKey resolves the key even when it is a constant declared in another assembly.
    private bool IsDefaultMatchTimeoutSetData(SemanticModel model, SyntaxNode node) =>
        Language.Syntax.NodeIdentifier(node) is { } identifier
        && identifier.Text.Equals(nameof(AppDomain.SetData), Language.NameComparison)
        && model.GetSymbolInfo(node).Symbol is IMethodSymbol { Name: nameof(AppDomain.SetData), IsStatic: false } method
        && method.ContainingType.Is(KnownType.System_AppDomain)
        && Language.MethodParameterLookup(node, method).TryGetSyntax("name", out var nameArguments)
        && nameArguments.Length == 1
        && IsDefaultMatchTimeoutKey(model, nameArguments[0]);

    private bool IsDefaultMatchTimeoutKey(SemanticModel model, SyntaxNode node) =>
        Language.FindConstantValue(model, node) as string == RegexDefaultMatchTimeout;

    private void ReportCandidates(SonarCompilationReportingContext c, ConcurrentBag<SyntaxNode> candidates, StrongBox<bool> hasDefaultMatchTimeout)
    {
        if (!Volatile.Read(ref hasDefaultMatchTimeout.Value))
        {
            foreach (var node in candidates)
            {
                c.ReportIssue(Language.GeneratedCodeRecognizer, Rule, node.GetLocation());
            }
        }
    }

    private bool RegexMethodLacksTimeout(SyntaxNode node, SemanticModel model) =>
        model.GetSymbolInfo(node).Symbol is IMethodSymbol method and ({ IsStatic: true } or { IsConstructor: true })
        && method.ContainingType.Is(KnownType.System_Text_RegularExpressions_Regex)
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
