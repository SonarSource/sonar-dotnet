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

using System;
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
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class AvoidExcessiveInheritance : ParameterLoadingDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S110";
        private const string MessageFormat = "This {0} has {1} parents which is greater than {2} authorized.";
        private const string FilteredClassesDefaultValue = "";
        private const int MaximumDepthDefaultValue = 5;
        private string filteredClasses = FilteredClassesDefaultValue;
        private ICollection<Regex> filters = new List<Regex>();

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager, false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        [RuleParameter("max", PropertyType.Integer, "Maximum depth of the inheritance tree. (Number)", MaximumDepthDefaultValue)]
        public int MaximumDepth { get; set; } = MaximumDepthDefaultValue;

        [RuleParameter(
            "filteredClasses",
            PropertyType.String,
            "Comma-separated list of classes or records to be filtered out of the count of inheritance. Depth " +
            "counting will stop when a filtered class or record is reached. For example: System.Windows.Controls.UserControl, " +
            "System.Windows.*. (String)",
            FilteredClassesDefaultValue)]
        public string FilteredClasses
        {
            get => filteredClasses;
            set
            {
                filteredClasses = value;
                filters = filteredClasses.Split(',')
                    .Select(WildcardPatternToRegularExpression)
                    .ToList();
            }
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/4745
        protected override bool EnableConcurrentExecution => false;

        protected override void Initialize(ParameterLoadingAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var objectTypeInfo = new ObjectTypeInfo(c.Node, c.SemanticModel);
                    // For records we are triggered twice and we need to differentiate between the calls by checking the containing symbol kind
                    // See: https://github.com/dotnet/roslyn/issues/50989
                    if (objectTypeInfo.Symbol == null || c.ContainingSymbol.Kind != SymbolKind.NamedType)
                    {
                        return;
                    }

                    var thisTypeRootNamespace = GetRootNamespace(objectTypeInfo.Symbol);

                    var baseTypesCount = objectTypeInfo.Symbol.BaseType.GetSelfAndBaseTypes()
                        .TakeWhile(s => GetRootNamespace(s) == thisTypeRootNamespace)
                        .Select(nts => nts.OriginalDefinition.ToDisplayString())
                        .TakeWhile(className => filters.All(regex => !regex.IsMatch(className)))
                        .Count();

                    if (baseTypesCount > MaximumDepth)
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, objectTypeInfo.Identifier.GetLocation(), objectTypeInfo.Name, baseTypesCount, MaximumDepth));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKindEx.RecordDeclaration);

        private static string GetRootNamespace(ISymbol symbol)
        {
            var namespaceString = symbol.ContainingNamespace.ToDisplayString();

            var lastDotIndex = namespaceString.IndexOf(".", StringComparison.Ordinal);
            return lastDotIndex == -1
                ? namespaceString
                : namespaceString.Substring(0, lastDotIndex);
        }

        private static Regex WildcardPatternToRegularExpression(string pattern)
        {
            var regexPattern = string.Concat("^", Regex.Escape(pattern).Replace("\\*", ".*"), "$");
            return new Regex(regexPattern, RegexOptions.Compiled);
        }

        private class ObjectTypeInfo
        {
            public SyntaxToken Identifier { get; }

            public INamedTypeSymbol Symbol { get; }

            public string Name { get; }

            public ObjectTypeInfo(SyntaxNode node, SemanticModel model)
            {
                if (node is ClassDeclarationSyntax classDeclaration)
                {
                    Identifier = classDeclaration.Identifier;
                    Symbol = model.GetDeclaredSymbol(classDeclaration);
                    Name = "class";
                }
                else
                {
                    var wrapper = (RecordDeclarationSyntaxWrapper)node;
                    Identifier = wrapper.Identifier;
                    Symbol = model.GetDeclaredSymbol(wrapper);
                    Name = "record";
                }
            }
        }
    }
}
