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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ControllingPermissionsBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4834";
        protected const string MessageFormat = "Make sure controlling this permission is safe here.";

        private readonly IAnalyzerConfiguration configuration;
        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected ControllingPermissionsBase(IAnalyzerConfiguration configuration, System.Resources.ResourceManager rspecResources)
        {
            this.configuration = configuration;
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources).WithNotConfigurable();
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            var input = new TrackerInput(context, configuration, rule);

            var oc = Language.Tracker.ObjectCreation;
            oc.Track(input, oc.MatchConstructor(KnownType.System_Security_Permissions_PrincipalPermission));

            oc.Track(input, oc.WhenDerivesOrImplementsAny(
                KnownType.System_Security_Principal_IIdentity,
                KnownType.System_Security_Principal_IPrincipal));

            var inv = Language.Tracker.Invocation;
            inv.Track(input, inv.MatchMethod(
                new MemberDescriptor(KnownType.System_Security_Principal_WindowsIdentity, "GetCurrent"),
                new MemberDescriptor(KnownType.System_IdentityModel_Tokens_SecurityTokenHandler, "ValidateToken"),
                new MemberDescriptor(KnownType.System_AppDomain, "SetPrincipalPolicy"),
                new MemberDescriptor(KnownType.System_AppDomain, "SetThreadPrincipal")));

            var pa = Language.Tracker.PropertyAccess;
            pa.Track(input, pa.MatchProperty(
                new MemberDescriptor(KnownType.System_Web_HttpContext, "User"),
                new MemberDescriptor(KnownType.System_Threading_Thread, "CurrentPrincipal")));

            var md = Language.Tracker.MethodDeclaration;
            md.Track(input,
                md.AnyParameterIsOfType(
                    KnownType.System_Security_Principal_IIdentity,
                    KnownType.System_Security_Principal_IPrincipal),
                md.IsOrdinaryMethod());

            md.Track(input, md.DecoratedWithAnyAttribute(KnownType.System_Security_Permissions_PrincipalPermissionAttribute));

            var bt = Language.Tracker.BaseType;
            bt.Track(input, bt.MatchSubclassesOf(
                KnownType.System_Security_Principal_IIdentity,
                KnownType.System_Security_Principal_IPrincipal));
        }
    }
}
