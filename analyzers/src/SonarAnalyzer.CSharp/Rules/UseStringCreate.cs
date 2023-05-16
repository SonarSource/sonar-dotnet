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
    private const string MessageFormat = """Use "string.Create(CultureInfo.InvariantCulture" instead of "FormattableString.Invariant".""";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private ImmutableArray<string> methodNames = ImmutableArray.Create<string>(
        "CurrentCulture",
        "Invariant");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
        {
            var node = (InvocationExpressionSyntax)c.Node;
            if (methodNames.Any(x => node.GetName().Equals(x, StringComparison.Ordinal))
                && IsCorrectType(node, c.SemanticModel))
            {
                c.ReportIssue(Diagnostic.Create(Rule, node.NodeIdentifier()?.GetLocation()));
            }
        },
        SyntaxKind.InvocationExpression);

    private static bool IsCorrectType(SyntaxNode node, SemanticModel model) =>
        model.GetTypeInfo(node).Type is { } type && type.DerivesFrom(KnownType.System_String);

    private bool CompilationRunsOnValidNetVersion(Compilation compilation, string methodName) =>
        compilation.GetTypeByMetadataName(KnownType.System_String) is { } str
        && str.GetMembers(methodName) is { } members
        && members.Any(x => x is IMethodSymbol { } method
                            && method.Parameters is { Length: 1 } parameters
                            && parameters[0].IsType(KnownType.System_Char));
}
