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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AssertionArgsShouldBePassedInCorrectOrder : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S3415";
    private const string MessageFormat = "Make sure these 2 arguments are in the correct order: expected value, actual value.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    private static readonly IDictionary<string, ImmutableArray<KnownType>> MethodsWithType = new Dictionary<string, ImmutableArray<KnownType>>
    {
        ["AreEqual"]    = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
        ["AreNotEqual"] = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
        ["AreSame"]     = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
        ["AreNotSame"]  = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
        ["Equal"]       = ImmutableArray.Create(KnownType.Xunit_Assert),
        ["Same"]        = ImmutableArray.Create(KnownType.Xunit_Assert),
        ["NotSame"]     = ImmutableArray.Create(KnownType.Xunit_Assert)
    };

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
        {
            var methodCall = (InvocationExpressionSyntax)c.Node;
            if (!methodCall.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                || methodCall.ArgumentList.Arguments.Count < 2)
            {
                return;
            }

            var argsWithSymbols = new CSharpMethodParameterLookup(methodCall.ArgumentList, c.SemanticModel).GetAllArgumentParameterMappings().ToList();
            var expected = argsWithSymbols.SingleOrDefault(x => x.Symbol.Name == "expected")?.Node.Expression;
            var actual = argsWithSymbols.SingleOrDefault(x => x.Symbol.Name == "actual")?.Node.Expression;

            if (expected is LiteralExpressionSyntax || actual is not LiteralExpressionSyntax)
            {
                return;
            }

            var methodCallExpression = (MemberAccessExpressionSyntax)methodCall.Expression;

            var methodKnownTypes = MethodsWithType.GetValueOrDefault(methodCallExpression.Name.Identifier.ValueText);
            if (methodKnownTypes == null)
            {
                return;
            }

            var symbolInfo = c.SemanticModel.GetSymbolInfo(methodCallExpression.Expression).Symbol;
            var isAnyTrackedAssertType = (symbolInfo as INamedTypeSymbol).IsAny(methodKnownTypes);
            if (!isAnyTrackedAssertType)
            {
                return;
            }

            c.ReportIssue(Diagnostic.Create(Rule, argsWithSymbols[0].Node.CreateLocation(argsWithSymbols[1].Node)));
        },
        SyntaxKind.InvocationExpression);
}
