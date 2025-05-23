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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AbstractTypesShouldNotHaveConstructors : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3442";
        private const string MessageFormat = "Change the visibility of this constructor to '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    if (c.Node.Parent.GetModifiers().Any(SyntaxKind.AbstractKeyword))
                    {
                        var invalidAccessModifier = c.Node.GetModifiers().FirstOrDefault(IsPublicOrInternal);
                        if (invalidAccessModifier != default)
                        {
                            c.ReportIssue(Rule, invalidAccessModifier, SuggestModifier(invalidAccessModifier));
                        }
                    }
                },
                SyntaxKind.ConstructorDeclaration);

        private static bool IsPublicOrInternal(SyntaxToken token) =>
            token.IsKind(SyntaxKind.PublicKeyword) || token.IsKind(SyntaxKind.InternalKeyword);

        private static string SuggestModifier(SyntaxToken token) =>
            token.IsKind(SyntaxKind.InternalKeyword) ? "private protected" : "protected";
    }
}
