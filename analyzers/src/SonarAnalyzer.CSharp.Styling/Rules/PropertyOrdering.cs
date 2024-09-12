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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PropertyOrdering : StylingAnalyzer
{
    public PropertyOrdering() : base("T0014", "Move this static property above the {0} instance ones.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            ValidateMembers,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.StructDeclaration);

    private void ValidateMembers(SonarSyntaxNodeReportingContext context)
    {
        foreach (var visibilityGroup in ((TypeDeclarationSyntax)context.Node).Members.OfType<PropertyDeclarationSyntax>().GroupBy(x => x.ComputeOrder()))
        {
            ValidateMembers(context, visibilityGroup.Key, visibilityGroup);
        }
    }

    private void ValidateMembers(SonarSyntaxNodeReportingContext context, OrderDescriptor order, IEnumerable<PropertyDeclarationSyntax> members)
    {
        var hasInstance = false;
        foreach (var member in members)
        {
            if (member.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                if (hasInstance)
                {
                    context.ReportIssue(Rule, member.Identifier, order.Description);
                }
            }
            else
            {
                hasInstance = true;
            }
        }
    }
}
