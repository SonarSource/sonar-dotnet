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
public sealed class SeparateDeclarations : StylingAnalyzer
{
    public SeparateDeclarations() : base("T0016", "Add an empty line before this declaration.") { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            ValidateSeparatedMember,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.StructDeclaration);
        context.RegisterNodeAction(
            ValidatePossibleSingleLineMember,
            SyntaxKind.EventDeclaration,
            SyntaxKind.EventFieldDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKind.FieldDeclaration,
            SyntaxKind.IndexerDeclaration,
            SyntaxKind.PropertyDeclaration);
    }

    private void ValidatePossibleSingleLineMember(SonarSyntaxNodeReportingContext context)
    {
        var firstToken = context.Node.GetFirstToken();
        if (firstToken.Line() != context.Node.GetLastToken().Line()
            || IsStandaloneCloseBrace(firstToken.GetPreviousToken())
            || PreviousDeclarationKind() != context.Node.Kind())
        {
            ValidateSeparatedMember(context);
        }

        SyntaxKind PreviousDeclarationKind() =>
            context.Node.Parent.ChildNodes().TakeWhile(x => x != context.Node).LastOrDefault() is { } preceding
                ? preceding.Kind()
                : SyntaxKind.None;

        static bool IsStandaloneCloseBrace(SyntaxToken token) =>
            token.IsKind(SyntaxKind.CloseBraceToken) && token.Line() != token.GetPreviousToken().Line();
    }

    private void ValidateSeparatedMember(SonarSyntaxNodeReportingContext context)
    {
        var firstToken = context.Node.GetFirstToken();
        if (!context.Node.GetModifiers().Any(SyntaxKind.AbstractKeyword)
            && !firstToken.GetPreviousToken().IsKind(SyntaxKind.OpenBraceToken)
            && !ContainsEmptyLine(firstToken.LeadingTrivia))
        {
            var firstComment = firstToken.LeadingTrivia.FirstOrDefault(IsComment);
            context.ReportIssue(Rule, firstComment == default ? firstToken.GetLocation() : firstComment.GetLocation());
        }
    }

    private static bool IsComment(SyntaxTrivia trivia) =>
        trivia.IsAnyKind(
            SyntaxKind.SingleLineCommentTrivia,
            SyntaxKind.MultiLineCommentTrivia,
            SyntaxKind.SingleLineDocumentationCommentTrivia,
            SyntaxKind.MultiLineDocumentationCommentTrivia);

    private static bool ContainsEmptyLine(SyntaxTriviaList trivia)
    {
        var previousLine = -1;
        foreach (var trivium in trivia.Where(x => !x.IsKind(SyntaxKind.WhitespaceTrivia)))
        {
            if (trivium.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (previousLine != trivium.GetLocation().StartLine())
                {
                    return true;
                }
            }
            else
            {
                previousLine = trivium.GetLocation().EndLine();
            }
        }
        return false;
    }
}
