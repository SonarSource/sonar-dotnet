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
    public sealed class StaticFieldInGenericClass : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2743";
        private const string MessageFormat = "A static field in a generic type is not shared among instances of different close constructed types.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var typeDeclaration = (TypeDeclarationSyntax)c.Node;
                    if (c.IsRedundantPositionalRecordContext()
                        || typeDeclaration.TypeParameterList == null
                        || typeDeclaration.TypeParameterList.Parameters.Count < 1)
                    {
                        return;
                    }
                    var typeParameterNames = typeDeclaration.TypeParameterList.Parameters.Select(x => x.Identifier.ToString()).ToArray();
                    var variables = typeDeclaration.Members
                        .OfType<FieldDeclarationSyntax>()
                        .Where(x => x.Modifiers.Any(SyntaxKind.StaticKeyword) && !HasGenericType(c, x.Declaration.Type, typeParameterNames))
                        .SelectMany(x => x.Declaration.Variables);
                    foreach (var variable in variables)
                    {
                        CheckMember(c, variable, variable.Identifier.GetLocation(), typeParameterNames);
                    }
                    foreach (var property in typeDeclaration.Members.OfType<PropertyDeclarationSyntax>().Where(x => x.Modifiers.Any(SyntaxKind.StaticKeyword)))
                    {
                        CheckMember(c, property, property.Identifier.GetLocation(), typeParameterNames);
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKindEx.RecordClassDeclaration,
                SyntaxKindEx.RecordStructDeclaration,
                SyntaxKind.StructDeclaration);

        private static void CheckMember(SonarSyntaxNodeReportingContext context, SyntaxNode root, Location location, string[] typeParameterNames)
        {
            if (!HasGenericType(context, root, typeParameterNames))
            {
                context.ReportIssue(CreateDiagnostic(Rule, location));
            }
        }

        private static bool HasGenericType(SonarSyntaxNodeReportingContext context, SyntaxNode root, string[] typeParameterNames) =>
            root.DescendantNodesAndSelf()
                .OfType<IdentifierNameSyntax>()
                .Any(x => typeParameterNames.Contains(x.Identifier.Value) && context.SemanticModel.GetSymbolInfo(x).Symbol is { Kind: SymbolKind.TypeParameter });
    }
}
