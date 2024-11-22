/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
