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
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class MarkWindowsFormsMainWithStaThreadBase<TSyntaxKind, TMethodSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TMethodSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S4210";
        private const string MessageFormat = "{0}";
        private const string AddStaThreadMessage = "Add the 'STAThread' attribute to this entry point.";
        private const string ChangeMtaThreadToStaThreadMessage = "Change the 'MTAThread' attribute of this entry point to 'STAThread'.";

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract TSyntaxKind[] SyntaxKinds { get; }

        protected abstract Location GetLocation(TMethodSyntax method);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected MarkWindowsFormsMainWithStaThreadBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, Action, SyntaxKinds);

        protected void Action(SyntaxNodeAnalysisContext c)
        {
            var methodDeclaration = (TMethodSyntax)c.Node;

            if (c.SemanticModel.GetDeclaredSymbol(methodDeclaration) is IMethodSymbol methodSymbol
                && methodSymbol.IsMainMethod()
                && !methodSymbol.IsAsync
                && !methodSymbol.HasAttribute(KnownType.System_STAThreadAttribute)
                && IsAssemblyReferencingWindowsForms(c.SemanticModel.Compilation)
                && c.Compilation.Options.OutputKind == OutputKind.WindowsApplication)
            {
                var message = methodSymbol.HasAttribute(KnownType.System_MTAThreadAttribute)
                    ? ChangeMtaThreadToStaThreadMessage
                    : AddStaThreadMessage;

                c.ReportIssue(Diagnostic.Create(SupportedDiagnostics[0], GetLocation(methodDeclaration), message));
            }
        }

        private static bool IsAssemblyReferencingWindowsForms(Compilation compilation) =>
            compilation.ReferencedAssemblyNames.Any(r => r.IsStrongName && r.Name == "System.Windows.Forms");
    }
}
