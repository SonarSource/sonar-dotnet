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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ThreadStaticNonStaticField : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3005";
        private const string MessageFormat = "Remove the 'ThreadStatic' attribute from this definition.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    if ((FieldDeclarationSyntax)c.Node is var fieldDeclaration
                        && !fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
                        && fieldDeclaration.AttributeLists.GetAttributes(KnownType.System_ThreadStaticAttribute, c.Model).FirstOrDefault() is { } threadStaticAttribute)
                    {
                        c.ReportIssue(Rule, threadStaticAttribute.Name);
                    }
                },
                SyntaxKind.FieldDeclaration);
    }
}
