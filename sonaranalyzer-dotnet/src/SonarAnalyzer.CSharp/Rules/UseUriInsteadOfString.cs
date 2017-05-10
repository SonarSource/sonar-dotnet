/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System.Collections.Generic;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId_RuleS3995)]
    [Rule(DiagnosticId_RuleS3996)]
    public sealed class UseUriInsteadOfString : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId_RuleS3995 = "S3995";
        private const string MessageFormat_RuleS3995 = "Change this return type to 'System.Uri'.";
        private static readonly DiagnosticDescriptor rule_S3995 =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId_RuleS3995, MessageFormat_RuleS3995, RspecStrings.ResourceManager);

        internal const string DiagnosticId_RuleS3996 = "S3996";
        private const string MessageFormat_RuleS3996 = "Change this property type to 'System.Uri'.";
        private static readonly DiagnosticDescriptor rule_S3996 =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId_RuleS3996, MessageFormat_RuleS3996, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule_S3995, rule_S3996);

        private static readonly HashSet<string> UrlNameVariants = new HashSet<string> { "uri", "url", "urn" };

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration);

                    if (methodSymbol.ReturnType.Is(KnownType.System_String) &&
                        !methodSymbol.IsOverride &&
                        NameContainsUri(methodSymbol.Name))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule_S3995, methodDeclaration.ReturnType.GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var propertyDeclaration = (PropertyDeclarationSyntax)c.Node;
                    var propertySymbol = c.SemanticModel.GetDeclaredSymbol(propertyDeclaration);

                    if (propertySymbol.Type.Is(KnownType.System_String) &&
                        !propertySymbol.IsOverride &&
                        NameContainsUri(propertySymbol.Name))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule_S3996, propertyDeclaration.Type.GetLocation()));
                    }
                },
                SyntaxKind.PropertyDeclaration);
        }

        private static bool NameContainsUri(string name)
        {
            var wordsInName = name.SplitCamelCaseToWords();
            return UrlNameVariants.Overlaps(wordsInName);
        }
    }
}
