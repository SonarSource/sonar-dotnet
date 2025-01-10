/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules.MessageTemplates;

public sealed class NamedPlaceholdersShouldBeUnique : IMessageTemplateCheck
{
    private const string DiagnosticId = "S6677";
    private const string MessageFormat = "Message template placeholder '{0}' is not unique.";

    internal static readonly DiagnosticDescriptor S6677 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public DiagnosticDescriptor Rule => S6677;

    public void Execute(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation, ArgumentSyntax templateArgument, MessageTemplatesParser.Placeholder[] placeholders)
    {
        var duplicatedGroups = placeholders
            .Where(x => x.Name != "_" && !int.TryParse(x.Name, out _)) // exclude wildcard "_" and index placeholders like {42}
            .GroupBy(x => x.Name)
            .Where(x => x.Count() > 1);

        foreach (var group in duplicatedGroups)
        {
            var templateStart = templateArgument.Expression.GetLocation().SourceSpan.Start;
            var locations = group.Select(x => Location.Create(context.Tree, new(templateStart + x.Start, x.Length)));
            context.ReportIssue(Rule, locations.First(), locations.Skip(1).ToSecondary(), group.First().Name);
        }
    }
}
