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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InsecureContentSecurityPolicy : TrackerHotspotDiagnosticAnalyzer<SyntaxKind>
{
    private const string DiagnosticId = "S7039";
    private const string MessageFormat = "Content Security Policies should be restrictive to mitigate the risk of content injection attacks.";

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    public InsecureContentSecurityPolicy() : base(AnalyzerConfiguration.AlwaysEnabled, DiagnosticId, MessageFormat) { }

    public InsecureContentSecurityPolicy(IAnalyzerConfiguration configuration) : base(configuration, DiagnosticId, MessageFormat) { }

    protected override void Initialize(TrackerInput input)
    {
        var propertyTracker = Language.Tracker.PropertyAccess;
        propertyTracker.Track(input,
            propertyTracker.MatchProperty(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Http_IHeaderDictionary, "ContentSecurityPolicy")),
            x => IsInsecureContentSecurityPolicyValue((string)propertyTracker.AssignedValue(x)));

        var elementAccessTracker = Language.Tracker.ElementAccess;
        elementAccessTracker.Track(input,
            elementAccessTracker.MatchProperty(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Http_HttpResponse, "Headers")),
            elementAccessTracker.ArgumentAtIndexEquals(0, "Content-Security-Policy"),
            x => IsInsecureContentSecurityPolicyValue((string)elementAccessTracker.AssignedValue(x)));

        var invocationTracker = Language.Tracker.Invocation;
        invocationTracker.Track(input,
            invocationTracker.MatchMethod(
                new MemberDescriptor(KnownType.System_Collections_Generic_IDictionary_TKey_TValue, "Add"),
                new MemberDescriptor(KnownType.Microsoft_AspNetCore_Http_HeaderDictionaryExtensions, "Append")),
            invocationTracker.ArgumentAtIndexIsAny(0, "Content-Security-Policy"),
            invocationTracker.ArgumentAtIndexIs(1, IsInsecureValue));
    }

    private static bool IsInsecureValue(SyntaxNode argumentNode, SemanticModel model) =>
        IsInsecureContentSecurityPolicyValue(((ArgumentSyntax)argumentNode).Expression, model);

    private static bool IsInsecureContentSecurityPolicyValue(SyntaxNode node, SemanticModel model) =>
        node switch
        {
            LiteralExpressionSyntax literal => IsInsecureContentSecurityPolicyValue(literal.Token.ValueText),
            InterpolatedStringExpressionSyntax interpolatedString => interpolatedString.InterpolatedTextValue(model) is { } value && IsInsecureContentSecurityPolicyValue(value),
            _ when node.FindConstantValue(model) is string constantValue => IsInsecureContentSecurityPolicyValue(constantValue),
            _ => false,
        };

    private static bool IsInsecureContentSecurityPolicyValue(string value) =>
        value is not null
        && (value.Contains('*')
            || value.Contains("'unsafe-inline'")
            || value.Contains("'unsafe-hashes'")
            || value.Contains("'unsafe-eval'"));
}
