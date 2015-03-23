using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NSonarQubeAnalyzer.Diagnostics
{
    public class StaticFieldInGenericClass : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2743";
        internal const string Description = @"Static fields should not be used in generic types";
        internal const string MessageFormat = @"A static field in a generic type is not shared among instances of different close constructed types.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;


        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;

                    if (classDeclaration.TypeParameterList == null ||
                        classDeclaration.TypeParameterList.Parameters.Count < 1)
                    {
                        return;
                    }
                    
                    var typeParameterNames =
                        classDeclaration.TypeParameterList.Parameters.Select(p => p.Identifier.ToString()).ToList();

                    var fields = classDeclaration.Members
                        .OfType<FieldDeclarationSyntax>()
                        .Where(f => f.Modifiers.Any(m => m.Text == "static"))
                        .ToList();

                    fields.ForEach(field => ReportMember(field, field.Declaration.Type, typeParameterNames, c));

                    var properties = classDeclaration.Members
                        .OfType<PropertyDeclarationSyntax>()
                        .Where(p => p.Modifiers.Any(m => m.Text == "static"))
                        .ToList();

                    properties.ForEach(property => ReportMember(property, property.Type, typeParameterNames, c));
                },
                SyntaxKind.ClassDeclaration);
        }

        private static void ReportMember(SyntaxNode node, TypeSyntax type, List<string> typeParameterNames, SyntaxNodeAnalysisContext c)
        {
            var genericTypeName = type as GenericNameSyntax;

            if (genericTypeName != null &&
                GetTypeArguments(genericTypeName)
                    .Intersect(typeParameterNames)
                    .Any())
            {
                return;
            }

            c.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
        }

        private static IEnumerable<string> GetTypeArguments(GenericNameSyntax genericTypeName)
        {
            var typeParameters = new List<string>();

            var arguments = genericTypeName.TypeArgumentList.Arguments;

            foreach (var typeSyntax in arguments)
            {
                var innerGenericTypeName = typeSyntax as GenericNameSyntax;
                if (innerGenericTypeName != null)
                {
                    typeParameters.AddRange(GetTypeArguments(innerGenericTypeName));
                }
                else
                {
                    typeParameters.Add(typeSyntax.ToString());
                }
            }

            return typeParameters;
        }
    }
}