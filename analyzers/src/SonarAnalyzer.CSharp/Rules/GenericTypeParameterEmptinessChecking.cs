/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GenericTypeParameterEmptinessChecking : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2955";
        private const string MessageFormat = "Use a comparison to 'default({0})' instead or add a constraint to '{0}' so that it can't be a value type.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var equalsExpression = (BinaryExpressionSyntax)c.Node;

                    var leftIsNull = CSharpEquivalenceChecker.AreEquivalent(equalsExpression.Left, SyntaxConstants.NullLiteralExpression);
                    var rightIsNull = CSharpEquivalenceChecker.AreEquivalent(equalsExpression.Right, SyntaxConstants.NullLiteralExpression);

                    if (!(leftIsNull ^ rightIsNull))
                    {
                        return;
                    }

                    var expressionToTypeCheck = leftIsNull ? equalsExpression.Right : equalsExpression.Left;
                    if (c.Model.GetTypeInfo(expressionToTypeCheck).Type is ITypeParameterSymbol { HasReferenceTypeConstraint: false } typeInfo
                        && !typeInfo.ConstraintTypes.OfType<IErrorTypeSymbol>().Any()
                        && !typeInfo.ConstraintTypes.Any(typeSymbol => typeSymbol.IsReferenceType && typeSymbol.IsClass()))
                    {
                        var expressionToReportOn = leftIsNull ? equalsExpression.Left : equalsExpression.Right;

                        c.ReportIssue(Rule, expressionToReportOn, typeInfo.Name);
                    }
                },
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);
    }
}
