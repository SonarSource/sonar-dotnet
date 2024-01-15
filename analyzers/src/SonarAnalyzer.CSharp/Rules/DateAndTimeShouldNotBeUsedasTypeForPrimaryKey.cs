/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override IEnumerable<SyntaxNode> TypeNodesOfTemporalKeyProperties(SonarSyntaxNodeReportingContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        if (classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword)))
        {
            return Enumerable.Empty<SyntaxNode>();
        }

        var className = classDeclaration.Identifier.ValueText;
        return classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(x => IsCandidateProperty(x)
                        && IsTemporalType(x.Type.GetName())
                        && IsKeyProperty(x, className))
            .Select(x => x.Type);
    }

    private static bool IsCandidateProperty(PropertyDeclarationSyntax property) =>
        property.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword))
        && !property.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword))
        && property.AccessorList is { } accessorList
        && accessorList.Accessors.Any(x => x.Keyword.IsKind(SyntaxKind.GetKeyword))
        && accessorList.Accessors.Any(x => x.Keyword.IsKind(SyntaxKind.SetKeyword));

    private bool IsKeyProperty(PropertyDeclarationSyntax property, string className)
    {
        var propertyName = property.Identifier.ValueText;
        return IsKeyPropertyBasedOnName(propertyName, className)
            || HasKeyAttribute(property);
    }

    private bool HasKeyAttribute(PropertyDeclarationSyntax property) =>
        property.AttributeLists
            .SelectMany(x => x.Attributes)
            .Any(x => MatchesAttributeName(x.GetName(), KeyAttributeTypeNames));
}
