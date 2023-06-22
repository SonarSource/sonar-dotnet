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
public sealed class TimestampsShouldNotBeUsedAsPrimaryKeys : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3363";
    private const string MessageFormat = "'{0}' should not be used as a type for primary keys";

    private static readonly KnownType[] TemporalTypes = new[]
    {
        KnownType.System_DateOnly,
        KnownType.System_DateTime,
        KnownType.System_DateTimeOffset,
        KnownType.System_TimeSpan,
        KnownType.System_TimeOnly
    };
    private static readonly string[] KeyAttributeTypeNames = TypeNamesForAttribute(KnownType.System_ComponentModel_DataAnnotations_KeyAttribute);
    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(start =>
        {
            if (!IsReferencingEntityFramework(start.Compilation))
            {
                return;
            }

            context.RegisterNodeAction(c =>
            {
                // To improve performance the attributes and property types are only matched by their names, rather than their actual symbol.
                // This results in a couple of FNs, but those scenarios are very rare.
                var classDeclaration = (ClassDeclarationSyntax)c.Node;
                var className = classDeclaration.Identifier.ValueText;
                var keyProperties = classDeclaration.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .Where(x => IsPublicReadWriteProperty(x) && IsTemporalType(x) && IsKeyProperty(x, className));

                foreach (var keyProperty in keyProperties)
                {
                    c.ReportIssue(Diagnostic.Create(Rule, keyProperty.Type.GetLocation(), keyProperty.Type.GetName()));
                }
            }, SyntaxKind.ClassDeclaration);
        });

    private static bool IsReferencingEntityFramework(Compilation compilation) =>
        compilation.GetTypeByMetadataName(KnownType.Microsoft_EntityFrameworkCore_DbContext) is not null
        || compilation.GetTypeByMetadataName(KnownType.Microsoft_EntityFramework_DbContext) is not null;

    private static bool IsPublicReadWriteProperty(PropertyDeclarationSyntax property) =>
        property.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword))
        && property.AccessorList is not null
        && property.AccessorList.Accessors.Any(x => x.Keyword.IsKind(SyntaxKind.GetKeyword))
        && property.AccessorList.Accessors.Any(x => x.Keyword.IsKind(SyntaxKind.SetKeyword));

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

    private static string[] TypeNamesForAttribute(KnownType attributeType) => new[]
    {
        attributeType.TypeName,
        RemoveFromEnd(attributeType.TypeName, "Attribute"),
        attributeType.FullName,
        RemoveFromEnd(attributeType.FullName, "Attribute"),
    };

    private static string RemoveFromEnd(string text, string subtextFromEnd) =>
        text.Substring(0, text.LastIndexOf(subtextFromEnd));

    private static bool IsTemporalType(PropertyDeclarationSyntax property) =>
        Array.Exists(TemporalTypes, x => property.Type.NameIs(x.TypeName) || property.Type.NameIs(x.FullName));

    private static bool MatchesAttributeName(AttributeSyntax attribute, string[] candidates) =>
        Array.Exists(candidates, x => attribute.NameIs(x));
}
