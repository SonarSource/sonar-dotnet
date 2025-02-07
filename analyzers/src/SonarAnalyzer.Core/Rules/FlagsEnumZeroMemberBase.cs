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
    public abstract class FlagsEnumZeroMemberBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S2346";
        protected override string MessageFormat => "Rename '{0}' to 'None'.";

        protected FlagsEnumZeroMemberBase() : base(DiagnosticId) { }

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
                {
                    if (c.Node.HasFlagsAttribute(c.Model)
                        && ZeroMember(c.Node, c.Model) is { } zeroMember
                        && Language.Syntax.NodeIdentifier(zeroMember) is { } identifier
                        && identifier.ValueText != "None")
                    {
                        c.ReportIssue(Rule, zeroMember, identifier.ValueText);
                    }
                },
                Language.SyntaxKind.EnumDeclaration);

        private SyntaxNode ZeroMember(SyntaxNode node, SemanticModel semanticModel)
        {
            foreach (var item in Language.Syntax.EnumMembers(node))
            {
                if (semanticModel.GetDeclaredSymbol(item) is IFieldSymbol symbol && symbol.ConstantValue is { } constValue)
                {
                    try
                    {
                        if (Convert.ToInt64(constValue) == 0)
                        {
                            return item;
                        }
                    }
                    catch (OverflowException)
                    {
                        return null;
                    }
                }
            }
            return null;
        }
    }
}
