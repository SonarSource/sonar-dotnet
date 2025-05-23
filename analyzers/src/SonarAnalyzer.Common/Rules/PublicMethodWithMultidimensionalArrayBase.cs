﻿/*
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

public abstract class PublicMethodWithMultidimensionalArrayBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S2368";
    protected override string MessageFormat => "Make this {0} private or simplify its parameters to not use multidimensional/jagged arrays.";

    protected abstract ImmutableArray<TSyntaxKind> SyntaxKindsOfInterest { get; }
    protected abstract Location GetIssueLocation(SyntaxNode node);
    protected abstract string GetType(SyntaxNode node);

    protected PublicMethodWithMultidimensionalArrayBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
            c =>
            {
                if (MethodSymbolOfNode(c.SemanticModel, c.Node) is { } methodSymbol
                    && methodSymbol.GetInterfaceMember() == null
                    && !methodSymbol.IsOverride
                    && methodSymbol.IsPubliclyAccessible()
                    && MethodHasMultidimensionalArrayParameters(methodSymbol))
                {
                    c.ReportIssue(SupportedDiagnostics[0], GetIssueLocation(c.Node), GetType(c.Node));
                }
            },
            SyntaxKindsOfInterest.ToArray());

    protected virtual IMethodSymbol MethodSymbolOfNode(SemanticModel semanticModel, SyntaxNode node) =>
        semanticModel.GetDeclaredSymbol(node) as IMethodSymbol;

    private static bool MethodHasMultidimensionalArrayParameters(IMethodSymbol methodSymbol) =>
        methodSymbol.IsExtensionMethod
            ? methodSymbol.Parameters.Skip(1).Any(IsMultidimensionalArrayParameter)
            : methodSymbol.Parameters.Any(IsMultidimensionalArrayParameter); // Perf: Make sure the Any method of ImmutableArray is called when possible. Don't do `Skip(m.IsExtensionMethod ? 0 : 1)`

    private static bool IsMultidimensionalArrayParameter(IParameterSymbol param) =>
        param.Type is IArrayTypeSymbol arrayType
        && (arrayType.Rank > 1
            || IsJaggedArrayParam(param, arrayType));

    private static bool IsJaggedArrayParam(IParameterSymbol param, IArrayTypeSymbol arrayType) =>
        param.IsParams
            ? arrayType.ElementType is IArrayTypeSymbol { ElementType: IArrayTypeSymbol }
            : arrayType.ElementType is IArrayTypeSymbol;
}
