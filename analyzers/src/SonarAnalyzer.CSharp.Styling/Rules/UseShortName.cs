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

namespace SonarAnalyzer.Rules.CSharp.Styling;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseShortName : StylingAnalyzer
{
    private static readonly RenameInfo[] RenameCandidates =
        [
            new("CancellationToken", "cancellationToken", "cancel"),
            new("CancellationToken", "CancellationToken", "Cancel"),
            new("DiagnosticDescriptor", "diagnosticDescriptor", "descriptor"),
            new("DiagnosticDescriptor", "DiagnosticDescriptor", "Descriptor"),
            new("SyntaxNode", "syntaxNode", "node"),
            new("SyntaxNode", "SyntaxNode", "Node"),
            new("SyntaxToken", "syntaxToken", "token"),
            new("SyntaxToken", "SyntaxToken", "Token"),
            new("SyntaxTree", "syntaxTree", "tree"),
            new("SyntaxTree", "SyntaxTree", "Tree"),
            new("SyntaxTrivia", "syntaxTrivia", "trivia"),
            new("SyntaxTrivia", "SyntaxTrivia", "Trivia"),
            new("SemanticModel", "semanticModel", "model"),
            new("SemanticModel", "SemanticModel", "Model")
        ];

    public UseShortName() : base("T0017", "Use short name '{0}'.") { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c => ValidateDeclaration(c, ((VariableDeclaratorSyntax)c.Node).Identifier), SyntaxKind.VariableDeclarator);
        context.RegisterNodeAction(c => ValidateDeclaration(c, ((PropertyDeclarationSyntax)c.Node).Identifier), SyntaxKind.PropertyDeclaration);
        context.RegisterNodeAction(c =>
            {
                if (!FollowsPredefinedName(c.ContainingSymbol))
                {
                    ValidateDeclaration(c, ((ParameterSyntax)c.Node).Identifier);
                }
            }
            , SyntaxKind.Parameter);
    }

    private void ValidateDeclaration(SonarSyntaxNodeReportingContext context, SyntaxToken identifier)
    {
        if (FindRename(identifier.ValueText) is { } name
            && context.SemanticModel.GetDeclaredSymbol(context.Node).GetSymbolType() is { } type
            && type.Name == name.TypeName)
        {
            context.ReportIssue(Rule, identifier, identifier.ValueText.Replace(name.UsedName, name.SuggestedName));
        }
    }

    private static RenameInfo FindRename(string name) =>
        Array.Find(RenameCandidates, x => name.Contains(x.UsedName));

    private static bool FollowsPredefinedName(ISymbol symbol) =>
        symbol is IMethodSymbol method
        && (symbol.IsOverride || symbol.GetInterfaceMember() is not null || method.PartialDefinitionPart is not null);

    private record RenameInfo(string TypeName, string UsedName, string SuggestedName);
}
