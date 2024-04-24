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

using System.Diagnostics.CodeAnalysis;
using static System.Linq.Expressions.Expression;
using CS = Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.ShimLayer.AnalysisContext;

public readonly struct SymbolStartAnalysisContextWrapper
{
    private const string VBSyntaxKind = "Microsoft.CodeAnalysis.VisualBasic.SyntaxKind";

    private static readonly Func<object, CancellationToken> CancellationTokenAccessor;
    private static readonly Func<object, Compilation> CompilationAccessor;
    private static readonly Func<object, AnalyzerOptions> OptionsAccessor;
    private static readonly Func<object, ISymbol> SymbolAccessor;

    private static readonly Action<object, Action<CodeBlockAnalysisContext>> RegisterCodeBlockActionMethod;
    private static readonly Action<object, Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>>> RegisterCodeBlockStartActionCS;
    private static readonly Action<object, Action<object>> RegisterCodeBlockStartActionVB;
    private static readonly Action<object, Action<OperationAnalysisContext>, ImmutableArray<OperationKind>> RegisterOperationActionMethod;
    private static readonly Action<object, Action<OperationBlockAnalysisContext>> RegisterOperationBlockActionMethod;
    private static readonly Action<object, Action<OperationBlockStartAnalysisContext>> RegisterOperationBlockStartActionMethod;
    private static readonly Action<object, Action<SymbolAnalysisContext>> RegisterSymbolEndActionMethod;
    private static readonly Action<object, Action<SyntaxNodeAnalysisContext>, ImmutableArray<CS.SyntaxKind>> RegisterSyntaxNodeActionCS;

    public CancellationToken CancellationToken => CancellationTokenAccessor(RoslynSymbolStartAnalysisContext);
    public Compilation Compilation => CompilationAccessor(RoslynSymbolStartAnalysisContext);
    public AnalyzerOptions Options => OptionsAccessor(RoslynSymbolStartAnalysisContext);
    public ISymbol Symbol => SymbolAccessor(RoslynSymbolStartAnalysisContext);
    private object RoslynSymbolStartAnalysisContext { get; }

    // Code is executed in static initializers and is not detected by the coverage tool
    // See the RegisterSymbolStartActionWrapperTest family of tests to check test coverage manually
    [ExcludeFromCodeCoverage]
    static SymbolStartAnalysisContextWrapper()
    {
        var symbolStartAnalysisContextType = typeof(CompilationStartAnalysisContext).Assembly.GetType("Microsoft.CodeAnalysis.Diagnostics.SymbolStartAnalysisContext");
        var languageKindEnumVBType = Type.GetType("Microsoft.CodeAnalysis.VisualBasic.SyntaxKind, Microsoft.CodeAnalysis.VisualBasic, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        CancellationTokenAccessor = CreatePropertyAccessor<CancellationToken>(nameof(CancellationToken));
        CompilationAccessor = CreatePropertyAccessor<Compilation>(nameof(Compilation));
        OptionsAccessor = CreatePropertyAccessor<AnalyzerOptions>(nameof(Options));
        SymbolAccessor = CreatePropertyAccessor<ISymbol>(nameof(Symbol));
        RegisterCodeBlockActionMethod = CreateRegistrationMethod<CodeBlockAnalysisContext>(nameof(RegisterCodeBlockAction));
        RegisterCodeBlockStartActionCS =
            CreateRegistrationMethod<CodeBlockStartAnalysisContext<CS.SyntaxKind>>(nameof(RegisterCodeBlockStartAction), typeof(CS.SyntaxKind));
        RegisterCodeBlockStartActionVB = CreateRegistrationMethodCodeBlockStart(languageKindEnumVBType);
        RegisterOperationActionMethod =
            CreateRegistrationMethodWithAdditionalParameter<OperationAnalysisContext, ImmutableArray<OperationKind>>(nameof(RegisterOperationAction));
        RegisterOperationBlockActionMethod = CreateRegistrationMethod<OperationBlockAnalysisContext>(nameof(RegisterOperationBlockAction));
        RegisterOperationBlockStartActionMethod = CreateRegistrationMethod<OperationBlockStartAnalysisContext>(nameof(RegisterOperationBlockStartAction));
        RegisterSymbolEndActionMethod = CreateRegistrationMethod<SymbolAnalysisContext>(nameof(RegisterSymbolEndAction));
        RegisterSyntaxNodeActionCS = CreateRegistrationMethodWithAdditionalParameter<SyntaxNodeAnalysisContext, ImmutableArray<CS.SyntaxKind>>(
            nameof(RegisterSyntaxNodeAction), typeof(CS.SyntaxKind));

        // receiverParameter => ((symbolStartAnalysisContextType)receiverParameter)."propertyName"
        Func<object, TProperty> CreatePropertyAccessor<TProperty>(string propertyName)
        {
            if (symbolStartAnalysisContextType == null)
            {
                return static _ => default;
            }
            var receiverParameter = Parameter(typeof(object));
            return Lambda<Func<object, TProperty>>(
                Property(Convert(receiverParameter, symbolStartAnalysisContextType), propertyName),
                receiverParameter).Compile();
        }

        // (object receiverParameter, Action<TContext> registerActionParameter) =>
        //     ((symbolStartAnalysisContextType)receiverParameter)."registrationMethodName"<typeArguments>(registerActionParameter)
        Action<object, Action<TContext>> CreateRegistrationMethod<TContext>(string registrationMethodName, params Type[] typeArguments)
        {
            if (symbolStartAnalysisContextType == null)
            {
                return static (_, _) => { };
            }
            var receiverParameter = Parameter(typeof(object));
            var registerActionParameter = Parameter(typeof(Action<TContext>));
            return Lambda<Action<object, Action<TContext>>>(
                Call(Convert(receiverParameter, symbolStartAnalysisContextType), registrationMethodName, typeArguments, registerActionParameter),
                receiverParameter,
                registerActionParameter).Compile();
        }

        // (object receiverParameter, Action<object> actionObjectParameter) =>
        //     ((symbolStartAnalysisContextType)receiverParameter).RegisterCodeBlockStartAction<languageKindEnumType>(contextLanguageParameter => actionObjectParameter.Invoke(contextLanguageParameter))
        Action<object, Action<object>> CreateRegistrationMethodCodeBlockStart(Type languageKindEnumType)
        {
            if (symbolStartAnalysisContextType == null || languageKindEnumType == null)
            {
                return static (_, _) => { };
            }
            var receiverParameter = Parameter(typeof(object));
            var actionObjectParameter = Parameter(typeof(Action<object>));
            var contextLanguageType = typeof(CodeBlockStartAnalysisContext<>).MakeGenericType(languageKindEnumType);
            var actionContextLanguageType = typeof(Action<>).MakeGenericType(contextLanguageType);
            var contextLanguageParameter = Parameter(contextLanguageType);
            var registerActionParameter = Parameter(actionContextLanguageType);
            // Action<CodeBlockStartAnalysisContext<languageKindEnumType>> innerRegistration = contextLanguageParameter => actionObjectParameter.Invoke(contextLanguageParameter)
            var innerRegistration = Lambda(actionContextLanguageType, Call(actionObjectParameter, nameof(Action.Invoke), [], contextLanguageParameter), contextLanguageParameter);
            return Lambda<Action<object, Action<object>>>(
                Call(Convert(receiverParameter, symbolStartAnalysisContextType), nameof(RegisterCodeBlockStartAction), [languageKindEnumType], innerRegistration),
                receiverParameter,
                actionObjectParameter).Compile();
        }

        // (object receiverParameter, Action<TContext> registerActionParameter, TParameter additionalParameter) =>
        //     ((symbolStartAnalysisContextType)receiverParameter)."registrationMethodName"<typeArguments>(registerActionParameter, additionalParameter)
        Action<object, Action<TContext>, TParameter> CreateRegistrationMethodWithAdditionalParameter<TContext, TParameter>(string registrationMethodName, params Type[] typeArguments)
        {
            if (symbolStartAnalysisContextType == null)
            {
                return static (_, _, _) => { };
            }
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
        RegisterCodeBlockActionMethod(RoslynSymbolStartAnalysisContext, action);

    public void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct
    {
        var languageKindType = typeof(TLanguageKindEnum);
        if (languageKindType == typeof(CS.SyntaxKind))
        {
            var cast = (Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>>)action;
            RegisterCodeBlockStartActionCS(RoslynSymbolStartAnalysisContext, cast);
        }
        else if (languageKindType.FullName == VBSyntaxKind)
        {
            Action<object> wrapper = x => action((CodeBlockStartAnalysisContext<TLanguageKindEnum>)x);
            RegisterCodeBlockStartActionVB(RoslynSymbolStartAnalysisContext, wrapper);
        }
        else
        {
            throw new ArgumentException("Invalid type parameter.", nameof(TLanguageKindEnum));
        }
    }

    public void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds) =>
        RegisterOperationActionMethod(RoslynSymbolStartAnalysisContext, action, operationKinds);

    public void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action) =>
        RegisterOperationBlockActionMethod(RoslynSymbolStartAnalysisContext, action);

    public void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action) =>
        RegisterOperationBlockStartActionMethod(RoslynSymbolStartAnalysisContext, action);

    public void RegisterSymbolEndAction(Action<SymbolAnalysisContext> action) =>
        RegisterSymbolEndActionMethod(RoslynSymbolStartAnalysisContext, action);

    public void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct
    {
        var languageKindType = typeof(TLanguageKindEnum);
        if (languageKindType == typeof(CS.SyntaxKind))
        {
            RegisterSyntaxNodeActionCS(RoslynSymbolStartAnalysisContext, action, syntaxKinds.Cast<CS.SyntaxKind>().ToImmutableArray());
        }
        else if (languageKindType.FullName == VBSyntaxKind)
        {
            // See RegisterCodeBlockStartAction for how to implement this
        }
        else
        {
            throw new ArgumentException("Invalid type parameter.", nameof(TLanguageKindEnum));
        }
    }
}
