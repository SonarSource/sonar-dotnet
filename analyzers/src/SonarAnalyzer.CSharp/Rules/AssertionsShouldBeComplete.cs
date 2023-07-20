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
public sealed class AssertionsShouldBeComplete : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2970";
    private const string MessageFormat = "Complete the assertion";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(start =>
            {
                if (start.Compilation.References(KnownAssembly.FluentAssertions))
                {
                    start.RegisterNodeAction(c =>
                        CheckInvocation(c, invocation =>
                            invocation.NameIs("Should")
                            && c.SemanticModel.GetSymbolInfo(invocation).AllSymbols().Any(x =>
                                x is IMethodSymbol
                                {
                                    IsExtensionMethod: true,
                                    ReturnsVoid: false,
                                    ContainingType: { } container,
                                    ReturnType: { } returnType,
                                }
                                && (container.Is(KnownType.FluentAssertions_AssertionExtensions)
                                    // ⬆️ Built in assertions. ⬇️ Custom assertions (the majority at least).
                                    || returnType.DerivesFrom(KnownType.FluentAssertions_Primitives_ReferenceTypeAssertions)))),
                            SyntaxKind.InvocationExpression);
                }
                if (start.Compilation.References(KnownAssembly.NFluent))
                {
                    start.RegisterNodeAction(c =>
                        CheckInvocation(c, invocation =>
                            invocation.NameIs("That", "ThatEnum", "ThatCode", "ThatAsyncCode", "ThatDynamic")
                            && c.SemanticModel.GetSymbolInfo(invocation) is
                            {
                                Symbol: IMethodSymbol
                                {
                                    IsStatic: true,
                                    ReturnsVoid: false,
                                    ContainingType: { IsStatic: true } container
                                }
                            }
                            && container.Is(KnownType.NFluent_Check)),
                            SyntaxKind.InvocationExpression);
                }
                if (start.Compilation.References(KnownAssembly.NSubstitute))
                {
                    start.RegisterNodeAction(c =>
                        CheckInvocation(c, invocation =>
                            invocation.NameIs("Received", "DidNotReceive", "ReceivedWithAnyArgs", "DidNotReceiveWithAnyArgs", "ReceivedCalls")
                            && c.SemanticModel.GetSymbolInfo(invocation) is
                            {
                                Symbol: IMethodSymbol
                                {
                                    IsExtensionMethod: true,
                                    ReturnsVoid: false,
                                    ContainingType: { } container,
                                }
                            }
                            && container.Is(KnownType.NSubstitute_SubstituteExtensions)),
                            SyntaxKind.InvocationExpression);
                }
            });

    private static void CheckInvocation(SonarSyntaxNodeReportingContext c, Func<InvocationExpressionSyntax, bool> isAssertionMethod)
    {
        if (c.Node is InvocationExpressionSyntax invocation
            && isAssertionMethod(invocation)
            && !HasContinuation(invocation))
        {
            c.ReportIssue(CreateDiagnostic(Rule, invocation.GetIdentifier()?.GetLocation()));
        }
    }

    private static bool HasContinuation(InvocationExpressionSyntax invocation)
    {
        var closeParen = invocation.ArgumentList.CloseParenToken;
        if (!closeParen.IsKind(SyntaxKind.CloseParenToken) || closeParen.IsMissing || !invocation.GetLastToken().Equals(closeParen))
        {
            // Any invocation should end with ")". We are in unknown territory here.
            return true;
        }
        if (closeParen.GetNextToken() is var nextToken
            && !nextToken.IsKind(SyntaxKind.SemicolonToken))
        {
            // There is something right to the invocation that is not a semicolon.
            return true;
        }
        // We are in some kind of statement context "??? Should();"
        // The result might be stored in a variable or returned from the method/property
        return nextToken.Parent switch
        {
            MethodDeclarationSyntax { ReturnType: { } returnType } => !IsVoid(returnType),
            { } parent when LocalFunctionStatementSyntaxWrapper.IsInstance(parent) => !IsVoid(((LocalFunctionStatementSyntaxWrapper)parent).ReturnType),
            PropertyDeclarationSyntax => true,
            AccessorDeclarationSyntax { Keyword.RawKind: (int)SyntaxKind.GetKeyword } => true,
            ReturnStatementSyntax => true,
            LocalDeclarationStatementSyntax => true,
            ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax } => true,
            _ => false,
        };
    }

    private static bool IsVoid(TypeSyntax type) =>
        type is PredefinedTypeSyntax { Keyword.RawKind: (int)SyntaxKind.VoidKeyword };
}
