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
using System.Text.RegularExpressions;
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
    public class AvoidExcessiveInheritance : ParameterLoadingDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S110";
        private const string MessageFormat = "This class has {0} parents which is greater than {1} authorized.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private const int MaximumDepthDefaultValue = 5;
        [RuleParameter(
            key: "max",
            type: PropertyType.Integer,
            description: "Maximum depth of the inheritance tree. (Number)",
            defaultValue: MaximumDepthDefaultValue)]
        public int MaximumDepth { get; set; } = MaximumDepthDefaultValue;

        private ICollection<Regex> filteredClassesRegex = new List<Regex>();

        private const string FilteredClassesDefaultValue = "";
        private string filteredClasses = FilteredClassesDefaultValue;
        [RuleParameter(
            key: "filteredClasses",
            type: PropertyType.String,
            description: "Classes to be filtered out of the count of inheritance. Depth counting will stop when a filtered class is reached. (String)",
            defaultValue: FilteredClassesDefaultValue)]
        public string FilteredClasses
        {
            get
            {
                return filteredClasses;
            }
            set
            {
                filteredClasses = value;
                filteredClassesRegex = filteredClasses.Split(',')
                    .Select(s => new Regex(WilcardToRegular(s), RegexOptions.Compiled))
                    .ToList();
            }
        }

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (ClassDeclarationSyntax)c.Node;
                    var symbol = c.SemanticModel.GetDeclaredSymbol(declaration);
                    if (symbol == null)
                    {
                        return;
                    }

                    var baseTypesCount = 0;
                    var baseTypeNames = symbol.GetSelfAndBaseTypes()
                        .Select(nts => nts.OriginalDefinition.ToDisplayString())
                        .Skip(1); // remove the class itself
                    foreach (var className in baseTypeNames)
                    {
                        if (filteredClassesRegex.Any(regex => regex.IsMatch(className)))
                        {
                            break;
                        }
                        baseTypesCount++;
                    }

                    if (baseTypesCount > MaximumDepth)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, declaration.Identifier.GetLocation(),
                            baseTypesCount, MaximumDepth));
                    }

                }, SyntaxKind.ClassDeclaration);
        }

        private static string WilcardToRegular(string pattern)
        {
            return string.Concat("^", Regex.Escape(pattern).Replace("\\*", ".*"), "$");
        }
    }
}