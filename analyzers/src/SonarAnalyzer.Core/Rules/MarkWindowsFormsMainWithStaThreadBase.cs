/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Core.Rules
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

            if (c.Model.GetDeclaredSymbol(methodDeclaration) is IMethodSymbol methodSymbol
                && methodSymbol.IsMainMethod()
                && !methodSymbol.IsAsync
                && !methodSymbol.HasAttribute(KnownType.System_STAThreadAttribute)
                && IsAssemblyReferencingWindowsForms(c.Model.Compilation)
                && c.Compilation.Options.OutputKind == OutputKind.WindowsApplication)
            {
                var message = methodSymbol.HasAttribute(KnownType.System_MTAThreadAttribute)
                    ? ChangeMtaThreadToStaThreadMessage
                    : AddStaThreadMessage;

                c.ReportIssue(Rule, GetLocation(methodDeclaration), message);
            }
        }

        private static bool IsAssemblyReferencingWindowsForms(Compilation compilation) =>
            compilation.ReferencedAssemblyNames.Any(r => r.IsStrongName && r.Name == "System.Windows.Forms");
    }
}
