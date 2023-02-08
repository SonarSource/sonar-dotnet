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

namespace SonarAnalyzer.Rules;

public abstract class DebuggerDisplayUsesExistingMembersBase<TAttributeSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TAttributeSyntax : SyntaxNode
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S4545";

    private readonly Regex nqModifierExpressionRegex = new(@",\s*nq\s*$", RegexOptions.None, RegexConstants.DefaultTimeout);
    private readonly Regex evaluatedExpressionRegex = new(@"\{(?<EvaluatedExpression>[^\}]+)\}", RegexOptions.None, RegexConstants.DefaultTimeout);

    protected abstract string GetAttributeName(TAttributeSyntax attribute);
    protected abstract SyntaxNode GetAttributeFormatString(TAttributeSyntax attribute);
    protected abstract bool IsValidMemberName(string memberName);

    protected override string MessageFormat => "'{0}' doesn't exist in this context.";

    protected DebuggerDisplayUsesExistingMembersBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
            c =>
            {
                var attribute = (TAttributeSyntax)c.Node;
                var attributeName = GetAttributeName(attribute);
                if ((string.Equals(attributeName, "DebuggerDisplayAttribute", Language.NameComparison) || string.Equals(attributeName, "DebuggerDisplay", Language.NameComparison))
                    && GetAttributeFormatString(attribute) is { } formatString
                    && FirstInvalidMemberName(c, formatString.GetFirstToken().ValueText, attribute) is { } firstInvalidMember)
                {
                    c.ReportIssue(Diagnostic.Create(Rule, formatString.GetLocation(), firstInvalidMember));
                }
            },
            Language.SyntaxKind.Attribute);

    private string FirstInvalidMemberName(SonarSyntaxNodeReportingContext context, string formatString, TAttributeSyntax attributeSyntax)
    {
        try
        {
            foreach (Match match in evaluatedExpressionRegex.Matches(formatString))
            {
                if (match.Groups["EvaluatedExpression"] is { Success: true, Value: var evaluatedExpression }
                    && ExtractValidMemberName(evaluatedExpression) is { } memberName
                    && attributeSyntax.Parent?.Parent is { } targetSyntax
                    && context.SemanticModel.GetDeclaredSymbol(targetSyntax) is { } targetSymbol
                    && RelevantType(targetSymbol) is { } typeSymbol
                    && typeSymbol.GetSelfAndBaseTypes().SelectMany(x => x.GetMembers()).All(x => Language.NameComparer.Compare(x.Name, memberName) != 0))
                {
                    return memberName;
                }
            }

            return null;
        }
        catch (RegexMatchTimeoutException)
        {
            return null;
        }
    }

    private string ExtractValidMemberName(string evaluatedExpression)
    {
        var sanitizedExpression = RemoveNqModifier(evaluatedExpression).Trim();
        return IsValidMemberName(sanitizedExpression) ? sanitizedExpression : null;
    }

    private string RemoveNqModifier(string evaluatedExpression) =>
        nqModifierExpressionRegex.Match(evaluatedExpression) is { Success: true, Length: var matchLength }
            ? evaluatedExpression.Substring(0, evaluatedExpression.Length - matchLength)
            : evaluatedExpression;

    private static ITypeSymbol RelevantType(ISymbol symbol) => symbol is ITypeSymbol typeSymbol ? typeSymbol : symbol.ContainingType;
}
