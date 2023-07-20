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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotUseOutRefParameters : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3874";
        private const string MessageFormat = "Consider refactoring this method in order to remove the need for this '{0}' modifier.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var parameter = (ParameterSyntax)c.Node;

                    if (!parameter.Modifiers.Any(IsRefOrOut)
                        || (parameter is { Parent: ParameterListSyntax { Parent: MethodDeclarationSyntax method } } && method.IsDeconstructor()))
                    {
                        return;
                    }

                    var modifier = parameter.Modifiers.First(IsRefOrOut);

                    var parameterSymbol = c.SemanticModel.GetDeclaredSymbol(parameter);

                    if (parameterSymbol?.ContainingSymbol is not IMethodSymbol containingMethod
                        || containingMethod.IsOverride
                        || !containingMethod.IsPubliclyAccessible()
                        || IsTryPattern(containingMethod, modifier)
                        || containingMethod.GetInterfaceMember() != null)
                    {
                        return;
                    }

                    c.ReportIssue(CreateDiagnostic(Rule, modifier.GetLocation(), modifier.ValueText));
                },
                SyntaxKind.Parameter);

        private static bool IsTryPattern(IMethodSymbol method, SyntaxToken modifier) =>
            method.Name.StartsWith("Try", StringComparison.Ordinal)
            && method.ReturnType.Is(KnownType.System_Boolean)
            && modifier.IsKind(SyntaxKind.OutKeyword);

        private static bool IsRefOrOut(SyntaxToken token) =>
            token.IsKind(SyntaxKind.RefKeyword)
            || token.IsKind(SyntaxKind.OutKeyword);
    }
}
