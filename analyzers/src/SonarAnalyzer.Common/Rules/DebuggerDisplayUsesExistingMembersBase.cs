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

using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Rules;

public abstract class DebuggerDisplayUsesExistingMembersBase<TAttributeSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TAttributeSyntax : SyntaxNode
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S4545";

    private readonly Regex evaluatedExpressionRegex = new(@"\{(?<EvaluatedExpression>[^}]+)\}", RegexOptions.Multiline, RegexConstants.DefaultTimeout);

    protected abstract SyntaxNode AttributeFormatString(TAttributeSyntax attribute);
    protected abstract bool IsValidMemberName(string memberName);

    protected override string MessageFormat => "'{0}' doesn't exist in this context.";

    protected DebuggerDisplayUsesExistingMembersBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
            c =>
            {
                var attribute = (TAttributeSyntax)c.Node;
                if (Language.Syntax.IsKnownAttributeType(c.SemanticModel, attribute, KnownType.System_Diagnostics_DebuggerDisplayAttribute)
                    && AttributeFormatString(attribute) is { } formatString
                    && Language.Syntax.StringValue(formatString, c.SemanticModel) is { } formatStringText
                    && FirstInvalidMemberName(c, formatStringText, attribute) is { } firstInvalidMember)
                {
                    c.ReportIssue(CreateDiagnostic(Rule, formatString.GetLocation(), firstInvalidMember));
                }
            },
            Language.SyntaxKind.Attribute);

    private string FirstInvalidMemberName(SonarSyntaxNodeReportingContext context, string formatString, TAttributeSyntax attributeSyntax)
    {
        try
        {
            return (attributeSyntax.Parent?.Parent is { } targetSyntax
                && context.SemanticModel.GetDeclaredSymbol(targetSyntax) is { } targetSymbol
                && TypeContainingReferencedMembers(targetSymbol) is { } typeSymbol)
                    ? FirstInvalidMemberName(typeSymbol)
                    : null;
        }
        catch (RegexMatchTimeoutException)
        {
            return null;
        }

        string FirstInvalidMemberName(ITypeSymbol typeSymbol)
        {
            var allMembers = typeSymbol
                .GetSelfAndBaseTypes()
                .SelectMany(x => x.GetMembers())
                .Select(x => x.Name)
                .ToHashSet(Language.NameComparer);

            foreach (Match match in evaluatedExpressionRegex.Matches(formatString))
            {
                if (match.Groups["EvaluatedExpression"] is { Success: true, Value: var evaluatedExpression }
                    && ExtractValidMemberName(evaluatedExpression) is { } memberName
                    && !allMembers.Contains(memberName))
                {
                    return memberName;
                }
            }

            return null;
        }

        string ExtractValidMemberName(string evaluatedExpression)
        {
            var sanitizedExpression = evaluatedExpression.Split(',')[0].Trim();
            return IsValidMemberName(sanitizedExpression) ? sanitizedExpression : null;
        }

        static ITypeSymbol TypeContainingReferencedMembers(ISymbol symbol) =>
            symbol is ITypeSymbol typeSymbol ? typeSymbol : symbol.ContainingType;
    }
}
