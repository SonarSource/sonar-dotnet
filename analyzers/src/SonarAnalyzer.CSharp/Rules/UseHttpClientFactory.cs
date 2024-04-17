/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
public sealed class UseHttpClientFactory : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6962";
    private const string MessageFormat = "FIXME";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            if (!compilationStartContext.Compilation.ReferencesControllers())
            {
                return;
            }
            compilationStartContext.RegisterSymbolStartAction(symbolStartContext =>
            {
                var symbol = (INamedTypeSymbol)symbolStartContext.Symbol;
                if (symbol.IsControllerType())
                {
                    symbolStartContext.RegisterSyntaxNodeAction(nodeContext =>
                    {
                        var node = nodeContext.Node;
                        var objectCreation = ObjectCreationFactory.Create(node);
                        if (objectCreation.IsKnownType(KnownType.System_Net_Http_HttpClient, nodeContext.SemanticModel)
                            && !IsAssignedForReuse(nodeContext))
                        {
                            nodeContext.ReportIssue(Rule, node);
                        }
                    }, SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);

                    symbolStartContext.RegisterSymbolEndAction(symbolEndContext =>
                    { });
                }
            }, SymbolKind.NamedType);
        });

    private static bool IsAssignedForReuse(SonarSyntaxNodeReportingContext context) =>
        !IsInVariableDeclaration(context.Node)
        && (IsInFieldOrPropertyInitializer(context.Node) || IsAssignedToStaticFieldOrProperty(context));

    private static bool IsInVariableDeclaration(SyntaxNode node) =>
        node.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: LocalDeclarationStatementSyntax or UsingStatementSyntax } } };

    private static bool IsInFieldOrPropertyInitializer(SyntaxNode node) =>
        node.Ancestors().Any(x => x.IsAnyKind(SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration));

    private static bool IsAssignedToStaticFieldOrProperty(SonarSyntaxNodeReportingContext context) =>
        context.Node.Parent.WalkUpParentheses() is AssignmentExpressionSyntax assignment
            && context.SemanticModel.GetSymbolInfo(assignment.Left, context.Cancel).Symbol is { IsStatic: true, Kind: SymbolKind.Field or SymbolKind.Property };

}
