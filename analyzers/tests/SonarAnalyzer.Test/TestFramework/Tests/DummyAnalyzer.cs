﻿/*
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

using SonarAnalyzer.AnalysisContext;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.Test.TestFramework.Tests
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DummyAnalyzerCS : DummyAnalyzer<CS.SyntaxKind>
    {
        protected override CS.SyntaxKind NumericLiteralExpression => CS.SyntaxKind.NumericLiteralExpression;
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => CSharpGeneratedCodeRecognizer.Instance;
    }

    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    internal class DummyAnalyzerVB : DummyAnalyzer<VB.SyntaxKind>
    {
        protected override VB.SyntaxKind NumericLiteralExpression => VB.SyntaxKind.NumericLiteralExpression;
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => VisualBasicGeneratedCodeRecognizer.Instance;
    }

    internal abstract class DummyAnalyzer<TSyntaxKind> : TestAnalyzer where TSyntaxKind : struct
    {
        protected abstract TSyntaxKind NumericLiteralExpression { get; }

        public int DummyProperty { get; set; }

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(GeneratedCodeRecognizer, c => c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation())), NumericLiteralExpression);
    }
}