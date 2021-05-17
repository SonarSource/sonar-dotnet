/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class MethodsShouldNotHaveIdenticalImplementationsBase<TMethodDeclarationSyntax, TLanguageKindEnum>
        : SonarDiagnosticAnalyzer
        where TLanguageKindEnum : struct
    {
        protected const string DiagnosticId = "S4144";
        protected const string MessageFormat = "Update this method so that its implementation is not identical to '{0}'.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract TLanguageKindEnum[] SyntaxKinds { get; }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(GeneratedCodeRecognizer,
                c =>
                {
                    if (c.ContainingSymbol.Kind != SymbolKind.NamedType)
                    {
                        return;
                    }

                    var methods = GetMethodDeclarations(c.Node).ToList();

                    var alreadyHandledMethods = new HashSet<TMethodDeclarationSyntax>();

                    foreach (var method in methods)
                    {
                        if (alreadyHandledMethods.Contains(method))
                        {
                            continue;
                        }

                        alreadyHandledMethods.Add(method);

                        var duplicates = methods.Except(alreadyHandledMethods)
                                                .Where(m => AreDuplicates(method, m))
                                                .ToList();

                        alreadyHandledMethods.UnionWith(duplicates);

                        foreach (var duplicate in duplicates)
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], GetMethodIdentifier(duplicate).GetLocation(),
                                additionalLocations: new[] { GetMethodIdentifier(method).GetLocation() },
                                messageArgs: GetMethodIdentifier(method).ValueText));
                        }
                    }
                }, SyntaxKinds);

        protected abstract IEnumerable<TMethodDeclarationSyntax> GetMethodDeclarations(SyntaxNode node);

        protected abstract SyntaxToken GetMethodIdentifier(TMethodDeclarationSyntax method);

        protected abstract bool AreDuplicates(TMethodDeclarationSyntax firstMethod, TMethodDeclarationSyntax secondMethod);

        protected static bool HaveSameParameters<TSyntax>(SeparatedSyntaxList<TSyntax>? leftParameters, SeparatedSyntaxList<TSyntax>? rightParameters)
            where TSyntax : SyntaxNode
        {
            if (leftParameters == null && rightParameters == null)
            {
                return true;
            }

            if (leftParameters == null || rightParameters == null || leftParameters.Value.Count != rightParameters.Value.Count)
            {
                return false;
            }

            return leftParameters.Value.Zip(rightParameters.Value, (p1, p2) => new { p1, p2 })
                                 .All(tuple => tuple.p1.IsEquivalentTo(tuple.p2, false));
        }
    }
}
