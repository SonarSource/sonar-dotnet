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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PasswordsShouldBeStoredCorrectly : TrackerHotspotDiagnosticAnalyzer<SyntaxKind>
{
    private const string DiagnosticId = "S5344";
    private const string MessageFormat = "FIXME";

    private const int IterationCountThreshold = 100_000;

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    public PasswordsShouldBeStoredCorrectly()
       : base(AnalyzerConfiguration.AlwaysEnabled, DiagnosticId, MessageFormat) { }

    protected override void Initialize(TrackerInput input)
    {
        var tracker = Language.Tracker.Argument;

        var descriptor = ArgumentDescriptor.MethodInvocation(
            KnownType.Microsoft_AspNetCore_Cryptography_KeyDerivation_KeyDerivation,
            "Pbkdf2",
            "iterationCount",
            3);

        tracker.Track(
            input,
            tracker.MatchArgument(descriptor),
            HasFewIterations());
    }

    private static TrackerBase<SyntaxKind, ArgumentContext>.Condition HasFewIterations() =>
        x =>
            x.Node is ArgumentSyntax argument
            && x.SemanticModel.GetConstantValue(argument.Expression) is { HasValue: true, Value: int iterationCount }
            && iterationCount < IterationCountThreshold;
}
