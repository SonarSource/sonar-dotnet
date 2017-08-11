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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MethodsShouldNotHaveIdenticalImplementations : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4144";
        private const string MessageFormat = "Update this method so that its implementation is not identical to '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> ThrowStatementTypesToIgnore = ImmutableHashSet.Create(
            KnownType.System_NotImplementedException,
            KnownType.System_NotSupportedException);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;
                    var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>().ToList();

                    var alreadyHandledMethods = new HashSet<MethodDeclarationSyntax>();

                    foreach (var method in methods)
                    {
                        if (alreadyHandledMethods.Contains(method))
                        {
                            continue;
                        }

                        var duplicates = methods.Where(m => AreDuplicates(method, m, c.SemanticModel)).ToList();
                        if (duplicates.Count > 0)
                        {
                            alreadyHandledMethods.Add(method);
                            alreadyHandledMethods.UnionWith(duplicates);

                            foreach (var duplicate in duplicates)
                            {
                                c.ReportDiagnostic(Diagnostic.Create(rule, duplicate.Identifier.GetLocation(),
                                    additionalLocations: new[] { method.Identifier.GetLocation() },
                                    messageArgs: method.Identifier.ValueText));
                            }
                        }
                    }
                }, SyntaxKind.ClassDeclaration);
        }

        private bool AreDuplicates(MethodDeclarationSyntax firstMethod, MethodDeclarationSyntax secondMethod,
            SemanticModel model)
        {
            if (firstMethod == secondMethod)
            {
                return false;
            }

            if (firstMethod.Body != null &&
                secondMethod.Body != null)
            {
                return firstMethod.Body.Statements.Count >= 2 &&
                    firstMethod.Identifier.ValueText != secondMethod.Identifier.ValueText &&
                    firstMethod.Body.IsEquivalentTo(secondMethod.Body, false);
            }

            return false;
        }
    }
}
