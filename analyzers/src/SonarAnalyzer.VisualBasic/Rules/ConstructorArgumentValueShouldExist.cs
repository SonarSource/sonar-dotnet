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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class ConstructorArgumentValueShouldExist : ConstructorArgumentValueShouldExistBase
    {
        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var propertyDeclaration = (PropertyStatementSyntax)c.Node;
                    var propertySymbol = c.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
                    CheckConstructorArgumentProperty(c, propertyDeclaration, propertySymbol);
                },
                SyntaxKind.PropertyStatement);
        }

        protected override IEnumerable<string> GetAllParentClassConstructorArgumentNames(SyntaxNode propertyDeclaration)
        {
            return propertyDeclaration
                .FirstAncestorOrSelf<ClassBlockSyntax>()
                .Members
                .OfType<ConstructorBlockSyntax>()
                .SelectMany(x => x.BlockStatement.ParameterList?.Parameters ?? new SeparatedSyntaxList<ParameterSyntax>())
                .Select(x => x.Identifier.Identifier.ValueText);
        }

        protected override void ReportIssue(SonarSyntaxNodeReportingContext c, AttributeData constructorArgumentAttribute)
        {
            var attributeSyntax =
                (AttributeSyntax)constructorArgumentAttribute.ApplicationSyntaxReference.GetSyntax();
            c.ReportIssue(CreateDiagnostic(rule,
                attributeSyntax.ArgumentList.Arguments[0].GetLocation()));
        }
    }
}
