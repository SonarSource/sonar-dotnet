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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class InsecureTemporaryFilesCreationBase<TMemberAccessSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer
        where TMemberAccessSyntax : SyntaxNode
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S5445";
        private const string VulnerableApiName = "GetTempFileName";
        private const string MessageFormat = "'Path.GetTempFileName()' is insecure. Use 'Path.GetRandomFileName()' instead.";

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        internal abstract bool IsMemberAccessOnKnownType(TMemberAccessSyntax memberAccess, string name, KnownType knownType, SemanticModel model);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected InsecureTemporaryFilesCreationBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, Visit, Language.SyntaxKind.SimpleMemberAccessExpression);

        private void Visit(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (TMemberAccessSyntax)context.Node;
            if (IsMemberAccessOnKnownType(memberAccess, VulnerableApiName, KnownType.System_IO_Path, context.SemanticModel))
            {
                context.ReportIssue(Diagnostic.Create(rule, memberAccess.GetLocation()));
            }
        }
    }
}
