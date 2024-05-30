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

using static SonarAnalyzer.Helpers.MessageTemplatesParser;

namespace SonarAnalyzer.Rules.MessageTemplates;

public sealed class UsePascalCaseForNamedPlaceHolders : IMessageTemplateCheck
{
    private const string DiagnosticId = "S6678";
    private const string MessageFormat = "Use PascalCase for named placeholders.";

    internal static readonly DiagnosticDescriptor S6678 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public DiagnosticDescriptor Rule => S6678;

    public void Execute(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation, ArgumentSyntax templateArgument, Placeholder[] placeholders)
    {
        var nonPascalCasePlaceholders = placeholders.Where(x => char.IsLower(x.Name[0])).ToArray();
        if (nonPascalCasePlaceholders.Length > 0)
        {
            context.ReportIssue(Rule, templateArgument, nonPascalCasePlaceholders.Select(CreateLocation));
        }

        SecondaryLocation CreateLocation(Placeholder placeholder) =>
            Location.Create(context.Tree, new(templateArgument.Expression.GetLocation().SourceSpan.Start + placeholder.Start, placeholder.Length)).ToSecondary();
    }
}
