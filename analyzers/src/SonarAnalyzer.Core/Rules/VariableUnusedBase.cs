/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Rules;

public abstract class VariableUnusedBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    protected abstract bool IsExcludedDeclaration(SyntaxNode node);

    protected override string MessageFormat => "Remove the unused local variable '{0}'.";

    protected VariableUnusedBase() : base("S1481") { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCodeBlockStartAction<TSyntaxKind>(Language.GeneratedCodeRecognizer, cbc =>
            {
                // Two-pass approach: in pass 1, declaration nodes populate declaredLocalNames and declaredLocals.
                // All IdentifierName nodes are buffered into pendingIdentifiers. In pass 2 (the end-of-block
                // action), GetSymbolInfo is called only on buffered identifiers whose text appears in
                // declaredLocalNames — avoiding the expensive call for every unrelated identifier in the block.
                //
                // Three collections are needed rather than two: declaredLocalNames serves as a cheap text-based
                // pre-filter while declaredLocals holds the resolved symbols for the unused check. Combining them
                // into a Dictionary<string, ISymbol> would break shadowing — the same name can map to multiple
                // symbols (e.g. 'x' in two sequential blocks), so the relationship is one-to-many.
                var declaredLocalNames = new HashSet<string>(Language.NameComparer);
                var declaredLocals = new HashSet<ISymbol>();
                var pendingIdentifiers = new List<SyntaxNode>();

                cbc.RegisterNodeAction(c => CollectDeclaration(c, declaredLocalNames, declaredLocals), Language.SyntaxKind.LocalDeclarationKinds);
                cbc.RegisterNodeAction(c => pendingIdentifiers.Add(c.Node), Language.SyntaxKind.IdentifierName);
                cbc.RegisterCodeBlockEndAction(c => ReportUnused(c, declaredLocalNames, declaredLocals, pendingIdentifiers));
            });

    private void CollectDeclaration(SonarSyntaxNodeReportingContext c, HashSet<string> declaredLocalNames, HashSet<ISymbol> declaredLocals)
    {
        if (!IsExcludedDeclaration(c.Node)
            && c.Model.GetDeclaredSymbol(c.Node) is (ILocalSymbol or IRangeVariableSymbol) and { Name: not "_" } symbol)
        {
            declaredLocalNames.Add(symbol.Name);
            declaredLocals.Add(symbol);
        }
    }

    private void ReportUnused(SonarCodeBlockReportingContext c, HashSet<string> declaredLocalNames, HashSet<ISymbol> declaredLocals, List<SyntaxNode> pendingIdentifiers)
    {
        if (declaredLocalNames.Count == 0)
        {
            return;
        }
        var usedLocals = new HashSet<ISymbol>();
        foreach (var id in pendingIdentifiers)
        {
            if (Language.Syntax.NodeIdentifier(id) is { ValueText: var idText } && declaredLocalNames.Contains(idText))
            {
                usedLocals.UnionWith(c.Model.GetSymbolInfo(id).AllSymbols());
            }
        }
        foreach (var unused in declaredLocals.Except(usedLocals))
        {
            c.ReportIssue(Rule, unused.Locations.First(), unused.Name);
        }
    }
}
