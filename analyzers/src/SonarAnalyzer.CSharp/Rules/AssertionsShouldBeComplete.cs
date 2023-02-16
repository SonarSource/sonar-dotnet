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

using SonarAnalyzer.Helpers;

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
                if (start.Compilation.References(KnownAssembly.MSTest)
                    // Assert.That was introduced in Version 1.1.14 but the AssemblyIdentity version (14.0.0.0) does not align with the Nuget version so we need
                    // to check at runtime for the presence of "Assert.That"
                    && start.Compilation.GetTypeByMetadataName(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert) is { } assertType
                    && assertType.GetMembers("That") is { Length: 1 } thatMembers
                    && thatMembers[0] is IPropertySymbol { IsStatic: true })
                {
                    start.RegisterNodeAction(c =>
                    {

                    }, SyntaxKind.SimpleMemberAccessExpression);
                }
                if (start.Compilation.References(KnownAssembly.FluentAssertions))
                {
                    start.RegisterNodeAction(c =>
                    {
                        CheckInvocation(c, invoke => invoke.NameIs("Should")
                            && c.SemanticModel.GetSymbolInfo(invoke) is { Symbol: IMethodSymbol method }
                            && method.ContainingType.Is(KnownType.FluentAssertions_Execution_AssertionScope));
                    }, SyntaxKind.InvocationExpression);
                }
                if (start.Compilation.References(KnownAssembly.NFluent))
                {
                    start.RegisterNodeAction(c =>
                    {

                    }, SyntaxKind.InvocationExpression);
                }
                if (start.Compilation.References(KnownAssembly.NSubstitute))
                {
                    start.RegisterNodeAction(c =>
                    {

                    }, SyntaxKind.InvocationExpression);
                }
            });
    private void CheckInvocation(SonarSyntaxNodeReportingContext c, Func<InvocationExpressionSyntax, bool> value) => throw new NotImplementedException();
}
