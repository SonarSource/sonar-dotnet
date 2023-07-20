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
    public abstract class InsecureTemporaryFilesCreationBase<TMemberAccessSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TMemberAccessSyntax : SyntaxNode
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S5445";
        private const string VulnerableApiName = "GetTempFileName";

        protected override string MessageFormat => "'Path.GetTempFileName()' is insecure. Use 'Path.GetRandomFileName()' instead.";

        protected InsecureTemporaryFilesCreationBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, Visit, Language.SyntaxKind.SimpleMemberAccessExpression);

        private void Visit(SonarSyntaxNodeReportingContext context)
        {
            var memberAccess = (TMemberAccessSyntax)context.Node;
            if (Language.Syntax.IsMemberAccessOnKnownType(memberAccess, VulnerableApiName, KnownType.System_IO_Path, context.SemanticModel))
            {
                context.ReportIssue(CreateDiagnostic(Rule, memberAccess.GetLocation()));
            }
        }
    }
}
