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

namespace SonarAnalyzer.Rules;

public abstract class ArrayPassedAsParamsBase<TSyntaxKind, TArgumentNode> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TArgumentNode : SyntaxNode
{
    private const string DiagnosticId = "S3878";

    protected abstract TSyntaxKind[] ExpressionKinds { get; }
    protected abstract TArgumentNode LastArgumentIfArrayCreation(SyntaxNode expression);

    protected override string MessageFormat => "Remove this array creation and simply pass the elements.";

    protected ArrayPassedAsParamsBase() : base(DiagnosticId) {}

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            if (LastArgumentIfArrayCreation(c.Node) is { } lastArgument
                && ParameterSymbol(c.SemanticModel, c.Node, lastArgument) is { IsParams: true } param
                && !Exluded(param))
            {
                c.ReportIssue(Rule, lastArgument.GetLocation());
            }
        }, ExpressionKinds);

    private IParameterSymbol ParameterSymbol(SemanticModel model, SyntaxNode invocation, TArgumentNode argument) =>
        model.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
        && Language.MethodParameterLookup(invocation, methodSymbol).TryGetSymbol(argument, out var param)
            ? param
            : null;

    private static bool Exluded(IParameterSymbol param)
    {
        return IsJaggedArrayParam(param) || IsObjectOrArrayType(param);

        static bool IsJaggedArrayParam(IParameterSymbol param) =>
            param.Type is IArrayTypeSymbol { ElementType: IArrayTypeSymbol };

        static bool IsObjectOrArrayType(IParameterSymbol param) =>
            param.Type is IArrayTypeSymbol array && array.ElementType.IsAny(KnownType.System_Object, KnownType.System_Array);
    }
}
