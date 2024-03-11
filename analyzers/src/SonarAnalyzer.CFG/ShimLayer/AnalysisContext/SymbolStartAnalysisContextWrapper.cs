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
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.ShimLayer.AnalysisContext;

public readonly struct SymbolStartAnalysisContextWrapper
{
    private static Func<object, CancellationToken> cancellationTokenAccessor;
    private static Func<object, Compilation> compilationAccessor;
    private static Func<object, AnalyzerOptions> optionsAccessor;
    private static Func<object, ISymbol> symbolAccessor;

    private static Action<object, Action<CodeBlockAnalysisContext>> registerCodeBlockAction;
    private static Action<object, Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>>> registerCodeBlockStartActionCS;
    private static Action<object, Action<CodeBlockStartAnalysisContext<VB.SyntaxKind>>> registerCodeBlockStartActionVB;
    private static Action<object, Action<OperationAnalysisContext>, ImmutableArray<OperationKind>> registerOperationAction;
    private static Action<object, Action<OperationBlockAnalysisContext>> registerOperationBlockAction;
    private static Action<object, Action<OperationBlockStartAnalysisContext>> registerOperationBlockStartAction;
    private static Action<object, Action<SymbolAnalysisContext>> registerSymbolEndAction;
    private static Action<object, Action<SyntaxNodeAnalysisContext>, ImmutableArray<CS.SyntaxKind>> registerSyntaxNodeActionCS;
    private static Action<object, Action<SyntaxNodeAnalysisContext>, ImmutableArray<VB.SyntaxKind>> registerSyntaxNodeActionVB;

    private object RoslynSymbolStartAnalysisContext { get; }
    public CancellationToken CancellationToken => cancellationTokenAccessor(RoslynSymbolStartAnalysisContext);
    public Compilation Compilation => compilationAccessor(RoslynSymbolStartAnalysisContext);
    public AnalyzerOptions Options => optionsAccessor(RoslynSymbolStartAnalysisContext);
    public ISymbol Symbol => symbolAccessor(RoslynSymbolStartAnalysisContext);

    static SymbolStartAnalysisContextWrapper()
    {
        var symbolStartAnalysisContextType = typeof(CompilationStartAnalysisContext).Assembly.GetType("Microsoft.CodeAnalysis.Diagnostics.SymbolStartAnalysisContext");
        cancellationTokenAccessor = CreatePropertyAccessor<CancellationToken>(nameof(CancellationToken));
        compilationAccessor = CreatePropertyAccessor<Compilation>(nameof(Compilation));
        optionsAccessor = CreatePropertyAccessor<AnalyzerOptions>(nameof(Options));
        symbolAccessor = CreatePropertyAccessor<ISymbol>(nameof(Symbol));
        registerCodeBlockAction = CreateRegistrationMethod<CodeBlockAnalysisContext>(nameof(RegisterCodeBlockAction));
        registerCodeBlockStartActionCS =
            CreateRegistrationMethod<CodeBlockStartAnalysisContext<CS.SyntaxKind>>(nameof(RegisterCodeBlockStartAction), typeof(CS.SyntaxKind));
        registerCodeBlockStartActionVB =
            CreateRegistrationMethod<CodeBlockStartAnalysisContext<VB.SyntaxKind>>(nameof(RegisterCodeBlockStartAction), typeof(VB.SyntaxKind));
        registerOperationAction =
            CreateRegistrationMethodWithAdditionalParameter<OperationAnalysisContext, ImmutableArray<OperationKind>>(nameof(RegisterOperationAction));
        registerOperationBlockAction = CreateRegistrationMethod<OperationBlockAnalysisContext>(nameof(RegisterOperationBlockAction));
        registerOperationBlockStartAction = CreateRegistrationMethod<OperationBlockStartAnalysisContext>(nameof(RegisterOperationBlockStartAction));
        registerSymbolEndAction = CreateRegistrationMethod<SymbolAnalysisContext>(nameof(RegisterSymbolEndAction));
        registerSyntaxNodeActionCS = CreateRegistrationMethodWithAdditionalParameter<SyntaxNodeAnalysisContext, ImmutableArray<CS.SyntaxKind>>(
            nameof(RegisterSyntaxNodeAction), typeof(CS.SyntaxKind));
        registerSyntaxNodeActionVB = CreateRegistrationMethodWithAdditionalParameter<SyntaxNodeAnalysisContext, ImmutableArray<VB.SyntaxKind>>(
            nameof(RegisterSyntaxNodeAction), typeof(VB.SyntaxKind));

        // receiverParameter => ((symbolStartAnalysisContextType)receiverParameter)."propertyName"
        Func<object, TProperty> CreatePropertyAccessor<TProperty>(string propertyName)
        {
            var receiverParameter = Parameter(typeof(object));
            return Lambda<Func<object, TProperty>>(
                Property(Convert(receiverParameter, symbolStartAnalysisContextType), propertyName),
                receiverParameter).Compile();
        }

        // (object receiverParameter, Action<TContext> registerActionParameter)
        //     => ((symbolStartAnalysisContextType)receiverParameter)."registrationMethodName"<typeArguments>(registerActionParameter)
        Action<object, Action<TContext>> CreateRegistrationMethod<TContext>(string registrationMethodName, params Type[] typeArguments)
        {
            var receiverParameter = Parameter(typeof(object));
            var registerActionParameter = Parameter(typeof(Action<TContext>));
            return Lambda<Action<object, Action<TContext>>>(
                Call(Convert(receiverParameter, symbolStartAnalysisContextType), registrationMethodName, typeArguments, registerActionParameter),
                receiverParameter,
                registerActionParameter).Compile();
        }

        // (object receiverParameter, Action<TContext> registerActionParameter, TParameter additionalParameter)
        //     => ((symbolStartAnalysisContextType)receiverParameter)."registrationMethodName"<typeArguments>(registerActionParameter, additionalParameter)
        Action<object, Action<TContext>, TParameter> CreateRegistrationMethodWithAdditionalParameter<TContext, TParameter>(string registrationMethodName, params Type[] typeArguments)
        {
            var receiverParameter = Parameter(typeof(object));
            var registerActionParameter = Parameter(typeof(Action<TContext>));
            var additionalParameter = Parameter(typeof(TParameter));
            return Lambda<Action<object, Action<TContext>, TParameter>>(
                Call(Convert(receiverParameter, symbolStartAnalysisContextType), registrationMethodName, typeArguments, registerActionParameter, additionalParameter),
                receiverParameter,
                registerActionParameter,
                additionalParameter).Compile();
        }
    }

    public SymbolStartAnalysisContextWrapper(object roslynSymbolStartAnalysisContext) =>
        RoslynSymbolStartAnalysisContext = roslynSymbolStartAnalysisContext;

    public void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action) =>
        registerCodeBlockAction(RoslynSymbolStartAnalysisContext, action);

    public void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct
    {
        var languageKindType = typeof(TLanguageKindEnum);
        if (languageKindType == typeof(CS.SyntaxKind))
        {
            var cast = (Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>>)action;
            registerCodeBlockStartActionCS(RoslynSymbolStartAnalysisContext, cast);
        }
        else if (languageKindType == typeof(VB.SyntaxKind))
        {
            var cast = (Action<CodeBlockStartAnalysisContext<VB.SyntaxKind>>)action;
            registerCodeBlockStartActionVB(RoslynSymbolStartAnalysisContext, cast);
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
        else if (languageKindType == typeof(VB.SyntaxKind))
        {
            registerSyntaxNodeActionVB(RoslynSymbolStartAnalysisContext, action, syntaxKinds.Cast<VB.SyntaxKind>().ToImmutableArray());
        }
        else
        {
            throw new ArgumentException("Invalid type parameter.", nameof(TLanguageKindEnum));
        }
    }
}
