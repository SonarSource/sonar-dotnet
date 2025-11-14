/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Styling.Rules;

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
        if (!context.Node.Parent.IsKind(SyntaxKind.InterfaceDeclaration)
            && !context.Node.GetModifiers().Any(SyntaxKind.AbstractKeyword)
            && !firstToken.GetPreviousToken().IsKind(SyntaxKind.OpenBraceToken)
            && !ContainsEmptyLine(firstToken.LeadingTrivia))
        {
            var firstComment = firstToken.LeadingTrivia.FirstOrDefault(x => x.IsComment());
            context.ReportIssue(Rule, firstComment == default ? firstToken.GetLocation() : firstComment.GetLocation());
        }
    }

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
