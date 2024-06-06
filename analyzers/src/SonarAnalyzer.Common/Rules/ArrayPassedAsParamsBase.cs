/*
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

using System.Xml.Linq;
using SonarAnalyzer.Json.Parsing;
using SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

namespace SonarAnalyzer.Rules;

public abstract class ArrayPassedAsParamsBase<TSyntaxKind, TArgumentNode> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TArgumentNode : SyntaxNode
{
    private const string DiagnosticId = "S3878";

    protected abstract TSyntaxKind[] ExpressionKinds { get; }
    protected abstract TArgumentNode LastArgumentIfArrayCreation(SyntaxNode expression);
    protected abstract ITypeSymbol ArrayElementType(SyntaxNode expression, SemanticModel model);

    protected override string MessageFormat => "Remove this array creation and simply pass the elements.";

    protected ArrayPassedAsParamsBase() : base(DiagnosticId) {}

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            if (LastArgumentIfArrayCreation(c.Node) is { } lastArgument
                && c.SemanticModel.GetSymbolInfo(c.Node).Symbol is IMethodSymbol methodSymbol
                && Language.MethodParameterLookup(c.Node, methodSymbol).TryGetSymbol(lastArgument, out var param)
                && param is { IsParams: true }
                && !IsObjectOrArrayType(c.Node, methodSymbol, param, c.SemanticModel)
                && !IsJaggedArrayParam(param))
            {
                c.ReportIssue(Rule, lastArgument.GetLocation());
            }
        }, ExpressionKinds);

    private static bool IsJaggedArrayParam(IParameterSymbol param) =>
        param.Type is IArrayTypeSymbol { ElementType: IArrayTypeSymbol };

    private bool IsObjectOrArrayType(SyntaxNode node, IMethodSymbol symbol, IParameterSymbol param, SemanticModel model)
    {
        return param.Type is IArrayTypeSymbol array
            && (array.ElementType.Is(KnownType.System_Array)
               || (array.ElementType.Is(KnownType.System_Object) && !ParamArgumentsAreReferenceTypeArrays(node, symbol, param, model)));

        bool ParamArgumentsAreReferenceTypeArrays(SyntaxNode node, IMethodSymbol symbol, IParameterSymbol param, SemanticModel model) =>
            Language.MethodParameterLookup(node, symbol) is { } parameterLookup
            && parameterLookup.TryGetSyntax(param, out var arguments)
            && arguments.Count() is 1
            && ArrayElementType(arguments.First(), model) is { } elementType
            && elementType.IsReferenceType
            && !elementType.Is(KnownType.System_Object);
    }
}
