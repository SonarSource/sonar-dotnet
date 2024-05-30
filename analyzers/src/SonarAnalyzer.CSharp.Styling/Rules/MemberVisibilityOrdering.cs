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

namespace SonarAnalyzer.Rules.CSharp.Styling;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MemberVisibilityOrdering : StylingAnalyzer
{
    public MemberVisibilityOrdering() : base("T0009", "Move this {0} {1} above the {2} ones.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            ValidateMembers,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.StructDeclaration);

    private void ValidateMembers(SonarSyntaxNodeReportingContext context)
    {
        var type = (TypeDeclarationSyntax)context.Node;
        var members = new Dictionary<string, List<MemberInfo>>();
        foreach (var member in type.Members)
        {
            if (Category(member) is { } category && ReportingLocation(member) is { } location)
            {
                members.GetOrAdd(category, x => []).Add(new(location, member.ComputeOrder()));
            }
        }
        foreach (var category in members.Keys)
        {
            OrderDescriptor maxOrder = null;
            SecondaryLocation secondary = null;
            foreach (var member in members[category])
            {
                if (member.Order.Value < maxOrder?.Value)
                {
                    context.ReportIssue(Rule, member.Location, [secondary], member.Order.Description, category, maxOrder.Description);
                }
                if (maxOrder is null || member.Order.Value > maxOrder.Value)
                {
                    maxOrder = member.Order;
                    secondary = member.Location.ToSecondary();
                }
            }
        }
    }

    private static string Category(MemberDeclarationSyntax member) =>
        member switch
        {
            _ when member.Modifiers.Any(SyntaxKind.AbstractKeyword) => "Abstract Member",
            FieldDeclarationSyntax when member.Modifiers.Any(SyntaxKind.ConstKeyword) => "Constant",
            EnumDeclarationSyntax => "Enum",
            FieldDeclarationSyntax => "Field",
            DelegateDeclarationSyntax => "Delegate",
            EventFieldDeclarationSyntax => "Event",
            PropertyDeclarationSyntax => "Property",
            IndexerDeclarationSyntax => "Indexer",
            ConstructorDeclarationSyntax => "Constructor",
            MethodDeclarationSyntax => "Method",
            _ => null
        };

    private static Location ReportingLocation(SyntaxNode node) =>
        node switch
        {
            EventFieldDeclarationSyntax eventField => eventField.Declaration.GetLocation(),
            FieldDeclarationSyntax field => field.Declaration.GetLocation(),
            _ => node.GetIdentifier()?.GetLocation()
        };

    private sealed record MemberInfo(Location Location, OrderDescriptor Order);
}
