/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DisposableTypesNeedFinalizers : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4002";
        private const string MessageFormat = "Implement a finalizer that calls your 'Dispose' method.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private static readonly ImmutableArray<KnownType> NativeHandles = ImmutableArray.Create(
            KnownType.System_IntPtr,
            KnownType.System_UIntPtr,
            KnownType.System_Runtime_InteropServices_HandleRef);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                var declaration = (TypeDeclarationSyntax)c.Node;
                if (!c.IsRedundantPositionalRecordContext()
                    && ((ITypeSymbol)c.ContainingSymbol).Implements(KnownType.System_IDisposable)
                    && HasNativeHandleFields(declaration, c.SemanticModel)
                    && !HasFinalizer(declaration))
                {
                    c.ReportIssue(Rule, declaration.Identifier);
                }
            },
            SyntaxKind.ClassDeclaration,
            SyntaxKindEx.RecordDeclaration);

        private static bool HasNativeHandleFields(TypeDeclarationSyntax classDeclaration, SemanticModel semanticModel) =>
            classDeclaration.Members
                .OfType<FieldDeclarationSyntax>()
                .Select(m => semanticModel.GetDeclaredSymbol(m.Declaration.Variables.FirstOrDefault())?.GetSymbolType())
                .Any(si => si.IsAny(NativeHandles));

        private static bool HasFinalizer(TypeDeclarationSyntax classDeclaration) =>
            classDeclaration.Members.OfType<DestructorDeclarationSyntax>().Any();
    }
}
