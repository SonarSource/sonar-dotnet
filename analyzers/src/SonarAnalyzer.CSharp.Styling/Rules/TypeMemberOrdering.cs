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
public sealed class TypeMemberOrdering : StylingAnalyzer
{
    private const string NestedTypes = "Nested Types";

    private static readonly MemberKind Constant = new(1, "Constants");
    private static readonly MemberKind Abstract = new(10, "Abstract Members");
    private static readonly Dictionary<SyntaxKind, MemberKind> MemberKinds = new()
        {
            // Order 1: Constants are FieldDeclaration and are handled separately in the code
            {SyntaxKind.EnumDeclaration, new(2, "Fields") },
            {SyntaxKind.FieldDeclaration, new(3, "Fields") },
            {SyntaxKind.EventFieldDeclaration, new(3, "Fields") },
            // Order 10: Abstract members are handled separately in the code
            {SyntaxKind.EventDeclaration, new(20, "Events") },
            {SyntaxKind.IndexerDeclaration, new(20, "Indexers") },
            {SyntaxKind.PropertyDeclaration, new(20, "Properties") },
            {SyntaxKind.ConstructorDeclaration, new(21, "Constructors") },
            {SyntaxKind.DestructorDeclaration, new(22, "Destructors") },
            {SyntaxKind.MethodDeclaration, new(30, "Methods") },
            {SyntaxKind.ConversionOperatorDeclaration, new(31, "Operators") },
            {SyntaxKind.OperatorDeclaration, new(31, "Operators") },
            {SyntaxKind.ClassDeclaration, new(40, NestedTypes) },
            {SyntaxKind.InterfaceDeclaration, new(40, NestedTypes) },
            {SyntaxKind.RecordDeclaration, new(40, NestedTypes) },
            {SyntaxKind.StructDeclaration, new(40, NestedTypes) },
        };

    public TypeMemberOrdering() : base("T0008", "Move {0} {1}") { }

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
        var members = new List<MemberInfo>();
        foreach (var member in type.Members)
        {
            if (ReportingLocation(member) is { } location && Kind(member) is { } kind)
            {
                members.Add(new(member, location, kind.Order, kind.Description));
            }
        }
        var maxOrder = 0;
        foreach (var member in members)
        {
            context.ReportIssue(Rule, member.Location, member.Description, "FIXME: before/after/after and before");
        }
    }

    private static MemberKind Kind(MemberDeclarationSyntax member)
    {
        if (member.IsKind(SyntaxKind.FieldDeclaration) && member.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            return Constant;
        }
        else if (member.Modifiers.Any(SyntaxKind.AbstractKeyword))
        {
            return Abstract;
        }
        else if (MemberKinds.TryGetValue(member.Kind(), out var kind))
        {
            return kind;
        }
        else
        {
            return null;
        }
    }

    private static Location ReportingLocation(SyntaxNode node) =>
        node switch
        {
            EventFieldDeclarationSyntax eventField => eventField.Declaration.GetLocation(),
            FieldDeclarationSyntax field => field.Declaration.GetLocation(),
            _ => node.GetIdentifier()?.GetLocation()
        };

    private sealed record MemberInfo(MemberDeclarationSyntax Member, Location Location, int Order, string Description);

    private sealed record MemberKind(int Order, string Description);
}
