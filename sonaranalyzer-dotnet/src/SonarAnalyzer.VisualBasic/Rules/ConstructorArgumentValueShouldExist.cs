/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class ConstructorArgumentValueShouldExist : ConstructorArgumentValueShouldExistBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
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
                .SelectMany(x => x.BlockStatement.ParameterList.Parameters)
                .Select(x => x.Identifier.Identifier.ValueText);
        }

        protected override void ReportIssue(SyntaxNodeAnalysisContext c, AttributeData constructorArgumentAttribute)
        {
            var attributeSyntax =
                (AttributeSyntax)constructorArgumentAttribute.ApplicationSyntaxReference.GetSyntax();
            c.ReportDiagnosticWhenActive(Diagnostic.Create(rule,
                attributeSyntax.ArgumentList.Arguments[0].GetLocation()));
        }
    }
}
