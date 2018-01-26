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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class TestMethodShouldNotBeIgnored : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1607";
        private const string MessageFormat = "Either remove this 'Ignore' attribute or add an explanation about why " +
            "this test is ignored.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> TrackedTestMethodAttributes = ImmutableHashSet.Create(
            KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestMethodAttribute,
            KnownType.NUnit_Framework_TestAttribute,
            KnownType.NUnit_Framework_TestCaseAttribute,
            KnownType.NUnit_Framework_TestCaseSourceAttribute,
            KnownType.Xunit_FactAttribute,
            KnownType.Xunit_TheoryAttribute);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (!c.IsTest())
                    {
                        return;
                    }

                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(c.Node);
                    if (methodSymbol == null)
                    {
                        return;
                    }

                    var attributes = methodSymbol.GetAttributes();
                    if (!attributes.Any(a => a.AttributeClass.IsAny(TrackedTestMethodAttributes)))
                    {
                        return;
                    }

                    var ignoreAttribute = attributes.FirstOrDefault(a =>
                        a.AttributeClass.Is(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_IgnoreAttribute));
                    if (ignoreAttribute == null)
                    {
                        return;
                    }

                    var ignoreAttributeSyntax = ignoreAttribute.ApplicationSyntaxReference.GetSyntax();

                    var hasWorkItemAttribute = attributes.Any(a =>
                        a.AttributeClass.Is(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_WorkItemAttribute));
                    var hasTrailingComment = ignoreAttributeSyntax.Parent.GetTrailingTrivia()
                        .Any(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia));

                    if (!hasWorkItemAttribute &&
                        !hasTrailingComment)
                    {
                        Diagnostic.Create(rule, ignoreAttributeSyntax.GetLocation()).ReportFor(c);
                    }
                }, SyntaxKind.MethodDeclaration);
        }
    }
}
