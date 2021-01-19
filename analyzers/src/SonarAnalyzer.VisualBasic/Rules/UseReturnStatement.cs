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

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class UseReturnStatement : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S5944";
        private const string MessageFormat = "Use a 'Return' statement; assigning returned values to function names is obsolete.";
        private static readonly DiagnosticDescriptor rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var name = (IdentifierNameSyntax)c.Node;
                if (!Excluded(name.Parent) &&
                    name.FirstAncestorOrSelf<MethodBlockSyntax>() is MethodBlockSyntax methodBlock &&
                    methodBlock.BlockStatement.DeclarationKeyword.IsKind(SyntaxKind.FunctionKeyword))
                {
                    var statement = (MethodStatementSyntax)methodBlock.BlockStatement;
                    if (name.Identifier.ValueText.Equals(statement?.Identifier.ValueText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, name.GetLocation()));
                    }
                }
            },
            SyntaxKind.IdentifierName);

        private static bool Excluded(SyntaxNode node) =>
            node is InvocationExpressionSyntax 
            || node is MemberAccessExpressionSyntax 
            || node is NamedFieldInitializerSyntax;
    }
}
