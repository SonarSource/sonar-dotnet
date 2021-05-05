/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class AsyncVoidMethod : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3168";
        private const string MessageFormat = "Return 'Task' instead.";
        private const string MsTestV1AssemblyName = "Microsoft.VisualStudio.QualityTools.UnitTestFramework";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<KnownType> AllowedAsyncVoidMsTestAttributes =
            ImmutableArray.Create(
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyCleanupAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyInitializeAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_ClassCleanupAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_ClassInitializeAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestCleanupAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestInitializeAttribute);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration);

                    if (IsViolatingRule(methodSymbol) && !IsExceptionToTheRule(methodDeclaration, methodSymbol))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, methodDeclaration.ReturnType.GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration);

        private static bool IsViolatingRule(IMethodSymbol methodSymbol) =>
            methodSymbol is {IsAsync: true, ReturnsVoid: true}
            && methodSymbol.IsChangeable();

        private static bool IsExceptionToTheRule(MethodDeclarationSyntax methodDeclaration, IMethodSymbol methodSymbol) =>
            methodSymbol.IsEventHandler()
            || IsUsedAsEventHandler(methodDeclaration)
            || IsNamedAsEventHandler(methodSymbol)
            || HasAnyMsTestV1AllowedAttribute(methodSymbol);

        private static bool IsUsedAsEventHandler(MethodDeclarationSyntax methodDeclaration) =>
            methodDeclaration.FirstAncestorOrSelf((SyntaxNode x) => x.IsKind(SyntaxKind.ClassDeclaration) || x.IsKind(SyntaxKindEx.RecordDeclaration)) is { } parentDeclaration
            && parentDeclaration.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(x => x.IsKind(SyntaxKind.AddAssignmentExpression))
                .Select(x => x.Right)
                .OfType<IdentifierNameSyntax>()
                .Any(x => x.Identifier.ValueText == methodDeclaration.Identifier.ValueText);

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
