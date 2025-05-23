﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
    public abstract class DeclareTypesInNamespacesBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S3903";

        protected abstract TSyntaxKind[] SyntaxKinds { get; }
        protected abstract bool IsInnerTypeOrWithinNamespace(SyntaxNode declaration, SemanticModel semanticModel);
        protected abstract SyntaxToken GetTypeIdentifier(SyntaxNode declaration);
        protected abstract bool IsException(SyntaxNode node);

        protected override string MessageFormat => "Move '{0}' into a named namespace.";

        protected DeclareTypesInNamespacesBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
          context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
              c =>
              {
                  var declaration = c.Node;
                  if (c.IsRedundantPositionalRecordContext() || IsInnerTypeOrWithinNamespace(declaration, c.SemanticModel) || IsException(c.Node))
                  {
                      return;
                  }

                  var identifier = GetTypeIdentifier(declaration);
                  c.ReportIssue(Rule, identifier.GetLocation(), identifier.ValueText);
              },
              SyntaxKinds);
    }
}
