/*
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

using SonarAnalyzer.Common.Walkers;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExceptionsShouldBeLogged : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6667";
    private const string MessageFormat = "Logging in a catch clause should pass the caught exception as a parameter.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    private static readonly KnownAssembly[] SupportedLoggingFrameworks =
    [
        KnownAssembly.MicrosoftExtensionsLoggingAbstractions,
        KnownAssembly.CastleCore,
        KnownAssembly.CommonLoggingCore,
        KnownAssembly.Log4Net,
        KnownAssembly.NLog
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
            {
                if (cc.Compilation.ReferencesAny(SupportedLoggingFrameworks))
                {
                    cc.RegisterNodeAction(c =>
                        {
                            var catchClauseSyntax = (CatchClauseSyntax)c.Node;
                            var walker = new CatchLoggingInvocationWalker(c.SemanticModel);
                            if (walker.SafeVisit(catchClauseSyntax) && !walker.IsExceptionLogged &&
                                walker.LoggingInvocationsWithoutException.Any())
                            {
                                var primaryLocation = walker.LoggingInvocationsWithoutException[0].GetLocation();
                                var secondaryLocations = walker.LoggingInvocationsWithoutException.Skip(1).Select(x => x.ToSecondaryLocation());
                                c.ReportIssue(Rule, primaryLocation, secondaryLocations);
                            }
                        },
                        SyntaxKind.CatchClause);
                }
            });
}
