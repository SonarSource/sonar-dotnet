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

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp;

public sealed class ObjectCreatedDropped : CSharpOnlyAnalyzer
{
    private const string DiagnosticId = "S1848";

    protected override string MessageFormat => "Either remove this useless object instantiation of class '{0}' or use it.";

    public ObjectCreatedDropped() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterSyntaxNodeActionInNonGenerated(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (Language.Syntax.IsKind(c.Node.Parent, Language.SyntaxKind.ExpressionStatement))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation(), Language.Syntax.Name(c.Node)));
                }
            },
            Language.SyntaxKind.ObjectCreationExpressions);
}
