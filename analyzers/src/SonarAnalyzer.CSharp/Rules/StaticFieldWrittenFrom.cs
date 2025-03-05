/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
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
    public abstract class StaticFieldWrittenFrom : SonarDiagnosticAnalyzer
    {
        protected abstract DiagnosticDescriptor Rule { get; }
        protected override bool EnableConcurrentExecution => false;
        protected abstract bool IsValidCodeBlockContext(SyntaxNode node, ISymbol owningSymbol);
        protected abstract string GetDiagnosticMessageArgument(SyntaxNode node, ISymbol owningSymbol, IFieldSymbol field);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCodeBlockStartAction(cbc =>
                {
                    if (!IsValidCodeBlockContext(cbc.CodeBlock, cbc.OwningSymbol))
                    {
                        return;
                    }

                    var locationsForFields = new MultiValueDictionary<IFieldSymbol, Location>();

                    cbc.RegisterNodeAction(c =>
                        {
                            var assignment = (AssignmentExpressionSyntax)c.Node;

                            foreach (var target in assignment.AssignmentTargets())
                            {
                                if (GetStaticFieldSymbol(c.Model, target) is { } fieldSymbol)
                                {
                                    locationsForFields.Add(fieldSymbol, target.CreateLocation(to: assignment.OperatorToken));
                                }
                            }
                        },
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxKind.AddAssignmentExpression,
                        SyntaxKind.SubtractAssignmentExpression,
                        SyntaxKind.MultiplyAssignmentExpression,
                        SyntaxKind.DivideAssignmentExpression,
                        SyntaxKind.ModuloAssignmentExpression,
                        SyntaxKind.AndAssignmentExpression,
                        SyntaxKind.ExclusiveOrAssignmentExpression,
                        SyntaxKind.OrAssignmentExpression,
                        SyntaxKind.LeftShiftAssignmentExpression,
                        SyntaxKind.RightShiftAssignmentExpression,
                        SyntaxKindEx.CoalesceAssignmentExpression,
                        SyntaxKindEx.UnsignedRightShiftAssignmentExpression);

                    cbc.RegisterNodeAction(c =>
                        {
                            var unary = (PrefixUnaryExpressionSyntax)c.Node;
                            CollectLocationOfStaticField(c.Model, locationsForFields, unary.Operand);
                        },
                        SyntaxKind.PreDecrementExpression,
                        SyntaxKind.PreIncrementExpression);

                    cbc.RegisterNodeAction(c =>
                        {
                            var unary = (PostfixUnaryExpressionSyntax)c.Node;
                            CollectLocationOfStaticField(c.Model, locationsForFields, unary.Operand);
                        },
                        SyntaxKind.PostDecrementExpression,
                        SyntaxKind.PostIncrementExpression);

                    cbc.RegisterCodeBlockEndAction(c =>
                    {
                        foreach (var fieldWithLocations in locationsForFields)
                        {
                            var firstPosition = fieldWithLocations.Value.Select(x => x.SourceSpan.Start).Min();
                            var location = fieldWithLocations.Value.First(x => x.SourceSpan.Start == firstPosition);
                            var message = GetDiagnosticMessageArgument(c.CodeBlock, c.OwningSymbol, fieldWithLocations.Key);
                            var secondaryLocations = fieldWithLocations.Key.DeclaringSyntaxReferences.Select(x => x.GetSyntax().ToSecondaryLocation());
                            c.ReportIssue(Rule, location, secondaryLocations, message);
                        }
                    });
                });

        private static void CollectLocationOfStaticField(SemanticModel semanticModel, MultiValueDictionary<IFieldSymbol, Location> locationsForFields, ExpressionSyntax expression)
        {
            if (GetStaticFieldSymbol(semanticModel, expression) is { } fieldSymbol)
            {
                locationsForFields.Add(fieldSymbol, expression.GetLocation());
            }
        }

        private static IFieldSymbol GetStaticFieldSymbol(SemanticModel semanticModel, SyntaxNode node) =>
            semanticModel.GetSymbolInfo(node) is { Symbol: IFieldSymbol { IsStatic: true } fieldSymbol }
                ? fieldSymbol
                : null;
    }
}
