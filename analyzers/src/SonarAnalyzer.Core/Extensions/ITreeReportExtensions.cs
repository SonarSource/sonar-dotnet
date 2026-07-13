/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Extensions;

// Don't change the this parameter to (this IAnalysisContext context) because it would cause boxing
public static class ITreeReportExtensions
{
    extension<T>(T context) where T : ITreeReport
    {
        public void ReportIssue(DiagnosticDescriptor rule,
                                SyntaxNode locationSyntax,
                                params string[] messageArgs) =>
            context.ReportIssue(rule, locationSyntax.GetLocation(), messageArgs);

        public void ReportIssue(DiagnosticDescriptor rule,
                                SyntaxNode locationSyntax,
                                ImmutableDictionary<string, string> properties,
                                params string[] messageArgs) =>
            context.ReportIssue(rule, locationSyntax.GetLocation(), properties, messageArgs);

        public void ReportIssue(DiagnosticDescriptor rule,
                                SyntaxNode primaryLocationSyntax,
                                IEnumerable<SecondaryLocation> secondaryLocations,
                                params string[] messageArgs) =>
            context.ReportIssue(rule, primaryLocationSyntax.GetLocation(), secondaryLocations, messageArgs);

        public void ReportIssue(DiagnosticDescriptor rule,
                                SyntaxToken locationToken,
                                params string[] messageArgs) =>
            context.ReportIssue(rule, locationToken.GetLocation(), messageArgs);

        public void ReportIssue(DiagnosticDescriptor rule,
                                SyntaxToken locationToken,
                                ImmutableDictionary<string, string> properties,
                                params string[] messageArgs) =>
            context.ReportIssue(rule, locationToken.GetLocation(), properties, messageArgs);

        public void ReportIssue(DiagnosticDescriptor rule,
                                SyntaxToken primaryLocationToken,
                                IEnumerable<SecondaryLocation> secondaryLocations,
                                params string[] messageArgs) =>
            context.ReportIssue(rule, primaryLocationToken.GetLocation(), secondaryLocations, messageArgs);

        public void ReportIssue(DiagnosticDescriptor rule,
                                Location location,
                                params string[] messageArgs) =>
            context.ReportIssue(rule, location, [], ImmutableDictionary<string, string>.Empty, messageArgs);

        public void ReportIssue(DiagnosticDescriptor rule,
                                Location location,
                                ImmutableDictionary<string, string> properties,
                                params string[] messageArgs) =>
            context.ReportIssue(rule, location, [], properties, messageArgs);

        public void ReportIssue(DiagnosticDescriptor rule,
                                Location primaryLocation,
                                IEnumerable<SecondaryLocation> secondaryLocations,
                                params string[] messageArgs) =>
            context.ReportIssue(rule, primaryLocation, secondaryLocations, ImmutableDictionary<string, string>.Empty, messageArgs);
    }
}
