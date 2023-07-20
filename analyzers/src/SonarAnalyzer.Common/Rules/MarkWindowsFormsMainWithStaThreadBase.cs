/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class MarkWindowsFormsMainWithStaThreadBase<TSyntaxKind, TMethodSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TMethodSyntax : SyntaxNode
    {
        private const string DiagnosticId = "S4210";
        private const string AddStaThreadMessage = "Add the 'STAThread' attribute to this entry point.";
        private const string ChangeMtaThreadToStaThreadMessage = "Change the 'MTAThread' attribute of this entry point to 'STAThread'.";

        protected abstract TSyntaxKind[] SyntaxKinds { get; }

        protected abstract Location GetLocation(TMethodSyntax method);

        protected override string MessageFormat => "{0}";

        protected MarkWindowsFormsMainWithStaThreadBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, Action, SyntaxKinds);

        private void Action(SonarSyntaxNodeReportingContext c)
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

                c.ReportIssue(CreateDiagnostic(Rule, GetLocation(methodDeclaration), message));
            }
        }

        private static bool IsAssemblyReferencingWindowsForms(Compilation compilation) =>
            compilation.ReferencedAssemblyNames.Any(r => r.IsStrongName && r.Name == "System.Windows.Forms");
    }
}
