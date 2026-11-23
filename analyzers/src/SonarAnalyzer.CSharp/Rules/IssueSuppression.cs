/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IssueSuppression : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S1309";
    private const string MessageFormat = "Do not suppress issues.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ImmutableArray<KnownType> SuppressMessageAttributes = ImmutableArray.Create(
        KnownType.System_Diagnostics_CodeAnalysis_SuppressMessageAttribute,
        KnownType.System_Diagnostics_CodeAnalysis_UnconditionalSuppressMessageAttribute);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c =>
            {
                var attribute = (AttributeSyntax)c.Node;

                if (c.Model.GetSymbolInfo(attribute).Symbol is IMethodSymbol methodSymbol
                    && methodSymbol.ContainingType.IsAny(SuppressMessageAttributes)
                    && attribute.GetIdentifier() is { } identifier)
                {
                    c.ReportIssue(Rule, identifier);
                }
            },
            SyntaxKind.Attribute);

        context.RegisterNodeAction(
            c =>
            {
                var pragmaWarning = (PragmaWarningDirectiveTriviaSyntax)c.Node;
                if (pragmaWarning.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword) && !IsVisitedTwice(c, pragmaWarning))
                {
                    c.ReportIssue(Rule, pragmaWarning.CreateLocation(pragmaWarning.DisableOrRestoreKeyword));
                }
            },
            SyntaxKind.PragmaWarningDirectiveTrivia);
    }

    // Some trivia is visited twice: with the method and with the enclosing declaration.
    // - Primary constructor types (type declaration and constructor). We only want to report from the constructor visit when the pragma is in the parameter list or base arguments.
    // - The end of file token of top-level statements (global namespace and generated Main). We only want to report from the global namespace visit.
    private static bool IsVisitedTwice(SonarSyntaxNodeReportingContext context, PragmaWarningDirectiveTriviaSyntax pragmaWarning) =>
        context.ContainingSymbol.Kind == SymbolKind.Method
        && pragmaWarning.ParentTrivia.Token.Parent.AncestorsAndSelf()
            .First(x => x is ParameterListSyntax or MemberDeclarationSyntax or CompilationUnitSyntax || x.IsKind(SyntaxKindEx.PrimaryConstructorBaseType))
            is TypeDeclarationSyntax or CompilationUnitSyntax;
}
