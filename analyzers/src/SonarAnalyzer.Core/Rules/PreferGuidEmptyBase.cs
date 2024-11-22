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
    public abstract class PreferGuidEmptyBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        internal const string DiagnosticId = "S4581";

        protected override string MessageFormat => "Use 'Guid.NewGuid()' or 'Guid.Empty' or add arguments to this GUID instantiation.";

        protected PreferGuidEmptyBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (IsInvalidCtor(c.Node, c.SemanticModel)
                        && c.SemanticModel.GetSymbolInfo(c.Node).Symbol is IMethodSymbol methodSymbol
                        && methodSymbol.ContainingType.Is(KnownType.System_Guid))
                    {
                        c.ReportIssue(Rule, c.Node);
                    }
                },
                Language.SyntaxKind.ObjectCreationExpressions);

            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (!IsInParameter(c.Node)
                        && c.SemanticModel.GetTypeInfo(c.Node).ConvertedType.Is(KnownType.System_Guid))
                    {
                        c.ReportIssue(Rule, c.Node);
                    }
                },
                Language.SyntaxKind.DefaultExpressions);
        }

        private bool IsInvalidCtor(SyntaxNode ctorNode, SemanticModel semanticModel)
        {
            var arguments = Language.Syntax.ArgumentExpressions(ctorNode).ToArray();
            return arguments.Length == 0 || CreatesEmptyGuid(arguments, semanticModel);
        }

        private bool IsInParameter(SyntaxNode defaultExpression) =>
            defaultExpression.Ancestors().Any(x => Language.Syntax.IsKind(x, Language.SyntaxKind.Parameter));

        private static bool CreatesEmptyGuid(SyntaxNode[] arguments, SemanticModel semanticModel) =>
            arguments.Length == 1
            && Guid.TryParse(semanticModel.GetConstantValue(arguments[0]).Value as string, out var guid)
            && guid == Guid.Empty;
        }
}
