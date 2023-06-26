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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey : DateAndTimeShouldNotBeUsedasTypeForPrimaryKeyBase<SyntaxKind>
{
    private static readonly string[] KeyAttributeTypeNames = TypeNamesForAttribute(KnownType.System_ComponentModel_DataAnnotations_KeyAttribute);

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override void AnalyzeClass(SonarSyntaxNodeReportingContext context)
    {
        // To improve performance the attributes and property types are only matched by their names, rather than their actual symbol.
        // This results in a couple of FNs, but those scenarios are very rare.
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var className = classDeclaration.Identifier.ValueText;
        var keyProperties = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(x => IsPublicReadWriteProperty(x)
                        && IsTemporalType(x)
                        && IsKeyProperty(x, className));

        foreach (var propertyType in keyProperties.Select(x => x.Type))
        {
            context.ReportIssue(Diagnostic.Create(Rule, propertyType.GetLocation(), propertyType.GetName()));
        }
    }

    private static bool IsPublicReadWriteProperty(PropertyDeclarationSyntax property) =>
        property.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword))
        && property.AccessorList is { } accessorList
        && accessorList.Accessors.Any(x => x.Keyword.IsKind(SyntaxKind.GetKeyword))
        && accessorList.Accessors.Any(x => x.Keyword.IsKind(SyntaxKind.SetKeyword));

    private static bool IsKeyProperty(PropertyDeclarationSyntax property, string className)
    {
        var propertyName = property.Identifier.ValueText;
        return propertyName.Equals("Id", StringComparison.InvariantCultureIgnoreCase)
            || propertyName.Equals($"{className}Id", StringComparison.InvariantCultureIgnoreCase)
            || HasKeyAttribute(property);
    }

    private static bool HasKeyAttribute(PropertyDeclarationSyntax property) =>
        property.AttributeLists
            .SelectMany(x => x.Attributes)
            .Any(x => MatchesAttributeName(x, KeyAttributeTypeNames));

    private static bool IsTemporalType(PropertyDeclarationSyntax property) =>
        Array.Exists(TemporalTypes, x => property.Type.NameIs(x.TypeName) || property.Type.NameIs(x.FullName));

    private static bool MatchesAttributeName(AttributeSyntax attribute, string[] candidates) =>
        Array.Exists(candidates, x => attribute.NameIs(x));
}
