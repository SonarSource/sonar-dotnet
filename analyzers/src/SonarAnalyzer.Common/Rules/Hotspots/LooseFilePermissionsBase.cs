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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class LooseFilePermissionsBase<TSyntaxKind> : HotspotDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S2612";
        protected const string Everyone = "Everyone";

        private const string MessageFormat = "Make sure this permission is safe.";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected readonly DiagnosticDescriptor Rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected abstract void VisitAssignments(SyntaxNodeAnalysisContext context);

        protected abstract void VisitInvocations(SyntaxNodeAnalysisContext context);

        protected LooseFilePermissionsBase(IAnalyzerConfiguration configuration) : base(configuration) =>
            Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources).WithNotConfigurable();

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(c =>
            {
                if (!IsEnabled(c.Options))
                {
                    return;
                }

                c.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, VisitInvocations, Language.SyntaxKind.InvocationExpression);

                c.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, VisitAssignments, Language.SyntaxKind.IdentifierName);
            });

        protected bool IsFileAccessPermissions(SyntaxNode syntaxNode, SemanticModel semanticModel) =>
            Language.Syntax.NodeIdentifier(syntaxNode) is { } identifier
            && LooseFilePermissionsConfig.WeakFileAccessPermissions.Contains(identifier.Text)
            && syntaxNode.IsKnownType(KnownType.Mono_Unix_FileAccessPermissions, semanticModel);
    }
}
