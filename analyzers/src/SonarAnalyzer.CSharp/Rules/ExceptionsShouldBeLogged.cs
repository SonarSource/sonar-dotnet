/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using SonarAnalyzer.CSharp.Walkers;

namespace SonarAnalyzer.CSharp.Rules;

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
                            var walker = new CatchLoggingInvocationWalker(c.Model);
                            if (walker.SafeVisit(catchClauseSyntax) && !walker.IsExceptionLogged &&
                                walker.LoggingInvocationsWithoutException.Any())
                            {
                                var primaryLocation = walker.LoggingInvocationsWithoutException[0].GetLocation();
                                var secondaryLocations = walker.LoggingInvocationsWithoutException.Skip(1).ToSecondaryLocations(MessageFormat);
                                c.ReportIssue(Rule, primaryLocation, secondaryLocations);
                            }
                        },
                        SyntaxKind.CatchClause);
                }
            });
}
