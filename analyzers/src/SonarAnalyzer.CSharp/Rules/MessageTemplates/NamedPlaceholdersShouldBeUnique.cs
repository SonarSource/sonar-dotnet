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

namespace SonarAnalyzer.Rules.MessageTemplates;

public sealed class NamedPlaceholdersShouldBeUnique : IMessageTemplateCheck
{
    private const string DiagnosticId = "S6677";
    private const string MessageFormat = "Message template placeholder '{0}' is not unique.";

    internal static readonly DiagnosticDescriptor S6677 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public DiagnosticDescriptor Rule => S6677;

    public void Execute(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation, ArgumentSyntax templateArgument, Helpers.MessageTemplates.Placeholder[] placeholders)
    {
        var duplicates = placeholders
            .Where(x => x.Name != "_" && !int.TryParse(x.Name, out var _)) // exclude wildcard "_" and index placeholders like {42}
            .GroupBy(x => x.Name)
            .Where(x => x.Count() > 1).Select(x => x.Skip(1))
            .SelectMany(x => x);

        foreach (var placeholder in duplicates)
        {
            var spanStart = templateArgument.Expression.GetLocation().SourceSpan.Start + placeholder.Start;
            var location = Location.Create(context.Tree, new(spanStart, placeholder.Length));
            context.ReportIssue(Diagnostic.Create(Rule, location, placeholder.Name));
        }
    }
}
