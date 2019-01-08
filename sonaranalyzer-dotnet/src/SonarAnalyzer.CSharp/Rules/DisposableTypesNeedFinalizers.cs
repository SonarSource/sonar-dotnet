/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DisposableTypesNeedFinalizers : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4002";
        private const string MessageFormat = "Implement a finalizer that calls your 'Dispose' method.";

        private static readonly DiagnosticDescriptor rule
            = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> nativeHandles =
            ImmutableArray.Create(
                KnownType.System_IntPtr,
                KnownType.System_UIntPtr,
                KnownType.System_Runtime_InteropServices_HandleRef
            );

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var classDeclaration = (ClassDeclarationSyntax)c.Node;
                var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);

                if (classSymbol.Implements(KnownType.System_IDisposable) &&
                    HasNativeHandleFields(classDeclaration, c.SemanticModel) &&
                    !HasFinalizer(classDeclaration))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, classDeclaration.Identifier.GetLocation()));
                }
            },
            SyntaxKind.ClassDeclaration);
        }

        private static bool HasNativeHandleFields(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            return classDeclaration.Members
                        .OfType<FieldDeclarationSyntax>()
                        .Select(m => semanticModel.GetDeclaredSymbol(m.Declaration.Variables.FirstOrDefault())?.GetSymbolType())
                        .Any(si => si.IsAny(nativeHandles));
        }

        private static bool HasFinalizer(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Members.OfType<DestructorDeclarationSyntax>().Any();
        }
    }
}
