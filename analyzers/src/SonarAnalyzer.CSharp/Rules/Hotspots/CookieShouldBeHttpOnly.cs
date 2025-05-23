﻿/*
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

using SonarAnalyzer.Helpers.Trackers;

namespace SonarAnalyzer.Rules.CSharp
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
