/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class UsingCookiesBase<TSyntaxKind> : TrackerHotspotDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S2255";
        private const string MessageFormat = "Make sure that this cookie is written safely.";

        protected UsingCookiesBase(IAnalyzerConfiguration configuration, System.Resources.ResourceManager rspecResources) : base(configuration, DiagnosticId, MessageFormat, rspecResources) { }

        protected override void Initialize(TrackerInput input)
        {
            var pa = Language.Tracker.PropertyAccess;
            pa.Track(input,
                pa.MatchProperty(new MemberDescriptor(KnownType.System_Web_HttpCookie, "Value")),
                pa.MatchSetter());

            var oc = Language.Tracker.ObjectCreation;
            oc.Track(input,
                oc.MatchConstructor(KnownType.System_Web_HttpCookie),
                oc.ArgumentAtIndexIs(1, KnownType.System_String));

            var ea = Language.Tracker.ElementAccess;
            ea.Track(input,
                ea.MatchIndexerIn(KnownType.System_Web_HttpCookie),
                ea.ArgumentAtIndexIs(0, KnownType.System_String),
                ea.MatchSetter());

            ea.Track(input,
                ea.MatchIndexerIn(KnownType.Microsoft_AspNetCore_Http_IHeaderDictionary),
                ea.ArgumentAtIndexEquals(0, "Set-Cookie"),
                ea.MatchSetter());

            ea.Track(input,
                ea.MatchIndexerIn(
                    KnownType.Microsoft_AspNetCore_Http_IRequestCookieCollection,
                    KnownType.Microsoft_AspNetCore_Http_IResponseCookies),
                ea.MatchSetter());

            ea.Track(input,
                ea.MatchIndexerIn(KnownType.System_Collections_Specialized_NameValueCollection),
                ea.MatchSetter(),
                ea.MatchProperty(new MemberDescriptor(KnownType.System_Web_HttpCookie, "Values")));

            var inv = Language.Tracker.Invocation;
            inv.Track(input,
                inv.MatchMethod(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Http_IResponseCookies, "Append")));

            inv.Track(input,
                inv.MatchMethod(
                    new MemberDescriptor(KnownType.System_Collections_Generic_IDictionary_TKey_TValue, "Add"),
                    new MemberDescriptor(KnownType.System_Collections_Generic_IDictionary_TKey_TValue_VB, "Add")),
                inv.ArgumentAtIndexIsAny(0, "Set-Cookie"),
                inv.MethodHasParameters(2),
                IsIHeadersDictionary());

            inv.Track(input,
                inv.MatchMethod(new MemberDescriptor(KnownType.System_Collections_Specialized_NameObjectCollectionBase, "Add")),
                inv.MatchProperty(new MemberDescriptor(KnownType.System_Web_HttpCookie, "Values")));
        }

        private static TrackerBase<TSyntaxKind, InvocationContext>.Condition IsIHeadersDictionary() =>
            context =>
            {
                var containingType = context.MethodSymbol.Value.ContainingType;
                // We already checked if ContainingType is IDictionary, but be defensive and check TypeArguments.Count
                return containingType.TypeArguments.Length == 2
                    && containingType.TypeArguments[0].Is(KnownType.System_String)
                    && containingType.TypeArguments[1].Is(KnownType.Microsoft_Extensions_Primitives_StringValues);
            };
    }
}
