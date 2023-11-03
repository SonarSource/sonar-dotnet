/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterTypeShouldMatchRouteTypeConstraint : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6800";
    private const string MessageFormat = "{0}";
    private const string MessageNoSecondaryLocationFormat = "Parameter type '{0}' does not match route parameter type constraint '{1}' in route '{2}'.";
    private const string MessageWithSecondaryLocationFormat = "Parameter type '{0}' does not match route parameter type constraint.";
    private const string SecondaryMessageFormat = "This route parameter has a '{0}' type constraint.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly Dictionary<string, string> ConstraintMapping = new(StringComparer.InvariantCultureIgnoreCase)
    {
        { "bool", nameof(Boolean) },
        { "datetime", nameof(DateTime) },
        { "decimal", nameof(Decimal) },
        { "double", nameof(Double) },
        { "float", nameof(Single) },
        { "guid", nameof(Guid) },
        { "int", nameof(Int32) },
        { "long", nameof(Int64) },
        { "string", nameof(String) }
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
        {
            // If we are not in a Blazor project, we don't need to register for lambda expressions.
            if (cc.Compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Components_RouteAttribute) is null)
            {
                return;
            }

            cc.RegisterNodeAction(c =>
            {
                var node = (ClassDeclarationSyntax)c.Node;

                foreach (var property in GetPropertyTypeMismatches(node, c.SemanticModel))
                {
                    var additionalLocations = property.RouteParamLocation is not null
                        ? new[] { property.RouteParamLocation }
                        : Array.Empty<Location>();
                    var message = property.RouteParamLocation is not null
                        ? string.Format(MessageWithSecondaryLocationFormat, property.Type.GetName())
                        : string.Format(MessageNoSecondaryLocationFormat, property.Type.GetName(), property.ConstraintType, property.Route);

                    c.ReportIssue(Diagnostic.Create(Rule,
                        property.Type.GetLocation(),
                        additionalLocations,
                        additionalLocations.ToProperties(string.Format(SecondaryMessageFormat, property.ConstraintType)),
                        message));
                }
            },
            SyntaxKind.ClassDeclaration);
        });

    private static IList<PropertyTypeMismatch> GetPropertyTypeMismatches(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        var routeParameters = GetRouteParametersWithValidConstraint(classDeclaration);

        if (routeParameters.Count == 0)
        {
            return Array.Empty<PropertyTypeMismatch>();
        }

        var mismatches = new List<PropertyTypeMismatch>();
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        var properties = classSymbol.GetMembers()
            .Where(member => member.Kind is SymbolKind.Property && member.HasAttribute(KnownType.Microsoft_AspNetCore_Components_ParameterAttribute) && routeParameters.ContainsKey(member.Name));

        foreach (var property in properties)
        {
            foreach (var routeParameter in routeParameters[property.Name])
            {
                if (TypeMatchConstraint(property.GetSymbolType(), routeParameter.Constraint))
                {
                    continue;
                }

                foreach (var propertySyntax in property.DeclaringSyntaxReferences.Where(r => r.GetSyntax() is PropertyDeclarationSyntax).Select(r => (PropertyDeclarationSyntax)r.GetSyntax()))
                {
                    mismatches.Add(new PropertyTypeMismatch(propertySyntax.Type, routeParameter.Constraint, routeParameter.FromRoute, routeParameter.RouteParamLocation));
                }
            }
        }

        return mismatches;
    }

    private static bool TypeMatchConstraint(ISymbol type, string constraintType) =>
        ConstraintMapping.ContainsKey(constraintType) && ConstraintMapping[constraintType] == type.Name;

    private static Dictionary<string, List<RouteParameter>> GetRouteParametersWithValidConstraint(ClassDeclarationSyntax classDeclaration)
    {
        var routeParameters = new Dictionary<string, List<RouteParameter>>(StringComparer.InvariantCultureIgnoreCase);

        var routes = classDeclaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .Where(attr => attr.IsKnownType(KnownType.Microsoft_AspNetCore_Components_RouteAttribute)
                           && attr.ArgumentList.Arguments.Count > 0
                           && attr.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax)
            .Select(attr => (LiteralExpressionSyntax)attr.ArgumentList.Arguments[0].Expression);

        foreach (var route in routes)
        {
            var routeParams = route.Token.ValueText.Split('/').Where(segment => segment.StartsWith("{") && segment.EndsWith("}"));

            foreach (var routeParam in routeParams)
            {
                var routeParameterParts = routeParam.TrimStart('{').TrimEnd('}', '?').Split(':');
                if (routeParameterParts.Length != 2 || !ConstraintMapping.ContainsKey(routeParameterParts[1]))
                {
                    continue;
                }

                var routeParameter = new RouteParameter(routeParameterParts[1], route.Token.ValueText, CalculateRouteParamLocation(route.GetLocation(), route.Token.ValueText, routeParam));
                if (routeParameters.ContainsKey(routeParameterParts[0]))
                {
                    routeParameters[routeParameterParts[0]].Add(routeParameter);
                }
                else
                {
                    routeParameters.Add(routeParameterParts[0], new List<RouteParameter> { routeParameter });
                }
            }
        }

        return routeParameters;
    }

    private static Location CalculateRouteParamLocation(Location routeLocation, string route, string routeParam)
    {
        if (!GeneratedCodeRecognizer.IsRazorGeneratedFile(routeLocation.SourceTree))
        {
            var startIndex = route.IndexOf(routeParam, StringComparison.InvariantCulture);
            return Location.Create(routeLocation.SourceTree, new TextSpan(routeLocation.SourceSpan.Start + startIndex + 1, routeParam.Length));
        }

        return null;
    }

    private sealed record PropertyTypeMismatch(TypeSyntax Type, string ConstraintType, string Route, Location RouteParamLocation);

    private sealed record RouteParameter(string Constraint, string FromRoute, Location RouteParamLocation);
}
