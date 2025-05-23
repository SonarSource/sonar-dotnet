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
    public sealed class ExceptionsShouldBeUsed : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3984";
        private const string MessageFormat = "Throw this exception or remove this useless statement.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                var objectCreation = (ObjectCreationExpressionSyntax)c.Node;
                var parent = objectCreation.GetFirstNonParenthesizedParent();
                if (parent.IsKind(SyntaxKind.ExpressionStatement)
                    && c.SemanticModel.GetSymbolInfo(objectCreation.Type).Symbol is INamedTypeSymbol createdObjectType
                    && createdObjectType.DerivesFrom(KnownType.System_Exception))
                {
                    c.ReportIssue(Rule, objectCreation);
                }
            },
            SyntaxKind.ObjectCreationExpression);
    }
}
