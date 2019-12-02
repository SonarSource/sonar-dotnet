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
        protected const string MessageFormat = "All '{0}' method overloads should be adjacent.";

        protected StringComparison CaseSensitivity
        {
            get => IsCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
        }

        protected abstract DiagnosticDescriptor Rule { get; }

        protected abstract bool IsCaseSensitive { get; }

        protected abstract SyntaxToken? GetNameSyntaxNode(TMemberDeclarationSyntax member);

        protected abstract bool IsValidMemberForOverload(TMemberDeclarationSyntax member);

        protected abstract bool IsStatic(TMemberDeclarationSyntax member);

        protected void CheckMembers(SyntaxNodeAnalysisContext c, IEnumerable<TMemberDeclarationSyntax> members)
        {
            var misplacedOverloadsMapping = GetMisplacedOverloads(members);
            foreach (var misplacedOverloadsEntry in misplacedOverloadsMapping)
            {
                var misplacedOverloadsByAccessibility = misplacedOverloadsEntry.Value
                    .GroupBy(memberDeclaration => c.SemanticModel.GetDeclaredSymbol(memberDeclaration)?.DeclaredAccessibility);

                foreach (var misplacedOverloads in misplacedOverloadsByAccessibility)
                {
                    var firstMethodSignature = misplacedOverloads.First();
                    if (misplacedOverloads.Key != null && misplacedOverloads.Count() > 1 && firstMethodSignature != null)
                    {
                        var secondaryLocations = misplacedOverloads
                            .Skip(1)
                            .Select(member => GetNameSyntaxNode(member))
                            .Where(nameSyntax => nameSyntax != null)
                            .Select(nameSyntax => new SecondaryLocation(nameSyntax?.GetLocation(), "Non-adjacent overload"));
                        c.ReportDiagnosticWhenActive(
                            Diagnostic.Create(
                                descriptor: Rule,
                                location: GetNameSyntaxNode(firstMethodSignature)?.GetLocation(),
                                additionalLocations: secondaryLocations.ToAdditionalLocations(),
                                properties: secondaryLocations.ToProperties(),
                                messageArgs: GetMethodName(firstMethodSignature, true)));

                    }
                }
            }
        }

        protected IDictionary<string, List<TMemberDeclarationSyntax>> GetMisplacedOverloads(IEnumerable<TMemberDeclarationSyntax> members)
        {
            //Key is methodName concatenated with IsStatic. This identifies group of members that should be placed together.
            var misplacedOverloads = new Dictionary<string, List<TMemberDeclarationSyntax>>();
            string previousKey = null, key;
            foreach (var member in members)
            {
                if (GetMethodName(member, IsCaseSensitive) is string methodName
                    && IsValidMemberForOverload(member))
                {
                    // #4136 Should not raise when static methods are grouped together
                    key = methodName + "/" + IsStatic(member);  
                    if (misplacedOverloads.TryGetValue(key, out var currentList))
                    {
                        if (!key.Equals(previousKey, CaseSensitivity))   
                        {
                            currentList.Add(member);
                        }
                    }
                    else
                    {
                        misplacedOverloads.Add(key, new List<TMemberDeclarationSyntax> { member });
                    }
                    previousKey = key;
                }
                else
                {
                    previousKey = null;
                }
            }
            return misplacedOverloads;
        }

        private string GetMethodName(TMemberDeclarationSyntax member, bool caseSensitive)
        {
            var nameSyntaxNode = GetNameSyntaxNode(member);
            if (caseSensitive)
            {
                return nameSyntaxNode?.ValueText;
            }
            else
            {
                return nameSyntaxNode?.ValueText?.ToLowerInvariant();
            }
        }
    }
}
