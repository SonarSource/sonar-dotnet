/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
    public sealed class DoNotDecreaseMemberVisibility : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4015";
        private const string MessageFormat = "Make this member non-private or the class sealed.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var methodDeclaration = c.Node as MethodDeclarationSyntax;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration);
                    var classType = methodSymbol?.ContainingType;

                    if (classType != null &&
                        !classType.IsSealed &&
                        methodSymbol.DeclaredAccessibility == Accessibility.Private &&
                        HasInheritedMatchingMethods(classType, methodSymbol))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, methodDeclaration.Identifier.GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration);
        }

        private bool HasInheritedMatchingMethods(INamedTypeSymbol classType, IMethodSymbol methodSymbol)
        {
            if (classType?.BaseType == null)
            {
                return false;
            }

            bool baseClassHasMatchingPublicMethod = classType.BaseType
                .GetMembers(methodSymbol.Name)
                .OfType<IMethodSymbol>()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public)
                .Any(m => HasSameParameters(m, methodSymbol));

            if (baseClassHasMatchingPublicMethod)
            {
                return true;
            }

            return HasInheritedMatchingMethods(classType.BaseType, methodSymbol);
        }

        private bool HasSameParameters(IMethodSymbol methodCandidate, IMethodSymbol methodSymbol)
        {
            var methodCandidateParams = methodCandidate.Parameters;
            var methodParams = methodSymbol.Parameters;

            if (methodCandidateParams.Length != methodParams.Length)
            {
                return false;
            }

            for (int i = 0; i < methodParams.Length; i++)
            {
                if (!Equals(methodCandidateParams[i].Type, methodParams[i].Type))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
