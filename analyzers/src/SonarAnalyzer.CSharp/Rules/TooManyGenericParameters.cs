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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TooManyGenericParameters : ParametrizedDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2436";
        private const string MessageFormat = "Reduce the number of generic parameters in the '{0}' {1} to no more than the {2} authorized.";
        private const int DefaultMaxNumberOfGenericParametersInClass = 2;
        private const int DefaultMaxNumberOfGenericParametersInMethod = 3;

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        [RuleParameter("max", PropertyType.Integer, "Maximum authorized number of generic parameters.", DefaultMaxNumberOfGenericParametersInClass)]
        public int MaxNumberOfGenericParametersInClass { get; set; } = DefaultMaxNumberOfGenericParametersInClass;

        [RuleParameter("maxMethod", PropertyType.Integer, "Maximum authorized number of generic parameters for methods.", DefaultMaxNumberOfGenericParametersInMethod)]
        public int MaxNumberOfGenericParametersInMethod { get; set; } = DefaultMaxNumberOfGenericParametersInMethod;

        protected override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var typeDeclaration = (TypeDeclarationSyntax)c.Node;

                    if (c.IsRedundantPositionalRecordContext()
                        || typeDeclaration.TypeParameterList == null
                        || typeDeclaration.TypeParameterList.Parameters.Count <= MaxNumberOfGenericParametersInClass)
                    {
                        return;
                    }

                    c.ReportIssue(Rule, typeDeclaration.Identifier, typeDeclaration.Identifier.ValueText, typeDeclaration.GetDeclarationTypeName(), MaxNumberOfGenericParametersInClass.ToString());
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKindEx.RecordDeclaration,
                SyntaxKindEx.RecordStructDeclaration);

            context.RegisterNodeAction(
                c =>
                {
                    var methodDeclaration = MethodDeclarationFactory.Create(c.Node);
                    if (methodDeclaration.TypeParameterList == null
                        || methodDeclaration.TypeParameterList.Parameters.Count <= MaxNumberOfGenericParametersInMethod)
                    {
                        return;
                    }

                    c.ReportIssue(
                        Rule,
                        methodDeclaration.Identifier,
                        new[] { EnclosingTypeName(c.Node), methodDeclaration.Identifier.ValueText }.JoinNonEmpty("."),
                        "method",
                        MaxNumberOfGenericParametersInMethod.ToString());
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKindEx.LocalFunctionStatement);
        }

        private static string EnclosingTypeName(SyntaxNode node) =>
            node.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault()?.Identifier.ValueText;
    }
}
