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
    public sealed class InitializeStaticFieldsInline : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3963";
        private const string MessageFormat = "Initialize all 'static fields' inline and remove the 'static constructor'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var constructor = (ConstructorDeclarationSyntax)c.Node;
                    if (!constructor.Modifiers.Any(SyntaxKind.StaticKeyword)
                        || (constructor.Body == null && constructor.ExpressionBody() == null))
                    {
                        return;
                    }
                    if (c.SemanticModel.GetDeclaredSymbol(constructor).ContainingType is { } currentType)
                    {
                        var bodyDescendantNodes = constructor.Body?.DescendantNodes().ToArray()
                            ?? constructor.ExpressionBody()?.DescendantNodes().ToArray()
                            ?? Array.Empty<SyntaxNode>();

                        var assignedFieldCount = bodyDescendantNodes
                            .OfType<AssignmentExpressionSyntax>()
                            .Select(x => c.SemanticModel.GetSymbolInfo(x.Left).Symbol)
                            .OfType<IFieldSymbol>()
                            .Where(x => x.ContainingType.Equals(currentType))
                            .Select(x => x.Name)
                            .Distinct()
                            .Count();
                        var hasIfOrSwitch = bodyDescendantNodes.Any(x => x.IsAnyKind(SyntaxKind.IfStatement, SyntaxKind.SwitchStatement));
                        if ((hasIfOrSwitch && assignedFieldCount <= 1)
                            || (!hasIfOrSwitch && assignedFieldCount > 0))
                        {
                            c.ReportIssue(CreateDiagnostic(Rule, constructor.Identifier.GetLocation()));
                        }
                    }
                },
                SyntaxKind.ConstructorDeclaration);
    }
}
