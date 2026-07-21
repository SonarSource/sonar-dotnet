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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OrderByRepeated : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S3169";
    private const string MessageFormat = "Use 'ThenBy' instead.";
    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly HashSet<string> OrderMethodNames =
    [
        nameof(Enumerable.OrderBy),
        nameof(Enumerable.OrderByDescending),
        "Order",            // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.order
        "OrderDescending"   // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.orderdescending
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
            {
                // These Azure APIs return IOrderedQueryable, despite not ordering the sequence so we exclude them
                if (cc.Compilation.GetTypeByMetadataName(KnownType.Microsoft_Azure_Cosmos_Container) is null
                    && cc.Compilation.GetTypeByMetadataName(KnownType.Microsoft_Azure_Documents_Client_DocumentClient) is null)
                {
                    cc.RegisterNodeAction(
                        c =>
                        {
                            var invocation = (InvocationExpressionSyntax)c.Node;
                            if (invocation.Expression.LeftOfDot is { } receiver
                                && IsOrderingOnAlreadyOrderedSequence(invocation, receiver, c.Model)
                                && invocation.GetIdentifier() is { } identifier)
                            {
                                c.ReportIssue(Rule, identifier.GetLocation());
                            }
                        },
                        SyntaxKind.InvocationExpression);

                    cc.RegisterNodeAction(
                        c =>
                        {
                            var clauses = ((QueryBodySyntax)c.Node).Clauses;
                            for (var i = 1; i < clauses.Count; i++)
                            {
                                if (clauses[i] is OrderByClauseSyntax orderBy && clauses[i - 1] is OrderByClauseSyntax)
                                {
                                    c.ReportIssue(Rule, orderBy.OrderByKeyword.GetLocation());
                                }
                            }
                        },
                        SyntaxKind.QueryBody);
                }
            });

    private static bool IsOrderingOnAlreadyOrderedSequence(InvocationExpressionSyntax invocation, ExpressionSyntax receiver, SemanticModel model) =>
        model.GetSymbolInfo(invocation).Symbol is IMethodSymbol { MethodKind: MethodKind.ReducedExtension } method
        && OrderMethodNames.Contains(method.Name)
        && model.GetTypeInfo(receiver).Type is { } receiverType
        && (receiverType.Is(KnownType.System_Linq_IOrderedEnumerable_T) || receiverType.Is(KnownType.System_Linq_IOrderedQueryable_T));
}
