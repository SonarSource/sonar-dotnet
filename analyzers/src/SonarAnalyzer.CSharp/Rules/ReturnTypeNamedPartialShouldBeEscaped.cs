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
public sealed class ReturnTypeNamedPartialShouldBeEscaped : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S8380";
    private const string MessageFormat = "Return types named 'partial' should be escaped with '@'";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
            {
                if (!compilationStart.Compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp14))
                {
                    compilationStart.RegisterNodeAction(c =>
                        {
                            if (!IsGenericMethod(c.Node)
                                && c.Node.TypeSyntax() is { } returnType
                                && (returnType.Kind() == SyntaxKindEx.RefType ? ((RefTypeSyntaxWrapper)returnType).Type : returnType) is { } unWrapped
                                && unWrapped is NameSyntax { Arity: 0 } name
                                && name.GetIdentifier() is { Text: "partial" } identifier)
                            {
                                c.ReportIssue(Rule, identifier);
                            }
                        },
                        SyntaxKind.MethodDeclaration,
                        SyntaxKind.DelegateDeclaration,
                        SyntaxKindEx.LocalFunctionStatement);
                }
            });

    private static bool IsGenericMethod(SyntaxNode node) =>
        node is MethodDeclarationSyntax { Arity: > 0 }
        || node is DelegateDeclarationSyntax { Arity: > 0 }
        || (node.IsKind(SyntaxKindEx.LocalFunctionStatement) && (LocalFunctionStatementSyntaxWrapper)node is { TypeParameterList.Parameters.Count: > 0 });
}
