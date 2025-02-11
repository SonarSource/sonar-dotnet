/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Text.RegularExpressions;

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AvoidExcessiveInheritance : ParametrizedDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S110";
        private const string MessageFormat = "This {0} has {1} parents which is greater than {2} authorized.";
        private const string FilteredClassesDefaultValue = "";
        private const int MaximumDepthDefaultValue = 5;
        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, false);
        private string filteredClasses = FilteredClassesDefaultValue;
        private ICollection<Regex> filters = new List<Regex>();
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
                filters = filteredClasses.Split(',').Select(WildcardPatternToRegularExpression).ToList();
            }
        }

        protected override void Initialize(SonarParametrizedAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    if (c.IsRedundantPositionalRecordContext())
                    {
                        return;
                    }
                    var objectTypeInfo = new ObjectTypeInfo(c.Node, c.Model);
                    if (objectTypeInfo.Symbol is null)
                    {
                        return;
                    }

                    var thisTypeRootNamespace = GetRootNamespace(objectTypeInfo.Symbol);
                    var baseTypesCount = objectTypeInfo.Symbol.BaseType.GetSelfAndBaseTypes()
                        .TakeWhile(s => GetRootNamespace(s) == thisTypeRootNamespace)
                        .Select(nts => nts.OriginalDefinition.ToDisplayString())
                        .TakeWhile(className => filters.All(regex => !regex.SafeIsMatch(className)))
                        .Count();
                    if (baseTypesCount > MaximumDepth)
                    {
                        c.ReportIssue(Rule, objectTypeInfo.Identifier, objectTypeInfo.Name, baseTypesCount.ToString(), MaximumDepth.ToString());
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKindEx.RecordDeclaration);

        private static string GetRootNamespace(ISymbol symbol)
        {
            var ns = symbol.ContainingNamespace;
            while (ns?.ContainingNamespace?.IsGlobalNamespace is false)
            {
                ns = ns.ContainingNamespace;
            }
            return ns?.Name ?? string.Empty;
        }

        private static Regex WildcardPatternToRegularExpression(string pattern)
        {
            var regexPattern = string.Concat("^", Regex.Escape(pattern).Replace("\\*", ".*"), "$");
            return new Regex(regexPattern, RegexOptions.Compiled, Constants.DefaultRegexTimeout);
        }

        private readonly struct ObjectTypeInfo
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
