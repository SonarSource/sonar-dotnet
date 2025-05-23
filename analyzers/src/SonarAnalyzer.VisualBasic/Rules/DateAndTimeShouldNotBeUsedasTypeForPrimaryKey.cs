﻿/*
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

using static Microsoft.CodeAnalysis.VisualBasic.SyntaxKind;

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey : DateAndTimeShouldNotBeUsedasTypeForPrimaryKeyBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override IEnumerable<SyntaxNode> TypeNodesOfTemporalKeyProperties(SonarSyntaxNodeReportingContext context)
    {
        var classDeclaration = (ClassBlockSyntax)context.Node;
        var className = classDeclaration.ClassStatement.Identifier.ValueText;
        return PropertyDeclarationsInClass(classDeclaration)
            .Where(x => IsCandidateProperty(x)
                        && IsTemporalType(x.AsClause.Type().GetName())
                        && IsKeyProperty(x, className))
            .Select(x => x.AsClause.Type());
    }

    protected override bool IsTemporalType(string propertyTypeName) =>
        propertyTypeName.Equals("Date", Language.NameComparison)
        || base.IsTemporalType(propertyTypeName);

    private static IEnumerable<PropertyStatementSyntax> PropertyDeclarationsInClass(ClassBlockSyntax classDeclaration) =>
        classDeclaration.Members
            .OfType<PropertyStatementSyntax>()
            .Concat(classDeclaration.Members.OfType<PropertyBlockSyntax>().Select(x => x.PropertyStatement));

    private static bool IsCandidateProperty(PropertyStatementSyntax property) =>
        (property.Modifiers.Any(x => x.IsKind(PublicKeyword))
         || property.Modifiers.All(x => !x.IsAnyKind(PrivateKeyword, ProtectedKeyword, FriendKeyword)))
        && property.Modifiers.All(x => !x.IsAnyKind(SharedKeyword, ReadOnlyKeyword, WriteOnlyKeyword));

    private bool IsKeyProperty(PropertyStatementSyntax property, string className)
    {
        var propertyName = property.Identifier.ValueText;
        return IsKeyPropertyBasedOnName(propertyName, className)
            || HasKeyAttribute(property);
    }

    private bool HasKeyAttribute(PropertyStatementSyntax property) =>
        property.AttributeLists
            .SelectMany(x => x.Attributes)
            .Any(x => MatchesAttributeName(x.GetName(), KeyAttributeTypeNames));
}
