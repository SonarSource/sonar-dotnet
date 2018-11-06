/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ExecutingXPathBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4817";
        protected const string MessageFormat = "Make sure that executing this XPATH expression is safe.";

        protected abstract DiagnosticDescriptor Rule { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        protected abstract InvocationTracker<TSyntaxKind> Tracker { get; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            Tracker.Track(context,

                Tracker.MatchSimpleNames(
                    new MethodSignature(KnownType.System_Xml_XmlNode, "SelectNodes"),
                    new MethodSignature(KnownType.System_Xml_XmlNode, "SelectSingleNode"),
                    new MethodSignature(KnownType.System_Xml_XPath_XPathExpression, "Compile"),
                    new MethodSignature(KnownType.System_Xml_XPath_XPathNavigator, "Compile"),
                    new MethodSignature(KnownType.System_Xml_XPath_XPathNavigator, "Evaluate"),
                    new MethodSignature(KnownType.System_Xml_XPath_XPathNavigator, "Matches"),
                    new MethodSignature(KnownType.System_Xml_XPath_XPathNavigator, "Select"),
                    new MethodSignature(KnownType.System_Xml_XPath_XPathNavigator, "SelectSingleNode")),

                Tracker.FirstParameterIsStringAndIsNotConstant

            );
        }
    }
}
