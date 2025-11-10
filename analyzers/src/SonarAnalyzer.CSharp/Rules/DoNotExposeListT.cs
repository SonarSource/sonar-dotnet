/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
    public sealed class DoNotExposeListT : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3956";
        private const string MessageFormat = "Refactor this {0} to use a generic collection designed for inheritance.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var baseMethodDeclaration = (BaseMethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.Model.GetDeclaredSymbol(baseMethodDeclaration);

                    if (methodSymbol == null
                        || !methodSymbol.IsPubliclyAccessible()
                        || methodSymbol.IsOverride
                        || !IsOrdinaryMethodOrConstructor(methodSymbol))
                    {
                        return;
                    }

                    var methodType = methodSymbol.IsConstructor() ? "constructor" : "method";

                    if (baseMethodDeclaration is MethodDeclarationSyntax methodDeclaration)
                    {
                        ReportIfListT(c, methodDeclaration.ReturnType, methodType);
                    }

                    baseMethodDeclaration
                        .ParameterList?
                        .Parameters
                        .ToList()
                        .ForEach(p => ReportIfListT(c, p.Type, methodType));
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration);

            context.RegisterNodeAction(
                c =>
                {
                    var propertyDeclaration = (PropertyDeclarationSyntax)c.Node;
                    var propertySymbol = c.Model.GetDeclaredSymbol(propertyDeclaration);

                    if (propertySymbol != null
                        && propertySymbol.IsPubliclyAccessible()
                        && !propertySymbol.IsOverride
                        && !HasXmlElementAttribute(propertySymbol))
                    {
                        ReportIfListT(c, propertyDeclaration.Type, "property");
                    }
                },
                SyntaxKind.PropertyDeclaration);

            context.RegisterNodeAction(
                c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;

                    var variableDeclaration = fieldDeclaration.Declaration?.Variables.FirstOrDefault();
                    if (variableDeclaration == null)
                    {
                        return;
                    }

                    var fieldSymbol = c.Model.GetDeclaredSymbol(variableDeclaration);

                    if (fieldSymbol != null
                        && fieldSymbol.IsPubliclyAccessible()
                        && !HasXmlElementAttribute(fieldSymbol))
                    {
                        ReportIfListT(c, fieldDeclaration.Declaration.Type, "field");
                    }
                },
                SyntaxKind.FieldDeclaration);
        }

        private static void ReportIfListT(SonarSyntaxNodeReportingContext context, TypeSyntax typeSyntax, string memberType)
        {
            if (typeSyntax != null && typeSyntax.IsKnownType(KnownType.System_Collections_Generic_List_T, context.Model))
            {
                context.ReportIssue(Rule, typeSyntax, memberType);
            }
        }

        private static bool IsOrdinaryMethodOrConstructor(IMethodSymbol method) =>
            method.MethodKind == MethodKind.Ordinary || method.MethodKind == MethodKind.Constructor;

        private static bool HasXmlElementAttribute(ISymbol symbol) =>
            symbol.GetAttributes(KnownType.System_Xml_Serialization_XmlElementAttribute).Any();
    }
}
