/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
using SonarAnalyzer.CSharp.Core.Trackers;

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CookieShouldBeHttpOnly : ObjectShouldBeInitializedCorrectlyBase
    {
        private const string DiagnosticId = "S3330";
        private const string MessageFormat = "Make sure creating this cookie without the \"HttpOnly\" flag is safe.";

        private static readonly ImmutableArray<KnownType> TrackedTypes =
            ImmutableArray.Create(
                KnownType.System_Web_HttpCookie,
                KnownType.Microsoft_AspNetCore_Http_CookieOptions);

        protected override CSharpObjectInitializationTracker ObjectInitializationTracker { get; } = new(
            isAllowedConstantValue: constantValue => constantValue is true,
            trackedTypes: TrackedTypes,
            isTrackedPropertyName: propertyName => propertyName == "HttpOnly");

        public CookieShouldBeHttpOnly() : this(AnalyzerConfiguration.Hotspot) { }

        internal CookieShouldBeHttpOnly(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration, DiagnosticId, MessageFormat) { }

        protected override bool IsDefaultConstructorSafe(SonarCompilationStartAnalysisContext context) =>
            IsWebConfigCookieSet(context, "httpOnlyCookies");

        protected override void Initialize(TrackerInput input)
        {
            var t = CSharpFacade.Instance.Tracker.ObjectCreation;
            t.Track(input,
                t.MatchConstructor(KnownType.Nancy_Cookies_NancyCookie),
                t.ExceptWhen(t.ArgumentIsBoolConstant("httpOnly", true)));
        }
    }
}
