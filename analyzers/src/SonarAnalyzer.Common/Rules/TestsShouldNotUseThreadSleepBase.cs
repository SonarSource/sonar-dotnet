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

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules;

public abstract class TestsShouldNotUseThreadSleepBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
     where TSyntaxKind : struct
{
    private const string DiagnosticId = "S2925";

    protected override string MessageFormat => "Do not use Thread.Sleep() in a test.";

    protected abstract bool IsWithinTest(SyntaxNode node, SemanticModel model);

    protected TestsShouldNotUseThreadSleepBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, c =>
        {
            if (IsThreadSleepMethod(Language.Syntax.NodeIdentifier(c.Node)?.Text)
                && c.SemanticModel.GetSymbolInfo(c.Node).Symbol is IMethodSymbol method
                && method.ContainingType.Is(KnownType.System_Threading_Thread)
                && IsWithinTest(c.Node, c.SemanticModel))
            {
                c.ReportIssue(Diagnostic.Create(Rule, c.Node.Parent.Parent.GetLocation()));
            }
        },
        Language.SyntaxKind.IdentifierName);

    private bool IsThreadSleepMethod(string name) =>
        nameof(Thread.Sleep).Equals(name, Language.NameComparison);
}
