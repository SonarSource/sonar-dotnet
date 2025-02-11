/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
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
public sealed class UnsafeCodeBlocks : HotspotDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6640";
    private const string MessageFormat = """Make sure that using "unsafe" is safe here.""";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public UnsafeCodeBlocks() : this(AnalyzerConfiguration.Hotspot) { }

    public UnsafeCodeBlocks(IAnalyzerConfiguration configuration) : base(configuration) { }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c => Report(c, ((UnsafeStatementSyntax)c.Node).UnsafeKeyword),
            SyntaxKind.UnsafeStatement);
        context.RegisterNodeAction(
            c => ReportIfUnsafe(c, ((BaseTypeDeclarationSyntax)c.Node).Modifiers),
            SyntaxKind.ClassDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.StructDeclaration, SyntaxKindEx.RecordDeclaration, SyntaxKindEx.RecordStructDeclaration);
        context.RegisterNodeAction(
            c => ReportIfUnsafe(c, ((BaseMethodDeclarationSyntax)c.Node).Modifiers),
            SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration, SyntaxKind.DestructorDeclaration, SyntaxKind.OperatorDeclaration);
        context.RegisterNodeAction(
            c => ReportIfUnsafe(c, ((LocalFunctionStatementSyntaxWrapper)c.Node).Modifiers),
            SyntaxKindEx.LocalFunctionStatement);
        context.RegisterNodeAction(
            c => ReportIfUnsafe(c, ((BaseFieldDeclarationSyntax)c.Node).Modifiers),
            SyntaxKind.FieldDeclaration, SyntaxKind.EventFieldDeclaration);
        context.RegisterNodeAction(
            c => ReportIfUnsafe(c, ((BasePropertyDeclarationSyntax)c.Node).Modifiers),
            SyntaxKind.PropertyDeclaration, SyntaxKind.IndexerDeclaration);
        context.RegisterNodeAction(
            c => ReportIfUnsafe(c, ((DelegateDeclarationSyntax)c.Node).Modifiers),
            SyntaxKind.DelegateDeclaration);
    }

    private void ReportIfUnsafe(SonarSyntaxNodeReportingContext context, SyntaxTokenList modifiers)
    {
        if (modifiers.Find(SyntaxKind.UnsafeKeyword) is { } unsafeModifier)
        {
            Report(context, unsafeModifier);
        }
    }

    private void Report(SonarSyntaxNodeReportingContext context, SyntaxToken token)
    {
        if (IsEnabled(context.Options))
        {
            context.ReportIssue(Rule, token);
        }
    }
}
