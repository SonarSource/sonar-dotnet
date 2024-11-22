/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
public sealed class XmlSignatureCheck : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6377";
    private const string MessageFormat = "Change this code to only accept signatures computed from a trusted party.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        var tracker = CSharpFacade.Instance.Tracker.Invocation;
        tracker.Track(
            new TrackerInput(context, AnalyzerConfiguration.AlwaysEnabled, Rule),
            tracker.Or(
                tracker.And(
                    tracker.MethodHasParameters(0),
                    tracker.MatchMethod(new MemberDescriptor(KnownType.System_Security_Cryptography_Xml_SignedXml, "CheckSignature"))),
                tracker.MatchMethod(new MemberDescriptor(KnownType.System_Security_Cryptography_Xml_SignedXml, "CheckSignatureReturningKey"))));
    }
}
