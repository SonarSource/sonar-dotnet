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

namespace SonarAnalyzer.Rules;

public abstract class ArrayPassedAsParamsBase<TSyntaxKind, TArgumentNode> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TArgumentNode : SyntaxNode
{
    private const string DiagnosticId = "S3878";

    protected abstract TSyntaxKind[] ExpressionKinds { get; }
    protected abstract TArgumentNode LastArgumentIfArrayCreation(SyntaxNode expression);
    protected abstract ITypeSymbol ArrayElementType(TArgumentNode argument, SemanticModel model);

    protected override string MessageFormat => "Remove this array creation and simply pass the elements.";

    protected ArrayPassedAsParamsBase() : base(DiagnosticId) {}

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            if (LastArgumentIfArrayCreation(c.Node) is { } lastArgument
                && c.Model.GetSymbolInfo(c.Node).Symbol is IMethodSymbol methodSymbol
                && Language.MethodParameterLookup(c.Node, methodSymbol) is { } parameterLookup
                && parameterLookup.TryGetSymbol(lastArgument, out var param)
                && param is { IsParams: true }
                && !IsArrayOfCandidateTypes(lastArgument, parameterLookup, param, c.Model)
                && !IsJaggedArrayParam(param))
            {
                c.ReportIssue(Rule, lastArgument.GetLocation());
            }
        }, ExpressionKinds);

    private static bool IsJaggedArrayParam(IParameterSymbol param) =>
        param.Type is IArrayTypeSymbol { ElementType: IArrayTypeSymbol };

    private bool IsArrayOfCandidateTypes(TArgumentNode lastArgument, IMethodParameterLookup parameterLookup, IParameterSymbol param, SemanticModel model)
    {
        return param.Type is IArrayTypeSymbol array
            && (array.ElementType.Is(KnownType.System_Array)
               || (array.ElementType.Is(KnownType.System_Object) && !ParamArgumentsAreReferenceTypeArrays(lastArgument, parameterLookup, model)));

        bool ParamArgumentsAreReferenceTypeArrays(TArgumentNode lastArgument, IMethodParameterLookup parameterLookup, SemanticModel model) =>
            ArrayElementType(lastArgument, model) is { IsReferenceType: true } elementType
            && !elementType.Is(KnownType.System_Object)
            && parameterLookup.TryGetSyntax(param, out var arguments)
            && arguments.Length is 1;
    }
}
