/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.UnitTest.TestFramework.Tests
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DummyAnalyzerCS : DummyAnalyzer<CS.SyntaxKind>
    {
        protected override CS.SyntaxKind NumericLiteralExpression => CS.SyntaxKind.NumericLiteralExpression;
    }

    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    internal class DummyAnalyzerVB : DummyAnalyzer<VB.SyntaxKind>
    {
        protected override VB.SyntaxKind NumericLiteralExpression => VB.SyntaxKind.NumericLiteralExpression;
    }

    internal abstract class DummyAnalyzer<TSyntaxKind> : DiagnosticAnalyzer where TSyntaxKind : struct
    {
        private static readonly DiagnosticDescriptor Rule = new("SDummy", "Dummy title", "Dummy message", string.Empty, DiagnosticSeverity.Warning, true);

        protected abstract TSyntaxKind NumericLiteralExpression { get;}

        public int DummyProperty { get; set; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public sealed override void Initialize(AnalysisContext context) =>
            context.RegisterSyntaxNodeAction(c => c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation())), NumericLiteralExpression);
    }
}
