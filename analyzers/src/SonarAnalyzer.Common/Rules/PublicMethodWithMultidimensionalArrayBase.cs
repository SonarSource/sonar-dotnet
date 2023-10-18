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

namespace SonarAnalyzer.Rules;

public abstract class PublicMethodWithMultidimensionalArrayBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S2368";
    protected override string MessageFormat => "Make this {0} private or simplify its parameters to not use multidimensional/jagged arrays.";

    protected abstract Location GetIssueLocation(SyntaxNode node);
    protected abstract string GetType(SyntaxNode node);

    protected PublicMethodWithMultidimensionalArrayBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, AnalyzeDeclaration, Language.SyntaxKind.MethodDeclarations);
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, AnalyzeDeclaration, Language.SyntaxKind.ConstructorDeclaration);
    }

    private void AnalyzeDeclaration(SonarSyntaxNodeReportingContext c)
    {
        if (c.SemanticModel.GetDeclaredSymbol(c.Node) is IMethodSymbol methodSymbol
            && methodSymbol.GetInterfaceMember() == null
            && methodSymbol.GetOverriddenMember() == null
            && methodSymbol.IsPubliclyAccessible()
            && MethodHasMultidimensionalArrayParameters(methodSymbol))
        {
            c.ReportIssue(Diagnostic.Create(SupportedDiagnostics[0], GetIssueLocation(c.Node), GetType(c.Node)));
        }
    }

    private static bool MethodHasMultidimensionalArrayParameters(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.IsExtensionMethod)
        {
            for (var i = 1; i < methodSymbol.Parameters.Length; i++)
            {
                if (IsMultidimensionalArrayParameter(methodSymbol.Parameters[i]))
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            return methodSymbol.Parameters.Any(param => IsMultidimensionalArrayParameter(param));
        }
    }

    private static bool IsMultidimensionalArrayParameter(IParameterSymbol param) =>
        param.Type is IArrayTypeSymbol arrayType
        && (arrayType.Rank > 1
            || IsJaggedArrayParam(param, arrayType));

    private static bool IsJaggedArrayParam(IParameterSymbol param, IArrayTypeSymbol arrayType) =>
        param.IsParams
            ? arrayType.ElementType is IArrayTypeSymbol subType && subType.ElementType is IArrayTypeSymbol
            : arrayType.ElementType is IArrayTypeSymbol;
}
