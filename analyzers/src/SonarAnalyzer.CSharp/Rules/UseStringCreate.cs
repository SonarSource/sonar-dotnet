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
public sealed class UseStringCreate : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6618";
    private const string MessageFormat = """Use "string.Create" instead of "FormattableString".""";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private ImmutableArray<string> methodNames = ImmutableArray.Create(
        "CurrentCulture",
        "Invariant");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(start =>
        {
            if (!CompilationTargetsValidNetVersion(start.Compilation))
            {
                return;
            }

            context.RegisterNodeAction(c =>
            {
                var node = (InvocationExpressionSyntax)c.Node;

                if (methodNames.Any(x => NameIsEqual(node, x))
                    && node.TryGetOperands(out var left, out _)
                    && NameIsEqual(left, nameof(FormattableString))
                    && node.HasExactlyNArguments(1)
                    && node.ArgumentList.Arguments[0].Expression is InterpolatedStringExpressionSyntax
                    && c.SemanticModel.GetTypeInfo(left).Type.Is(KnownType.System_FormattableString))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, node.NodeIdentifier()?.GetLocation()));
                }
            },
            SyntaxKind.InvocationExpression);
        });

    private static bool NameIsEqual(SyntaxNode node, string name) =>
        node.GetName().Equals(name, StringComparison.Ordinal);

    private static bool CompilationTargetsValidNetVersion(Compilation compilation) =>
        compilation.GetTypeByMetadataName(KnownType.System_String) is var stringType
        && stringType.GetMembers("Create").Any(x => x is IMethodSymbol);
}
