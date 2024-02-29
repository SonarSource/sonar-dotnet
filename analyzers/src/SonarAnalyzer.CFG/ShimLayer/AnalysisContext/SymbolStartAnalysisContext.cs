/*
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

using Microsoft.CodeAnalysis.CSharp;
using CS = Microsoft.CodeAnalysis.CSharp;
namespace SonarAnalyzer.ShimLayer.AnalysisContext;

public class SymbolStartAnalysisContext
{
    private readonly Action<Action<CodeBlockAnalysisContext>> registerCodeBlockAction;
    private readonly Action<Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>>> registerCodeBlockStartActionCS;
    private readonly Action<Action<OperationAnalysisContext>, ImmutableArray<OperationKind>> registerOperationAction;
    private readonly Action<Action<OperationBlockAnalysisContext>> registerOperationBlockAction;
    private readonly Action<Action<OperationBlockStartAnalysisContext>> registerOperationBlockStartAction;
    private readonly Action<Action<SymbolAnalysisContext>> registerSymbolEndAction;
    private readonly Action<Action<SyntaxNodeAnalysisContext>, ImmutableArray<SyntaxKind>> registerSyntaxNodeActionCS;

    public SymbolStartAnalysisContext(
        CancellationToken cancellationToken,
        Compilation compilation,
        AnalyzerOptions options,
        ISymbol symbol,
        Action<Action<CodeBlockAnalysisContext>> registerCodeBlockAction,
        Action<Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>>> registerCodeBlockStartActionCS,
        Action<Action<OperationAnalysisContext>, ImmutableArray<OperationKind>> registerOperationAction,
        Action<Action<OperationBlockAnalysisContext>> registerOperationBlockAction,
        Action<Action<OperationBlockStartAnalysisContext>> registerOperationBlockStartAction,
        Action<Action<SymbolAnalysisContext>> registerSymbolEndAction,
        Action<Action<SyntaxNodeAnalysisContext>, ImmutableArray<CS.SyntaxKind>> registerSyntaxNodeActionCS)
    {
        CancellationToken = cancellationToken;
        Compilation = compilation;
        Options = options;
        Symbol = symbol;
        this.registerCodeBlockAction = registerCodeBlockAction;
        this.registerCodeBlockStartActionCS = registerCodeBlockStartActionCS;
        this.registerOperationAction = registerOperationAction;
        this.registerOperationBlockAction = registerOperationBlockAction;
        this.registerOperationBlockStartAction = registerOperationBlockStartAction;
        this.registerSymbolEndAction = registerSymbolEndAction;
        this.registerSyntaxNodeActionCS = registerSyntaxNodeActionCS;
    }

    public CancellationToken CancellationToken { get; }
    public Compilation Compilation { get; }
    public AnalyzerOptions Options { get; }
    public ISymbol Symbol { get; }

    public void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action) =>
        registerCodeBlockAction(action);

    public void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct
    {
        var languageKindType = typeof(TLanguageKindEnum);
        if (languageKindType == typeof(CS.SyntaxKind))
        {
            var casted = (Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>>)action;
            registerCodeBlockStartActionCS(casted);
        }
        else if (languageKindType.FullName == "Microsoft.CodeAnalysis.VisualBasic.SyntaxKind")
        {
            throw new NotImplementedException("Add a reference to the Microsoft.CodeAnalysis.VisualBasic.Workspaces package.");
        }
        else
        {
            throw new ArgumentException("Invalid type parameter.", nameof(TLanguageKindEnum));
        }
    }

    public void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds) =>
        registerOperationAction(action, operationKinds);

    public void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action) =>
        registerOperationBlockAction(action);

    public void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action) =>
        registerOperationBlockStartAction(action);

    public void RegisterSymbolEndAction(Action<SymbolAnalysisContext> action) =>
        registerSymbolEndAction(action);

    public void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct
    {
        var languageKindType = typeof(TLanguageKindEnum);
        if (languageKindType == typeof(CS.SyntaxKind))
        {
            registerSyntaxNodeActionCS(action, syntaxKinds.Cast<CS.SyntaxKind>().ToImmutableArray());
        }
        else if (languageKindType.FullName == "Microsoft.CodeAnalysis.VisualBasic.SyntaxKind")
        {
            throw new NotImplementedException("Add a reference to the Microsoft.CodeAnalysis.VisualBasic.Workspaces package.");
        }
        else
        {
            throw new ArgumentException("Invalid type parameter.", nameof(TLanguageKindEnum));
        }
    }
}
