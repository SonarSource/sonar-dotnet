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

using System.Text.RegularExpressions;
using SonarAnalyzer.Rules.MessageTemplates;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MessageTemplatesShouldBeCorrect : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6674";
    private const string MessageFormat = "Log message template {0}.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
        {
            if (cc.Compilation.ReferencesAny(KnownAssembly.MicrosoftExtensionsLoggingAbstractions, KnownAssembly.Serilog, KnownAssembly.NLog))
            {
                cc.RegisterNodeAction(c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    if (MessageTemplateExtractor.TemplateArgument(invocation, c.Model) is { } argument
                        && argument.Expression.IsKind(SyntaxKind.StringLiteralExpression)
                        && TemplateValidator.ContainsErrors(argument.Expression.ToString(), out var errors))
                    {
                        var templateStart = argument.Expression.GetLocation().SourceSpan.Start;
                        foreach (var error in errors)
                        {
                            var location = Location.Create(c.Tree, new(templateStart + error.Start, error.Length));
                            c.ReportIssue(Rule, location, error.Message);
                        }
                    }
                },
                SyntaxKind.InvocationExpression);
            }
        });

    private static class TemplateValidator
    {
        private const int EmptyPlaceholderSize = 2; // "{}"

        private const string TextPattern = @"([^\{]|\{\{|\}\})+";
        private const string HolePattern = @"{(?<Placeholder>[^\}]*)}";
        private const string TemplatePattern = $"^({TextPattern}|{HolePattern})*$";
        // This is similar to the regex used for MessageTemplatesAnalyzer, but it is far more permissive.
        // The goal is to manually parse the placeholders, so that we can report more specific issues than just "malformed template".
        private static readonly Regex TemplateRegex = new(TemplatePattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(300));
        private static readonly Regex PlaceholderNameRegex = new("^[0-9a-zA-Z_]+$", RegexOptions.Compiled, Constants.DefaultRegexTimeout);
        private static readonly Regex PlaceholderAlignmentRegex = new("^-?[0-9]+$", RegexOptions.Compiled, Constants.DefaultRegexTimeout);

        public static bool ContainsErrors(string template, out List<ParsingError> errors)
        {
            var result = MessageTemplatesParser.Parse(template, TemplateRegex);
            errors = result.Success
                ? result.Placeholders.Select(ParsePlaceholder).Where(x => x is not null).ToList()
                : [new("should be syntactically correct", 0, template.Length)];
            return errors.Count > 0;
        }

        private static ParsingError ParsePlaceholder(MessageTemplatesParser.Placeholder placeholder)
        {
            if (placeholder.Length == 0)
            {
                return new("should not contain empty placeholder", placeholder.Start - 1, EmptyPlaceholderSize);
            }
            var parts = Split(placeholder.Name);
            return "🔥" switch
            {
                _ when !PlaceholderNameRegex.SafeIsMatch(parts.Name) =>
                    new($"placeholder '{parts.Name}' should only contain letters, numbers, and underscore", placeholder),

                _ when parts.Alignment is not null && !PlaceholderAlignmentRegex.SafeIsMatch(parts.Alignment) =>
                    new($"placeholder '{parts.Name}' should have numeric alignment instead of '{parts.Alignment}'", placeholder),

                _ when parts.Format == string.Empty =>
                    new($"placeholder '{parts.Name}' should not have empty format", placeholder),

                _ => null,
            };
        }

        // pattern is: name[,alignment][:format]
        private static Parts Split(string placeholder)
        {
            string alignment = null;
            string format = null;

            var formatIndex = placeholder.IndexOf(':');
            var alignmentIndex = placeholder.IndexOf(',');
            if (formatIndex >= 0 && alignmentIndex > formatIndex)
            {
                // example {name:format,alignment}
                //               ^^^^^^^^^^^^^^^^ all of this is format, need to reset alignment
                alignmentIndex = -1;
            }

            if (formatIndex == -1)
            {
                formatIndex = placeholder.Length;
            }
            else
            {
                format = placeholder.Substring(formatIndex + 1);
            }
            if (alignmentIndex == -1)
            {
                alignmentIndex = formatIndex;
            }
            else
            {
                alignment = placeholder.Substring(alignmentIndex + 1, formatIndex - alignmentIndex - 1);
            }

            var start = placeholder[0] is '@' or '$' ? 1 : 0; // skip prefix
            var name = placeholder.Substring(start, alignmentIndex - start);
            return new(name, alignment, format);
        }

        private sealed record Parts(string Name, string Alignment, string Format);

        public sealed record ParsingError(string Message, int Start, int Length)
        {
            public ParsingError(string message, MessageTemplatesParser.Placeholder placeholder)
                : this(message, placeholder.Start, placeholder.Length)
            { }
        }
    }
}
