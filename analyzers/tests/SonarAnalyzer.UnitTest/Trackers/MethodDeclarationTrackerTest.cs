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

using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.Trackers;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class MethodDeclarationTrackerTest
    {
        private const string TestInputCS = @"
public class Sample
{
    public void NoArgs() {}
}";

        [TestMethod]
        public void MatchMethodName()
        {
            var tracker = new CSharpMethodDeclarationTracker();
            var context = CreateContext(TestInputCS, AnalyzerLanguage.CSharp, "NoArgs");
            tracker.MatchMethodName("NoArgs")(context).Should().BeTrue();
            tracker.MatchMethodName("Something")(context).Should().BeFalse();
        }

        [TestMethod]
        public void Track_VerifyMethodIdentifierLocations()
        {
            const string code = @"
public class Sample
{
    public Sample() {}
//         ^^^^^^
    public void Method() {}
//              ^^^^^^
    ~Sample() {}
//   ^^^^^^
    public int Property
//             ^^^^^^^^
//             ^^^^^^^^ @-1
    {
        get => 42;
        set { }
    }
    public int InitProperty
//             ^^^^^^^^^^^^
//             ^^^^^^^^^^^^ @-1
    {
        get => 42;
        init { }
    }
    public event System.EventHandler Event
//                                   ^^^^^
//                                   ^^^^^ @-1
    {
        add {}
        remove {}
    }
    public static int operator +(Sample a, Sample b) => 42;
//                             ^
}";
            Verifier.VerifyCSharpAnalyzer(code, new TestRule(), ParseOptionsHelper.FromCSharp9);
        }

        private static MethodDeclarationContext CreateContext(string testInput, AnalyzerLanguage language, string methodName)
        {
            var testCode = new SnippetCompiler(testInput, false, language);
            var symbol = testCode.GetMethodSymbol("Sample." + methodName);
            return new MethodDeclarationContext(symbol, testCode.SemanticModel.Compilation);
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        private class TestRule : TrackerHotspotDiagnosticAnalyzer<SyntaxKind>
        {
            protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

            public TestRule() : base(AnalyzerConfiguration.AlwaysEnabled, "S100", "Message") { } // Any existing rule ID

            protected override void Initialize(TrackerInput input) =>
                Language.Tracker.MethodDeclaration.Track(input);
        }
    }
}
