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

using static System.Linq.Expressions.Expression;
using CS = Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.ShimLayer.AnalysisContext;

public class SymbolStartAnalysisContext
{
    private static Func<object, CancellationToken> cancellationTokenAccessor;
    private static Func<object, Compilation> compilationAccessor;
    private static Func<object, AnalyzerOptions> optionsAccessor;
    private static Func<object, ISymbol> symbolAccessor;

    private readonly Action<Action<CodeBlockAnalysisContext>> registerCodeBlockAction;
    private readonly Action<Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>>> registerCodeBlockStartActionCS;
    private readonly Action<Action<OperationAnalysisContext>, ImmutableArray<OperationKind>> registerOperationAction;
    private readonly Action<Action<OperationBlockAnalysisContext>> registerOperationBlockAction;
    private readonly Action<Action<OperationBlockStartAnalysisContext>> registerOperationBlockStartAction;
    private readonly Action<Action<SymbolAnalysisContext>> registerSymbolEndAction;
    private readonly Action<Action<SyntaxNodeAnalysisContext>, ImmutableArray<CS.SyntaxKind>> registerSyntaxNodeActionCS;

    static SymbolStartAnalysisContext()
    {
        var symbolStartAnalysisContextType = typeof(CompilationStartAnalysisContext).Assembly.GetType("Microsoft.CodeAnalysis.Diagnostics.SymbolStartAnalysisContext");
        cancellationTokenAccessor = CreatePropertyAccessor<CancellationToken>(symbolStartAnalysisContextType, nameof(CancellationToken));
        compilationAccessor = CreatePropertyAccessor<Compilation>(symbolStartAnalysisContextType, nameof(Compilation));
        optionsAccessor = CreatePropertyAccessor<AnalyzerOptions>(symbolStartAnalysisContextType, nameof(Options));
        symbolAccessor = CreatePropertyAccessor<ISymbol>(symbolStartAnalysisContextType, nameof(Symbol));
    }

    public SymbolStartAnalysisContext(
        object roslynSymbolStartAnalysisContext,
        Action<Action<CodeBlockAnalysisContext>> registerCodeBlockAction,
        Action<Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>>> registerCodeBlockStartActionCS,
        Action<Action<OperationAnalysisContext>, ImmutableArray<OperationKind>> registerOperationAction,
        Action<Action<OperationBlockAnalysisContext>> registerOperationBlockAction,
        Action<Action<OperationBlockStartAnalysisContext>> registerOperationBlockStartAction,
        Action<Action<SymbolAnalysisContext>> registerSymbolEndAction,
        Action<Action<SyntaxNodeAnalysisContext>, ImmutableArray<CS.SyntaxKind>> registerSyntaxNodeActionCS)
    {
        RoslynSymbolStartAnalysisContext = roslynSymbolStartAnalysisContext;
        this.registerCodeBlockAction = registerCodeBlockAction;
        this.registerCodeBlockStartActionCS = registerCodeBlockStartActionCS;
        this.registerOperationAction = registerOperationAction;
        this.registerOperationBlockAction = registerOperationBlockAction;
        this.registerOperationBlockStartAction = registerOperationBlockStartAction;
        this.registerSymbolEndAction = registerSymbolEndAction;
        this.registerSyntaxNodeActionCS = registerSyntaxNodeActionCS;
    }
    public object RoslynSymbolStartAnalysisContext { get; }
    public CancellationToken CancellationToken => cancellationTokenAccessor(RoslynSymbolStartAnalysisContext);
    public Compilation Compilation => compilationAccessor(RoslynSymbolStartAnalysisContext);
    public AnalyzerOptions Options => optionsAccessor(RoslynSymbolStartAnalysisContext);
    public ISymbol Symbol => symbolAccessor(RoslynSymbolStartAnalysisContext);

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

    private static Func<object, TProperty> CreatePropertyAccessor<TProperty>(Type symbolStartAnalysisContextType, string propertyName)
    {
        var symbolStartAnalysisContextParameter = Parameter(typeof(object));
        return Lambda<Func<object, TProperty>>(
            Property(
                Convert(symbolStartAnalysisContextParameter, symbolStartAnalysisContextType), propertyName),
            symbolStartAnalysisContextParameter).Compile();
    }
}
