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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CookieShouldBeSecure : ObjectShouldBeInitializedCorrectlyBase
    {
        internal const string DiagnosticId = "S2092";
        private const string MessageFormat = "Make sure creating this cookie without setting the 'Secure' property is safe here.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager)
                .WithNotConfigurable();

        private ObjectCreationTracker<SyntaxKind> ObjectCreationTracker { get; set; }

        public CookieShouldBeSecure()
            : this(AnalyzerConfiguration.Hotspot)
        {
        }

        internal CookieShouldBeSecure(IAnalyzerConfiguration analyzerConfiguration)
            : base(analyzerConfiguration)
        {
            ObjectCreationTracker = new CSharpObjectCreationTracker(analyzerConfiguration, rule);
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            base.Initialize(context);

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(KnownType.Nancy_Cookies_NancyCookie),
                Conditions.ExceptWhen(ObjectCreationTracker.ArgumentIsBoolConstant("secure", true)));
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        internal override ImmutableArray<KnownType> TrackedTypes { get; } =
            ImmutableArray.Create(
                KnownType.System_Web_HttpCookie,
                KnownType.Microsoft_AspNetCore_Http_CookieOptions
            );

        protected override bool IsTrackedPropertyName(string propertyName) => "Secure" == propertyName;

        protected override bool IsAllowedValue(object constantValue) =>
            constantValue is bool value && value;

        protected override bool IsAllowedValue(ISymbol symbol) => false;

    }
}
