/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
    public sealed class MarkWindowsFormsMainWithStaThread : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4210";
        private const string MessageFormat = "{0}";
        private const string AddStaThreadMessage = "Add the 'STAThread' attribute to this entry point.";
        private const string ChangeMtaThreadToStaThreadMessage = "Change the 'MTAThread' attribute of this entry point to 'STAThread'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration);

                    if (methodSymbol.IsMainMethod() &&
                        !methodSymbol.GetAttributes(KnownType.System_STAThreadAttribute).Any() &&
                        IsAssemblyReferencingWindowsForms(c.SemanticModel.Compilation))
                    {
                        string message = methodSymbol.GetAttributes(KnownType.System_MTAThreadAttribute).Any()
                            ? ChangeMtaThreadToStaThreadMessage
                            : AddStaThreadMessage;

                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, methodDeclaration.FindIdentifierLocation(), message));
                    }
                },
                SyntaxKind.MethodDeclaration);
        }

        private static bool IsAssemblyReferencingWindowsForms(Compilation compilation)
        {
            return compilation.ReferencedAssemblyNames.Any(r => r.IsStrongName && r.Name == "System.Windows.Forms");
        }
    }
}
