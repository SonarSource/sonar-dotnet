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
    private const string MessageFormat = "Timestamps should not be used as primary keys";

    private static readonly KnownType[] TemporalTypes = new[]
    {
        KnownType.System_DateTime,
        KnownType.System_DateTimeOffset,
        KnownType.System_TimeSpan
    };
    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
        {
            var classDeclaration = (ClassDeclarationSyntax)c.Node;
            var className = classDeclaration.Identifier.ValueText;
            var keyProperties = classDeclaration.Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(x => IsKeyProperty(x, className, c.SemanticModel) && IsTemporalType(x));

            foreach (var keyProperty in keyProperties)
            {
                c.ReportIssue(Diagnostic.Create(Rule, keyProperty.Type.GetLocation()));
            }
        },
            SyntaxKind.ClassDeclaration);

    private static bool IsKeyProperty(PropertyDeclarationSyntax property, string className, SemanticModel semanticModel)
    {
        var propertyName = property.Identifier.ValueText;
        return propertyName.Equals("Id", StringComparison.InvariantCultureIgnoreCase)
        || propertyName.Equals($"{className}Id", StringComparison.InvariantCultureIgnoreCase)
        || HasKeyAttribute(property, semanticModel);
    }

    private static bool HasKeyAttribute(PropertyDeclarationSyntax property, SemanticModel semanticModel) =>
        property.AttributeLists
            .SelectMany(x => x.Attributes)
            .Any(x => x.IsKnownType(KnownType.System_ComponentModel_DataAnnotations_KeyAttribute, semanticModel));

    private static bool IsTemporalType(PropertyDeclarationSyntax property) =>
        Array.Exists(TemporalTypes, x => property.Type.NameIs(x.TypeName) || property.Type.NameIs(x.FullName));
}
