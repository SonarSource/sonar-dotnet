/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class LossOfFractionInDivision : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2184";
        private const string MessageFormat = "Cast one of the operands of this division to '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var division = (BinaryExpressionSyntax)c.Node;

                    if (c.Model.GetSymbolInfo(division).Symbol as IMethodSymbol is not { } symbol
                        || !symbol.ContainingType.IsAny(KnownType.IntegralNumbersIncludingNative))
                    {
                        return;
                    }

                    if (DivisionIsInAssignmentAndTypeIsNonIntegral(division, c.Model, out var divisionResultType)
                        || DivisionIsArgumentAndTypeIsNonIntegral(division, c.Model, out divisionResultType)
                        || DivisionIsInReturnAndTypeIsNonIntegral(division, c.Model, out divisionResultType))
                    {
                        c.ReportIssue(Rule, division, divisionResultType.ToMinimalDisplayString(c.Model, division.SpanStart));
                    }
                },
                SyntaxKind.DivideExpression);

        private static bool DivisionIsInReturnAndTypeIsNonIntegral(SyntaxNode division, SemanticModel semanticModel, out ITypeSymbol divisionResultType)
        {
            if (division.Parent is ReturnStatementSyntax
                || division.Parent is LambdaExpressionSyntax)
            {
                divisionResultType = (semanticModel.GetEnclosingSymbol(division.SpanStart) as IMethodSymbol)?.ReturnType;
                return divisionResultType.IsAny(KnownType.NonIntegralNumbers);
            }

            divisionResultType = null;
            return false;
        }

        private static bool DivisionIsArgumentAndTypeIsNonIntegral(SyntaxNode division, SemanticModel semanticModel, out ITypeSymbol divisionResultType)
        {
            if (division.Parent is not ArgumentSyntax argument)
            {
                divisionResultType = null;
                return false;
            }

            if (argument.Parent.Parent is not InvocationExpressionSyntax invocation)
            {
                divisionResultType = null;
                return false;
            }

            var lookup = new CSharpMethodParameterLookup(invocation, semanticModel);
            if (!lookup.TryGetSymbol(argument, out var parameter))
            {
                divisionResultType = null;
                return false;
            }

            divisionResultType = parameter.Type;
            return divisionResultType.IsAny(KnownType.NonIntegralNumbers);
        }

        private static bool DivisionIsInAssignmentAndTypeIsNonIntegral(SyntaxNode division, SemanticModel semanticModel, out ITypeSymbol divisionResultType)
        {
            if (division.Parent is AssignmentExpressionSyntax assignment)
            {
                divisionResultType = semanticModel.GetTypeInfo(assignment.Left).Type;
                return divisionResultType.IsAny(KnownType.NonIntegralNumbers);
            }
            if (division is { Parent: EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax variableDecl } } })
            {
                divisionResultType = semanticModel.GetTypeInfo(variableDecl.Type).Type;
                return divisionResultType.IsAny(KnownType.NonIntegralNumbers);
            }
            if (DivisionIsInTupleTypeIsNonIntegral(division, semanticModel, out divisionResultType))
            {
                return divisionResultType.IsAny(KnownType.NonIntegralNumbers);
            }

            divisionResultType = null;
            return false;
        }

        private static bool DivisionIsInTupleTypeIsNonIntegral(SyntaxNode division, SemanticModel semanticModel, out ITypeSymbol divisionResultType)
        {
            var outerTuple = GetMostOuterTuple(division);
            if (outerTuple is { Parent: AssignmentExpressionSyntax assignmentSyntax }
                && assignmentSyntax.MapAssignmentArguments() is { } assignmentMappings)
            {
                var divisionResult = assignmentMappings.FirstOrDefault(x => x.Right.Equals(division)).Left;
                if (divisionResult is { })
                {
                    divisionResultType = semanticModel.GetTypeInfo(divisionResult).Type;
                    return divisionResultType.IsAny(KnownType.NonIntegralNumbers);
                }
            }
            // var (a, b) = (1, 1 / 3)
            else if (outerTuple is { Parent: EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax variableDeclaration } } })
            {
                var tupleArguments = ((TupleExpressionSyntaxWrapper)outerTuple).AllArguments();
                var declarationType = semanticModel.GetTypeInfo(variableDeclaration.Type).Type;
                var flattenTupleTypes = AllTupleElements(declarationType);
                var divisionArgumentIndex = DivisionArgumentIndex(tupleArguments, division);
                if (divisionArgumentIndex != -1)
                {
                    divisionResultType = flattenTupleTypes[divisionArgumentIndex];
                    return divisionResultType.IsAny(KnownType.NonIntegralNumbers);
                }
            }
            divisionResultType = null;
            return false;

            static SyntaxNode GetMostOuterTuple(SyntaxNode node) =>
                node.Ancestors()
                    .TakeWhile(x => TupleExpressionSyntaxWrapper.IsInstance(x) || x.IsKind(SyntaxKind.Argument))
                    .LastOrDefault(x => TupleExpressionSyntaxWrapper.IsInstance(x));

            static int DivisionArgumentIndex(ImmutableArray<ArgumentSyntax> arguments, SyntaxNode division) =>
                arguments.IndexOf(x => x.Expression.Equals(division));

            static List<ITypeSymbol> AllTupleElements(ITypeSymbol typeSymbol)
            {
                List<ITypeSymbol> flattenTupleTypes = new();
                CollectTupleTypes(flattenTupleTypes, typeSymbol);
                return flattenTupleTypes;

                static void CollectTupleTypes(List<ITypeSymbol> symbolList, ITypeSymbol tupleTypeSymbol)
                {
                    if (tupleTypeSymbol.IsTupleType())
                    {
                        var elements = ((INamedTypeSymbol)tupleTypeSymbol).TupleElements();
                        foreach (var element in elements)
                        {
                            CollectTupleTypes(symbolList, element.Type);
                        }
                    }
                    else
                    {
                        symbolList.Add(tupleTypeSymbol.GetSymbolType());
                    }
                }
            }
        }
    }
}
