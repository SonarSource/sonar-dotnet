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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class MarkWindowsFormsMainWithStaThreadBase<TMethodSyntax> : SonarDiagnosticAnalyzer
        where TMethodSyntax : SyntaxNode
    {
        internal const string DiagnosticId = "S4210";
        protected const string MessageFormat = "{0}";
        protected const string AddStaThreadMessage = "Add the 'STAThread' attribute to this entry point.";
        protected const string ChangeMtaThreadToStaThreadMessage = "Change the 'MTAThread' attribute of this entry point to 'STAThread'.";

        protected abstract IMethodSymbol GetSymbol(SemanticModel model, TMethodSyntax method);

        protected abstract void Report(SyntaxNodeAnalysisContext c, TMethodSyntax method, string message);

        protected void Action(SyntaxNodeAnalysisContext c)
        {
            var methodDeclaration = (TMethodSyntax)c.Node;
            var methodSymbol = GetSymbol(c.SemanticModel, methodDeclaration);

            if (methodSymbol.IsMainMethod() &&
                !methodSymbol.GetAttributes(KnownType.System_STAThreadAttribute).Any() &&
                IsAssemblyReferencingWindowsForms(c.SemanticModel.Compilation))
            {
                string message = methodSymbol.GetAttributes(KnownType.System_MTAThreadAttribute).Any()
                    ? ChangeMtaThreadToStaThreadMessage
                    : AddStaThreadMessage;

                Report(c, methodDeclaration, message);
            }
        }

        protected static bool IsAssemblyReferencingWindowsForms(Compilation compilation)
        {
            return compilation.ReferencedAssemblyNames.Any(r => r.IsStrongName && r.Name == "System.Windows.Forms");
        }
    }
}
