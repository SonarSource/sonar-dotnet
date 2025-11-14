/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypeMemberOrdering : StylingAnalyzer
{
    private const string NestedTypes = "Nested Types";

    private static readonly MemberKind Constant = new(1, "Constants");
    private static readonly MemberKind AbstractMember = new(10, "Abstract Members");
    private static readonly MemberKind AbstractType = new(40, "Abstract Types");

    private static readonly Dictionary<SyntaxKind, MemberKind> MemberKinds = new()
        {
            // Order 1: Constants are FieldDeclaration and are handled separately in the code
            {SyntaxKind.EnumDeclaration, new(2, "Enums") },
            {SyntaxKind.FieldDeclaration, new(3, "Fields") },
            // Order 10: Abstract members are handled separately in the code
            {SyntaxKind.DelegateDeclaration, new(20, "Delegates") },
            {SyntaxKind.EventFieldDeclaration, new(21, "Events") },
            {SyntaxKind.PropertyDeclaration, new(22, "Properties") },
            {SyntaxKind.IndexerDeclaration, new(23, "Indexers") },
            {SyntaxKind.ConstructorDeclaration, new(24, "Constructors") },
            {SyntaxKind.DestructorDeclaration, new(25, "Destructor") },
            {SyntaxKind.MethodDeclaration, new(30, "Methods") },
            {SyntaxKind.ConversionOperatorDeclaration, new(31, "Operators") },
            {SyntaxKind.OperatorDeclaration, new(31, "Operators") },
            // Order 40: Abstract types are handled separtely in the code
            {SyntaxKind.ClassDeclaration, new(40, NestedTypes) },
            {SyntaxKind.InterfaceDeclaration, new(40, NestedTypes) },
            {SyntaxKind.RecordDeclaration, new(40, NestedTypes) },
            {SyntaxKind.StructDeclaration, new(40, NestedTypes) },
        };

    public TypeMemberOrdering() : base("T0008", "Move {0} {1}.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            ValidateMembers,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.StructDeclaration);

    private void ValidateMembers(SonarSyntaxNodeReportingContext context)
    {
        var secondaries = new Dictionary<MemberKind, SecondaryLocation>();
        var type = (TypeDeclarationSyntax)context.Node;
        var members = new List<MemberInfo>();
        foreach (var member in type.Members)
        {
            if (ReportingLocation(member) is { } location && Kind(member) is { } kind)
            {
                members.Add(new(member, location, kind.Order, kind.Description));
                if (!secondaries.ContainsKey(kind))
                {
                    secondaries.Add(kind, location.ToSecondary("Move the declaration before this one."));
                }
            }
        }
        var maxOrder = 0;
        var availableKinds = members.GroupBy(x => x.Order).OrderBy(x => x.Key).Select(x => new MemberKind(x.Key, x.First().Description)).ToArray();
        foreach (var member in members)
        {
            if (member.Order < maxOrder)
            {
                var before = availableKinds.First(x => x.Order > member.Order);
                context.ReportIssue(Rule, member.Location, [secondaries[before]], member.Description, ExpectedLocation(availableKinds, member.Order, before));
            }
            maxOrder = Math.Max(maxOrder, member.Order);
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
            return member is BaseTypeDeclarationSyntax ? AbstractType : AbstractMember;
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

    private static string ExpectedLocation(MemberKind[] availableKinds, int order, MemberKind before)
    {
        var after = availableKinds.LastOrDefault(x => x.Order < order) is { } previous ? $"after {previous.Description}, " : null;
        return $"{after}before {before.Description}";
    }

    private sealed record MemberInfo(MemberDeclarationSyntax Member, Location Location, int Order, string Description);

    private sealed record MemberKind(int Order, string Description);
}
