/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
                    if (c.Node is BaseMethodDeclarationSyntax baseMethodDeclaration
                        && c.ContainingSymbol is IMethodSymbol { IsPubliclyAccessible: true, IsOverride: false, MethodKind: MethodKind.Ordinary or MethodKind.Constructor } methodSymbol)
                    {
                        var methodType = methodSymbol.IsConstructor ? "constructor" : "method";
                        if (baseMethodDeclaration is MethodDeclarationSyntax methodDeclaration)
                        {
                            ReportIfListT(c, methodSymbol.ReturnType, methodDeclaration.ReturnType, methodType);
                        }
                        var parameterSyntaxes = baseMethodDeclaration.ParameterList?.Parameters ?? default;
                        for (var i = 0; i < Math.Min(methodSymbol.Parameters.Length, parameterSyntaxes.Count); i++)
                        {
                            ReportIfListT(c, methodSymbol.Parameters[i].Type, parameterSyntaxes[i].Type, methodType);
                        }
                    }
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration);

            context.RegisterNodeAction(
                c =>
                {
                    if (c.Node is PropertyDeclarationSyntax propertyDeclaration
                        && c.ContainingSymbol is IPropertySymbol { IsPubliclyAccessible: true, IsOverride: false } propertySymbol
                        && !HasXmlElementAttribute(propertySymbol))
                    {
                        ReportIfListT(c, propertySymbol.Type, propertyDeclaration.Type, "property");
                    }
                },
                SyntaxKind.PropertyDeclaration);

            context.RegisterNodeAction(
                c =>
                {
                    if (c.Node is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax fieldDeclaration } declaration }
                        && declaration.Variables[0] == c.Node
                        && c.ContainingSymbol is IFieldSymbol { IsPubliclyAccessible: true } fieldSymbol
                        && !HasXmlElementAttribute(fieldSymbol))
                    {
                        ReportIfListT(c, fieldSymbol.Type, fieldDeclaration.Declaration.Type, "field");
                    }
                },
                SyntaxKind.VariableDeclarator);
        }

        private static void ReportIfListT(SonarSyntaxNodeReportingContext context, ITypeSymbol typeSymbol, TypeSyntax typeSyntax, string memberType)
        {
            if (typeSyntax is not null && typeSymbol.Is(KnownType.System_Collections_Generic_List_T))
            {
                context.ReportIssue(Rule, typeSyntax, memberType);
            }
        }

        private static bool HasXmlElementAttribute(ISymbol symbol) =>
            symbol.GetAttributes(KnownType.System_Xml_Serialization_XmlElementAttribute).Any();
    }
}
