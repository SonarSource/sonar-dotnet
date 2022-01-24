/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RoslynCfgComparer : RoslynCfgComparerBase
    {
        protected override void Initialize(SonarAnalysisContext context)
        {
            // Output is rendered to Solution/Tests/RoslynData project
            context.RegisterSyntaxNodeActionInNonGenerated(
                ProcessBaseMethod,
                SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration);
        }

        internal override string LanguageVersion(Compilation c) =>
            c.GetLanguageVersion().ToString();

        internal override string MethodName(SyntaxNodeAnalysisContext c) =>
            (c.Node as MethodDeclarationSyntax)?.Identifier.ValueText ?? c.Node.FirstAncestorOrSelf<TypeDeclarationSyntax>().Identifier.ValueText + ".ctor";
    }
}
