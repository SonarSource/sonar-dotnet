/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
    public sealed class CastConcreteTypeToInterface : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3215";
        private const string MessageFormat = "Remove this cast and edit the interface to add the missing functionality.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var castExpression = (CastExpressionSyntax)c.Node;
                    CheckForIssue(c, castExpression.Expression, castExpression.Type);
                },
                SyntaxKind.CastExpression);

            context.RegisterNodeAction(
                c =>
                {
                    var castExpression = (BinaryExpressionSyntax)c.Node;
                    CheckForIssue(c, castExpression.Left, castExpression.Right);
                },
                SyntaxKind.AsExpression);
        }

        private static void CheckForIssue(SonarSyntaxNodeReportingContext context, SyntaxNode fromExpression, SyntaxNode toExpression)
        {
            var castedFrom = context.SemanticModel.GetTypeInfo(fromExpression).Type;
            var castedTo = context.SemanticModel.GetTypeInfo(toExpression).Type;
            if (castedFrom.Is(TypeKind.Interface)
                && castedFrom.DeclaringSyntaxReferences.Any()
                && castedTo.Is(TypeKind.Class)
                && !castedTo.Is(KnownType.System_Object))
            {
                context.ReportIssue(CreateDiagnostic(Rule, context.Node.GetLocation()));
            }
        }
    }
}
