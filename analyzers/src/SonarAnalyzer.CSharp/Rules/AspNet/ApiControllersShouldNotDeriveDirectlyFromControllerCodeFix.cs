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

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class ApiControllersShouldNotDeriveDirectlyFromControllerCodeFix : SonarCodeFix
    {
        internal const string Title = "Inherit from ControllerBase instead of Controller.";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ApiControllersShouldNotDeriveDirectlyFromController.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var toReplace = root.FindNode(context.Diagnostics.First().Location.SourceSpan) as BaseTypeSyntax;
            var replacement = root.ReplaceNode(toReplace, SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName("ControllerBase")))
                .WithAdditionalAnnotations(Formatter.Annotation);

            context.RegisterCodeFix(
               Title,
               token => Task.FromResult(context.Document.WithSyntaxRoot(replacement)),
               context.Diagnostics);
            return Task.CompletedTask;
        }
  }
}
