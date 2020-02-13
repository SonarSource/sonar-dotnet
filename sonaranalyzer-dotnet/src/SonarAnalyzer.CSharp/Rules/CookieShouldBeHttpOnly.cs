/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SyntaxTrackers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CookieShouldBeHttpOnly : ObjectShouldBeInitializedCorrectlyBase
    {
        internal const string DiagnosticId = "S3330";
        private const string MessageFormat = "Make sure creating this cookie without the \"HttpOnly\" flag is safe.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager)
                                       .WithNotConfigurable();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private ObjectCreationTracker<SyntaxKind> ObjectCreationTracker { get; set; }

        internal override ImmutableArray<KnownType> TrackedTypes { get; } =
            ImmutableArray.Create(
                KnownType.System_Web_HttpCookie,
                KnownType.Microsoft_AspNetCore_Http_CookieOptions
            );

        protected override CSharpObjectInitializationTracker objectInitializationTracker { get; } = new CSharpObjectInitializationTracker(
            isAllowedConstantValue: constantValue => constantValue is bool value && value
        );

        protected override bool IsTrackedPropertyName(string propertyName) => "HttpOnly" == propertyName;

        public CookieShouldBeHttpOnly()
            : this(AnalyzerConfiguration.Hotspot)
        {
        }

        internal CookieShouldBeHttpOnly(IAnalyzerConfiguration analyzerConfiguration)
            : base(analyzerConfiguration)
        {
            ObjectCreationTracker = new CSharpObjectCreationTracker(analyzerConfiguration, rule);
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            base.Initialize(context);

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(KnownType.Nancy_Cookies_NancyCookie),
                Conditions.ExceptWhen(ObjectCreationTracker.ArgumentIsBoolConstant("httpOnly", true)));
        }
    }
}
