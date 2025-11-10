/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AsyncVoidMethod : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3168";
        private const string MessageFormat = "Return 'Task' instead.";
        private const string MsTestV1AssemblyName = "Microsoft.VisualStudio.QualityTools.UnitTestFramework";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        private static readonly ImmutableArray<KnownType> AllowedAsyncVoidMsTestAttributes =
            ImmutableArray.Create(
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyCleanupAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyInitializeAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_ClassCleanupAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_ClassInitializeAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestCleanupAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestInitializeAttribute);

        private static readonly HashSet<SyntaxKind> ParentTypeSyntaxKinds =
            [
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKindEx.RecordDeclaration,
                SyntaxKindEx.RecordStructDeclaration,
                SyntaxKind.InterfaceDeclaration
            ];

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.Model.GetDeclaredSymbol(methodDeclaration);

                    if (IsViolatingRule(methodSymbol) && !IsExceptionToTheRule(methodDeclaration, methodSymbol))
                    {
                        c.ReportIssue(Rule, methodDeclaration.ReturnType);
                    }
                },
                SyntaxKind.MethodDeclaration);

        private static bool IsViolatingRule(IMethodSymbol methodSymbol) =>
            methodSymbol is {IsAsync: true, ReturnsVoid: true}
            && methodSymbol.IsChangeable();

        private static bool IsExceptionToTheRule(MethodDeclarationSyntax methodDeclaration, IMethodSymbol methodSymbol) =>
            methodSymbol.IsEventHandler()
            || IsAcceptedUsage(methodDeclaration)
            || IsNamedAsEventHandler(methodSymbol)
            || HasAnyMsTestV1AllowedAttribute(methodSymbol);

        private static bool IsAcceptedUsage(MethodDeclarationSyntax methodDeclaration) =>
            GetParentDeclaration(methodDeclaration) is { } parentDeclaration
            && parentDeclaration
               .DescendantNodes()
               .SelectMany(node => node switch
                                   {
                                       ObjectCreationExpressionSyntax objectCreation => GetIdentifierArguments(objectCreation),
                                       InvocationExpressionSyntax invocation => GetIdentifierArguments(invocation),
                                       AssignmentExpressionSyntax assignment => GetIdentifierRightHandSide(assignment),
                                       _ => Enumerable.Empty<IdentifierNameSyntax>()
                                   })
               .Any(x => x.Identifier.ValueText == methodDeclaration.Identifier.ValueText);

        private static IEnumerable<IdentifierNameSyntax> GetIdentifierArguments(ObjectCreationExpressionSyntax objectCreation) =>
            objectCreation.ArgumentList?.Arguments.Select(x => x.Expression).OfType<IdentifierNameSyntax>() ?? Enumerable.Empty<IdentifierNameSyntax>();

        private static IEnumerable<IdentifierNameSyntax> GetIdentifierArguments(InvocationExpressionSyntax invocation) =>
            invocation.ArgumentList.Arguments.Select(x => x.Expression).OfType<IdentifierNameSyntax>();

        private static IEnumerable<IdentifierNameSyntax> GetIdentifierRightHandSide(AssignmentExpressionSyntax assignment) =>
            assignment.IsKind(SyntaxKind.AddAssignmentExpression) && assignment.Right is IdentifierNameSyntax identifier
                ? new[] { identifier }
                : Enumerable.Empty<IdentifierNameSyntax>();

        private static SyntaxNode GetParentDeclaration(SyntaxNode syntaxNode) =>
            syntaxNode.FirstAncestorOrSelf<TypeDeclarationSyntax>(x => x.IsAnyKind(ParentTypeSyntaxKinds));

        private static bool IsNamedAsEventHandler(ISymbol symbol) =>
            symbol.Name.Length > 2
            && symbol.Name.StartsWith("On")
            && char.IsUpper(symbol.Name[2]);

        private static bool HasAnyMsTestV1AllowedAttribute(IMethodSymbol methodSymbol) =>
            methodSymbol.GetAttributes().Any(x =>
                x.AttributeClass.ContainingAssembly.Name == MsTestV1AssemblyName
                && x.AttributeClass.IsAny(AllowedAsyncVoidMsTestAttributes));
    }
}
