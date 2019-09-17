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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonarAnalyzer.Rules
{
    public abstract class MethodOverloadsShouldBeGroupedBase<TMemberDeclarationSyntax> : SonarDiagnosticAnalyzer
        where TMemberDeclarationSyntax : SyntaxNode
    {
        internal const string DiagnosticId = "S4136";
        protected const string MessageFormat = "All '{0}' signatures should be adjacent";

        protected StringComparison CaseSensitivity
        {
            get => IsCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
        }

        protected abstract DiagnosticDescriptor Rule { get; }

        protected abstract bool IsCaseSensitive { get; }

        protected abstract SyntaxToken? GetNameSyntaxNode(TMemberDeclarationSyntax member);

        protected bool IsValidMemberForOverload(TMemberDeclarationSyntax member) => true;
        
        protected string GetMethodName(TMemberDeclarationSyntax member)
        {
            var nameSyntaxNode = GetNameSyntaxNode(member);
            if (IsCaseSensitive)
            {
                return nameSyntaxNode?.ValueText;
            }
            else
            {
                return nameSyntaxNode?.ValueText?.ToLowerInvariant();
            }
        }
        
        protected void CheckMembers(SyntaxNodeAnalysisContext c, IEnumerable<TMemberDeclarationSyntax> members)
        {
            var misplacedOverloadsMapping = GetMisplacedOverloads(members);
            foreach (var misplacedOverloadsEntry in misplacedOverloadsMapping)
            {
                var misplacedOverloadsByAccessibility = misplacedOverloadsEntry.Value
                    .GroupBy(member => c.SemanticModel.GetSymbolInfo(member).Symbol?.DeclaredAccessibility);
                
                foreach (var misplacedOverloads in misplacedOverloadsByAccessibility)
                {
                    if (misplacedOverloads.Key != null && misplacedOverloads.Count() > 1)
                    {
                        var mainMethod = misplacedOverloads.First();
                        var secondaryLocations = misplacedOverloads.Skip(1).Select(member => new SecondaryLocation(member.GetLocation(), "Non-adjacent overload"));
                        c.ReportDiagnosticWhenActive(
                            Diagnostic.Create(
                                descriptor: Rule,
                                location: mainMethod.GetLocation(),
                                additionalLocations: secondaryLocations.ToAdditionalLocations(),
                                properties: secondaryLocations.ToProperties(),
                                messageArgs: misplacedOverloadsEntry.Key));

                    }
                }
            }
        }

        protected IDictionary<string, List<TMemberDeclarationSyntax>> GetMisplacedOverloads(IEnumerable<TMemberDeclarationSyntax> members)
        {
            var misplacedOverloads = new Dictionary<string, List<TMemberDeclarationSyntax>>();
            string previousMemberName = null;
            foreach (var member in members)
            {
                if (GetMethodName(member) is string methodName
                    && IsValidMemberForOverload(member))
                {
                    if (misplacedOverloads.TryGetValue(methodName, out var currentList))
                    {
                        if (!methodName.Equals(previousMemberName, CaseSensitivity))
                        {
                            currentList.Add(member);
                        }
                    }
                    else
                    {
                        misplacedOverloads.Add(methodName, new List<TMemberDeclarationSyntax> { member });
                    }
                    previousMemberName = methodName;
                }
                else
                {
                    previousMemberName = null;
                }
            }
            return misplacedOverloads;
        }
    }
}
