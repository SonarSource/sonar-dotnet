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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class ParametersCorrectOrder : ParametersCorrectOrderBase<ArgumentSyntax>
    {
        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var methodCall = (InvocationExpressionSyntax)c.Node;
                    AnalyzeArguments(c, methodCall.ArgumentList, GetLocation);

                    Location GetLocation() =>
                        methodCall.Expression is not MemberAccessExpressionSyntax memberAccess
                        ? methodCall.Expression.GetLocation()
                        : memberAccess.Name.GetLocation();
                }, SyntaxKind.InvocationExpression);

            context.RegisterNodeAction(
                c =>
                {
                    var objectCreationCall = (ObjectCreationExpressionSyntax)c.Node;
                    AnalyzeArguments(c, objectCreationCall.ArgumentList, GetLocation);

                    Location GetLocation() =>
                        objectCreationCall.Type is not QualifiedNameSyntax qualifiedAccess
                        ? objectCreationCall.Type.GetLocation()
                        : qualifiedAccess.Right.GetLocation();
                }, SyntaxKind.ObjectCreationExpression);
        }

        private void AnalyzeArguments(SonarSyntaxNodeReportingContext analysisContext, ArgumentListSyntax argumentList, Func<Location> getLocation)
        {
            if (argumentList == null)
            {
                return;
            }

            var methodParameterLookup = new VisualBasicMethodParameterLookup(argumentList, analysisContext.SemanticModel);
            ReportIncorrectlyOrderedParameters(analysisContext, methodParameterLookup, argumentList.Arguments, getLocation);
        }

        protected override TypeInfo GetArgumentTypeSymbolInfo(ArgumentSyntax argument, SemanticModel model) =>
            model.GetTypeInfo(argument.GetExpression());

        protected override Location GetMethodDeclarationIdentifierLocation(SyntaxNode syntaxNode) =>
            (syntaxNode as MethodBlockBaseSyntax)?.FindIdentifierLocation();

        protected override SyntaxToken? GetArgumentIdentifier(ArgumentSyntax argument, SemanticModel model) =>
            GetExpressionSyntaxIdentifier(argument?.GetExpression(), model);

        protected override SyntaxToken? GetNameColonArgumentIdentifier(ArgumentSyntax argument) =>
            (argument as SimpleArgumentSyntax)?.NameColonEquals?.Name.Identifier;

        private static SyntaxToken? GetExpressionSyntaxIdentifier(ExpressionSyntax expression, SemanticModel model) =>
            expression switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier,
                MemberAccessExpressionSyntax memberAccess => GetValueAccessIdentifier(memberAccess, model),
                CastExpressionSyntax cast => GetExpressionSyntaxIdentifier(cast.Expression, model),
                PredefinedCastExpressionSyntax predefinedCast => GetExpressionSyntaxIdentifier(predefinedCast.Expression, model),
                ParenthesizedExpressionSyntax parentheses => GetExpressionSyntaxIdentifier(parentheses.Expression, model),
                _ => null
            };

        private static SyntaxToken? GetValueAccessIdentifier(MemberAccessExpressionSyntax expression, SemanticModel model) =>
            expression.Name.ToString() == "Value" && model.GetTypeInfo(expression.Expression).ConvertedType.DerivesOrImplements(KnownType.System_Nullable_T)
                ? GetExpressionSyntaxIdentifier(expression.Expression, model)
                : expression.Name.Identifier;
    }
}
