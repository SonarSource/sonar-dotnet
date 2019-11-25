/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CastConcreteTypeToInterface : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3215";
        private const string MessageFormat = "Remove this cast and edit the interface to add the missing functionality.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var castExpression = (CastExpressionSyntax)c.Node;
                    var castedTo = c.SemanticModel.GetTypeInfo(castExpression.Type).Type;
                    var castedFrom = c.SemanticModel.GetTypeInfo(castExpression.Expression).Type;
                    CheckForIssue(castedTo, castedFrom, c);
                },
                SyntaxKind.CastExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var castExpression = (BinaryExpressionSyntax)c.Node;
                    var castedTo = c.SemanticModel.GetTypeInfo(castExpression.Right).Type;
                    var castedFrom = c.SemanticModel.GetTypeInfo(castExpression.Left).Type;
                    CheckForIssue(castedTo, castedFrom, c);
                },
                SyntaxKind.AsExpression);
        }

        public static void CheckForIssue(ITypeSymbol castedTo, ITypeSymbol castedFrom,
            SyntaxNodeAnalysisContext context)
        {
            if (!castedFrom.Is(TypeKind.Interface) ||
                !castedFrom.DeclaringSyntaxReferences.Any() ||
                !castedTo.Is(TypeKind.Class) ||
                castedTo.Is(KnownType.System_Object))
            {
                return;
            }

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, context.Node.GetLocation()));
        }
    }
}
