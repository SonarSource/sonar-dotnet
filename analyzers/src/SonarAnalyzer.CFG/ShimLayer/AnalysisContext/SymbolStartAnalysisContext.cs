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

public readonly struct SymbolStartAnalysisContext
{
    // symbolStartAnalysisContextParameter => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter).CancellationToken
    private static Func<object, CancellationToken> cancellationTokenAccessor;
    // symbolStartAnalysisContextParameter => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter).Compilation
    private static Func<object, Compilation> compilationAccessor;
    // symbolStartAnalysisContextParameter => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter).Options
    private static Func<object, AnalyzerOptions> optionsAccessor;
    // symbolStartAnalysisContextParameter => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter).Symbol
    private static Func<object, ISymbol> symbolAccessor;

    // (object symbolStartAnalysisContextParameter, Action<CodeBlockAnalysisContext> registerActionParameter)
    //     => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter).RegisterCodeBlockAction(registerActionParameter),
    private static Action<object, Action<CodeBlockAnalysisContext>> registerCodeBlockAction;
    // (object symbolStartAnalysisContextParameter, Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>> registerActionParameter)
    //     => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter).CodeBlockStartAction<CS.SyntaxKind>(registerActionParameter),
    private static Action<object, Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>>> registerCodeBlockStartActionCS;
    // (object symbolStartAnalysisContextParameter, Action<OperationAnalysisContext> registerActionParameter, ImmutableArray<OperationKind> additionalParameter)
    //     => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter).RegisterOperationAction(registerActionParameter, additionalParameter),
    private static Action<object, Action<OperationAnalysisContext>, ImmutableArray<OperationKind>> registerOperationAction;
    // (object symbolStartAnalysisContextParameter, Action<OperationBlockAnalysisContext> registerActionParameter)
    //     => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter).RegisterOperationBlockAction(registerActionParameter),
    private static Action<object, Action<OperationBlockAnalysisContext>> registerOperationBlockAction;
    // (object symbolStartAnalysisContextParameter, Action<OperationBlockStartAnalysisContext> registerActionParameter)
    //     => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter).RegisterOperationBlockStartAction(registerActionParameter),
    private static Action<object, Action<OperationBlockStartAnalysisContext>> registerOperationBlockStartAction;
    // (object symbolStartAnalysisContextParameter, Action<SymbolAnalysisContext> registerActionParameter)
    //     => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter).RegisterSymbolEndAction(registerActionParameter),
    private static Action<object, Action<SymbolAnalysisContext>> registerSymbolEndAction;
    // (object symbolStartAnalysisContextParameter, Action<SyntaxNodeAnalysisContext> registerActionParameter, ImmutableArray<CS.SyntaxKind> additionalParameter)
    //     => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter).RegisterSyntaxNodeAction<CS.SyntaxKind>(registerActionParameter, additionalParameter))
    private static Action<object, Action<SyntaxNodeAnalysisContext>, ImmutableArray<CS.SyntaxKind>> registerSyntaxNodeActionCS;

    static SymbolStartAnalysisContext()
    {
        var symbolStartAnalysisContextType = typeof(CompilationStartAnalysisContext).Assembly.GetType("Microsoft.CodeAnalysis.Diagnostics.SymbolStartAnalysisContext");
        cancellationTokenAccessor = CreatePropertyAccessor<CancellationToken>(symbolStartAnalysisContextType, nameof(CancellationToken));
        compilationAccessor = CreatePropertyAccessor<Compilation>(symbolStartAnalysisContextType, nameof(Compilation));
        optionsAccessor = CreatePropertyAccessor<AnalyzerOptions>(symbolStartAnalysisContextType, nameof(Options));
        symbolAccessor = CreatePropertyAccessor<ISymbol>(symbolStartAnalysisContextType, nameof(Symbol));
        registerCodeBlockAction = CreateRegistrationMethod<CodeBlockAnalysisContext>(symbolStartAnalysisContextType, nameof(RegisterCodeBlockAction));
        registerCodeBlockStartActionCS =
            CreateRegistrationMethod<CodeBlockStartAnalysisContext<CS.SyntaxKind>>(symbolStartAnalysisContextType, nameof(RegisterCodeBlockStartAction), typeof(CS.SyntaxKind));
        registerOperationAction =
            CreateRegistrationMethodWithAdditionalParameter<OperationAnalysisContext, ImmutableArray<OperationKind>>(symbolStartAnalysisContextType, nameof(RegisterOperationAction));
        registerOperationBlockAction = CreateRegistrationMethod<OperationBlockAnalysisContext>(symbolStartAnalysisContextType, nameof(RegisterOperationBlockAction));
        registerOperationBlockStartAction = CreateRegistrationMethod<OperationBlockStartAnalysisContext>(symbolStartAnalysisContextType, nameof(RegisterOperationBlockStartAction));
        registerSymbolEndAction = CreateRegistrationMethod<SymbolAnalysisContext>(symbolStartAnalysisContextType, nameof(RegisterSymbolEndAction));
        registerSyntaxNodeActionCS = CreateRegistrationMethodWithAdditionalParameter<SyntaxNodeAnalysisContext, ImmutableArray<CS.SyntaxKind>>(
            symbolStartAnalysisContextType, nameof(RegisterSyntaxNodeAction), typeof(CS.SyntaxKind));
    }

    public SymbolStartAnalysisContext(object roslynSymbolStartAnalysisContext)
    {
        RoslynSymbolStartAnalysisContext = roslynSymbolStartAnalysisContext;
    }
    public object RoslynSymbolStartAnalysisContext { get; }
    public CancellationToken CancellationToken => cancellationTokenAccessor(RoslynSymbolStartAnalysisContext);
    public Compilation Compilation => compilationAccessor(RoslynSymbolStartAnalysisContext);
    public AnalyzerOptions Options => optionsAccessor(RoslynSymbolStartAnalysisContext);
    public ISymbol Symbol => symbolAccessor(RoslynSymbolStartAnalysisContext);

    public void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action) =>
        registerCodeBlockAction(RoslynSymbolStartAnalysisContext, action);

    public void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct
    {
        var languageKindType = typeof(TLanguageKindEnum);
        if (languageKindType == typeof(CS.SyntaxKind))
        {
            var casted = (Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>>)action;
            registerCodeBlockStartActionCS(RoslynSymbolStartAnalysisContext, casted);
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
        registerOperationAction(RoslynSymbolStartAnalysisContext, action, operationKinds);

    public void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action) =>
        registerOperationBlockAction(RoslynSymbolStartAnalysisContext, action);

    public void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action) =>
        registerOperationBlockStartAction(RoslynSymbolStartAnalysisContext, action);

    public void RegisterSymbolEndAction(Action<SymbolAnalysisContext> action) =>
        registerSymbolEndAction(RoslynSymbolStartAnalysisContext, action);

    public void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct
    {
        var languageKindType = typeof(TLanguageKindEnum);
        if (languageKindType == typeof(CS.SyntaxKind))
        {
            registerSyntaxNodeActionCS(RoslynSymbolStartAnalysisContext, action, syntaxKinds.Cast<CS.SyntaxKind>().ToImmutableArray());
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

    // symbolStartAnalysisContextParameter => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter)."propertyName"
    private static Func<object, TProperty> CreatePropertyAccessor<TProperty>(Type symbolStartAnalysisContextType, string propertyName)
    {
        var symbolStartAnalysisContextParameter = Parameter(typeof(object));
        return Lambda<Func<object, TProperty>>(
            Property(
                Convert(symbolStartAnalysisContextParameter, symbolStartAnalysisContextType), propertyName),
            symbolStartAnalysisContextParameter).Compile();
    }

    // (object symbolStartAnalysisContextParameter, Action<TContext> registerActionParameter)
    //     => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter)."registrationMethodName"<typeArguments>(registerActionParameter)
    private static Action<object, Action<TContext>> CreateRegistrationMethod<TContext>(Type symbolStartAnalysisContextType, string registrationMethodName, params Type[] typeArguments)
    {
        var receiver = Parameter(typeof(object));
        var registerActionParameter = Parameter(typeof(Action<TContext>));
        return Lambda<Action<object, Action<TContext>>>(
            Call(Convert(receiver, symbolStartAnalysisContextType), registrationMethodName, typeArguments, registerActionParameter), receiver, registerActionParameter).Compile();
    }

    // (object symbolStartAnalysisContextParameter, Action<TContext> registerActionParameter, TParameter additionalParameter)
    //     => ((symbolStartAnalysisContextType)symbolStartAnalysisContextParameter)."registrationMethodName"<typeArguments>(registerActionParameter, additionalParameter)
    private static Action<object, Action<TContext>, TParameter> CreateRegistrationMethodWithAdditionalParameter<TContext, TParameter>(
        Type symbolStartAnalysisContextType, string registrationMethodName, params Type[] typeArguments)
    {
        var receiver = Parameter(typeof(object));
        var registerActionParameter = Parameter(typeof(Action<TContext>));
        var additionalParameter = Parameter(typeof(TParameter));
        return Lambda<Action<object, Action<TContext>, TParameter>>(Call(Convert(receiver, symbolStartAnalysisContextType), registrationMethodName, typeArguments,
            registerActionParameter, additionalParameter), receiver, registerActionParameter, additionalParameter).Compile();
    }
}
