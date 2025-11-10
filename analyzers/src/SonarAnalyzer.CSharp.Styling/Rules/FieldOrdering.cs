/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
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
public sealed class FieldOrdering : StylingAnalyzer
{
    public FieldOrdering() : base("T0013", "Move this static field above the {0} instance ones.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            ValidateMembers,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.StructDeclaration);

    private void ValidateMembers(SonarSyntaxNodeReportingContext context)
    {
        var fields = ((TypeDeclarationSyntax)context.Node).Members.OfType<FieldDeclarationSyntax>().Where(x => !x.Modifiers.Any(SyntaxKind.ConstKeyword));
        foreach (var visibilityGroup in fields.GroupBy(x => x.ComputeOrder()))
        {
            ValidateMembers(context, visibilityGroup.Key, visibilityGroup);
        }
    }

    private void ValidateMembers(SonarSyntaxNodeReportingContext context, OrderDescriptor order, IEnumerable<FieldDeclarationSyntax> members)
    {
        var hasInstance = false;
        foreach (var member in members)
        {
            if (member.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                if (hasInstance)
                {
                    context.ReportIssue(Rule, member.Declaration, order.Description);
                }
            }
            else
            {
                hasInstance = true;
            }
        }
    }
}
