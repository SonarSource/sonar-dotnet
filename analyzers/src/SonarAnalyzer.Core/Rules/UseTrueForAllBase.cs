/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules;

public abstract class UseTrueForAllBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind> where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6603";

    protected override string MessageFormat => "The collection-specific \"TrueForAll\" method should be used instead of the \"All\" extension";

    private static readonly ImmutableArray<KnownType> TargetTypes = ImmutableArray.Create(
        KnownType.System_Array,
        KnownType.System_Collections_Generic_List_T,
        KnownType.System_Collections_Immutable_ImmutableList_T);

    protected UseTrueForAllBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            if (Language.GetName(c.Node).Equals(nameof(Enumerable.All), Language.NameComparison)
                && Language.Syntax.TryGetOperands(c.Node, out var left, out var right)
                && IsCorrectType(left, c.Model)
                && IsCorrectCall(right, c.Model))
            {
                c.ReportIssue(Rule, Language.Syntax.NodeIdentifier(c.Node)?.GetLocation());
            }
        },
        Language.SyntaxKind.InvocationExpression);

    protected static bool IsCorrectType(SyntaxNode left, SemanticModel model) =>
        model.GetTypeInfo(left).Type.DerivesFromAny(TargetTypes);

    protected static bool IsCorrectCall(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);
}
