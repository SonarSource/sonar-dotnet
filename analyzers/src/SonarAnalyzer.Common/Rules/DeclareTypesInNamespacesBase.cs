/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DeclareTypesInNamespacesBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S3903";

        protected abstract TSyntaxKind[] SyntaxKinds { get; }

        protected abstract bool IsInnerTypeOrWithinNamespace(SyntaxNode declaration, SemanticModel semanticModel);
        protected abstract SyntaxToken GetTypeIdentifier(SyntaxNode declaration);

        protected override string MessageFormat => "Move '{0}' into a named namespace.";

        protected DeclareTypesInNamespacesBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
          context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer,
              c =>
              {
                  var declaration = c.Node;

                  if (c.ContainingSymbol.Kind != SymbolKind.NamedType
                      || IsInnerTypeOrWithinNamespace(declaration, c.SemanticModel))
                  {
                      return;
                  }

                  var identifier = GetTypeIdentifier(declaration);
                  c.ReportIssue(Diagnostic.Create(Rule, identifier.GetLocation(), identifier.ValueText));
              },
              SyntaxKinds);
    }
}
