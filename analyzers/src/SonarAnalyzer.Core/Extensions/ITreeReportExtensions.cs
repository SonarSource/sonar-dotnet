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

namespace SonarAnalyzer.Core.AnalysisContext;

public static class ITreeReportExtensions
{
    public static void ReportIssue<T>(this T context,
                                      DiagnosticDescriptor rule,
                                      SyntaxNode locationSyntax,
                                      params string[] messageArgs) where T : ITreeReport =>
        context.ReportIssue(rule, locationSyntax.GetLocation(), messageArgs);

    public static void ReportIssue<T>(this T context,
                                      DiagnosticDescriptor rule,
                                      SyntaxNode locationSyntax,
                                      ImmutableDictionary<string, string> properties,
                                      params string[] messageArgs) where T : ITreeReport =>
        context.ReportIssue(rule, locationSyntax.GetLocation(), properties, messageArgs);

    public static void ReportIssue<T>(this T context,
                                      DiagnosticDescriptor rule,
                                      SyntaxNode primaryLocationSyntax,
                                      IEnumerable<SecondaryLocation> secondaryLocations,
                                      params string[] messageArgs) where T : ITreeReport =>
        context.ReportIssue(rule, primaryLocationSyntax.GetLocation(), secondaryLocations, messageArgs);

    public static void ReportIssue<T>(this T context,
                                      DiagnosticDescriptor rule,
                                      SyntaxToken locationToken,
                                      params string[] messageArgs) where T : ITreeReport =>
        context.ReportIssue(rule, locationToken.GetLocation(), messageArgs);

    public static void ReportIssue<T>(this T context,
                                      DiagnosticDescriptor rule,
                                      SyntaxToken locationToken,
                                      ImmutableDictionary<string, string> properties,
                                      params string[] messageArgs) where T : ITreeReport =>
        context.ReportIssue(rule, locationToken.GetLocation(), properties, messageArgs);

    public static void ReportIssue<T>(this T context,
                                      DiagnosticDescriptor rule,
                                      SyntaxToken primaryLocationToken,
                                      IEnumerable<SecondaryLocation> secondaryLocations,
                                      params string[] messageArgs) where T : ITreeReport =>
        context.ReportIssue(rule, primaryLocationToken.GetLocation(), secondaryLocations, messageArgs);

    public static void ReportIssue<T>(this T context,
                                      DiagnosticDescriptor rule,
                                      Location location,
                                      params string[] messageArgs) where T : ITreeReport =>
        context.ReportIssue(rule, location, [], ImmutableDictionary<string, string>.Empty, messageArgs);

    public static void ReportIssue<T>(this T context,
                                      DiagnosticDescriptor rule,
                                      Location location,
                                      ImmutableDictionary<string, string> properties,
                                      params string[] messageArgs) where T : ITreeReport =>
        context.ReportIssue(rule, location, [], properties, messageArgs);

    public static void ReportIssue<T>(this T context,
                                      DiagnosticDescriptor rule,
                                      Location primaryLocation,
                                      IEnumerable<SecondaryLocation> secondaryLocations,
                                      params string[] messageArgs) where T : ITreeReport =>
        context.ReportIssue(rule, primaryLocation, secondaryLocations, ImmutableDictionary<string, string>.Empty, messageArgs);
}
