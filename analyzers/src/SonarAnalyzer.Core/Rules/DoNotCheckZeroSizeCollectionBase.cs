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

namespace SonarAnalyzer.Core.Rules;

public abstract class DoNotCheckZeroSizeCollectionBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    protected const string DiagnosticId = "S3981";

    private static readonly HashSet<string> CandidateNames = [nameof(Enumerable.Count), nameof(Array.Length), nameof(Array.LongLength)];
    private static readonly SymbolDisplayFormat TypeNameFormat = new(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

    protected override string MessageFormat => "The '{0}' of '{1}' always evaluates as '{2}' regardless the size.";

    protected DoNotCheckZeroSizeCollectionBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                var binary = c.Node;
                var binaryLeft = Language.Syntax.BinaryExpressionLeft(binary);
                var binaryRight = Language.Syntax.BinaryExpressionRight(binary);

                if (Language.ExpressionNumericConverter.ConstantIntValue(c.Model, binaryLeft) is { } left)
                {
                    CheckExpression(c, binary, binaryRight, left, Language.Syntax.ComparisonKind(binary).Mirror());
                }
                else if (Language.ExpressionNumericConverter.ConstantIntValue(c.Model, binaryRight) is { } right)
                {
                    CheckExpression(c, binary, binaryLeft, right, Language.Syntax.ComparisonKind(binary));
                }
            },
            Language.SyntaxKind.ComparisonKinds);

    protected void CheckExpression(SonarSyntaxNodeReportingContext context, SyntaxNode issue, SyntaxNode expression, int constant, ComparisonKind comparison)
    {
        expression = Language.Syntax.RemoveConditionalAccess(expression);
        var result = comparison.Compare(constant);
        if (result.IsInvalid
            && CandidateNames.Contains(Language.Syntax.NodeIdentifier(expression)?.ValueText)
            && context.Model.GetSymbolInfo(expression).Symbol is { } symbol
            && CollectionSizeTypeName(symbol) is { } symbolType)
        {
            context.ReportIssue(Rule, issue, symbol.Name, symbolType, (result == CountComparisonResult.AlwaysTrue).ToString());
        }
    }

    private string CollectionSizeTypeName(ISymbol symbol)
    {
        if (symbol.ContainingType.IsCountable() && IsCollectionLike(symbol.ContainingType))
        {
            return TypeName(symbol.ContainingType);
        }
        else if (symbol is IMethodSymbol method && IsSequenceCountMethod(method))
        {
            return TypeName(SequenceType(method));
        }
        return null;
    }

    private bool IsSequenceCountMethod(IMethodSymbol method) =>
        nameof(Enumerable.Count).Equals(method.Name, Language.NameComparison)
        && method.IsExtension
        && method.ReceiverType is not null
        && (method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T) || method.IsExtensionOn(KnownType.System_Linq_IQueryable));

    private static bool IsCollectionLike(ITypeSymbol type) =>
        type.IsAny(KnownType.System_Collections_IEnumerable, KnownType.System_Span_T, KnownType.System_ReadOnlySpan_T)
        || type.Implements(KnownType.System_Collections_IEnumerable);

    private static ITypeSymbol SequenceType(IMethodSymbol method) =>
        (method.ReducedFrom ?? method).Parameters[0].Type;

    private static string TypeName(ITypeSymbol type) =>
        type.OriginalDefinition.ToDisplayString(TypeNameFormat);
}
