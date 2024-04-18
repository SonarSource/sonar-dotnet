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

namespace SonarAnalyzer.Rules.CSharp.Styling;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseNullInsteadOfDefault : StylingAnalyzer
{
    public UseNullInsteadOfDefault() : base("T0012", "Use 'null' instead of 'default' for reference types.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                if (IsReferenceType(c.Node, c.SemanticModel))
                {
                    c.ReportIssue(Rule, c.Node);
                }
            },
            SyntaxKind.DefaultLiteralExpression, SyntaxKind.DefaultExpression);

    private static bool IsReferenceType(SyntaxNode expression, SemanticModel semanticModel)
    {
        var typeInfo = semanticModel.GetTypeInfo(expression);
        return typeInfo.Type is not IErrorTypeSymbol && ((typeInfo.Type?.IsReferenceType ?? false) || typeInfo.Type.Is(KnownType.System_Nullable_T));
    }
}
