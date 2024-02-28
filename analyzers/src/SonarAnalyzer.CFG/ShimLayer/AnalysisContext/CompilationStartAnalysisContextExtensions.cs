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

using System.Linq.Expressions;

using static System.Linq.Expressions.Expression;
using CS = Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.ShimLayer.AnalysisContext;

public static class CompilationStartAnalysisContextExtensions
{
    private static readonly Action<CompilationStartAnalysisContext, Action<SymbolStartAnalysisContext>, SymbolKind> RegisterSymbolStartAnalysisWrapper = CreateRegisterSymbolStartAnalysisWrapper();
    private static Action<CompilationStartAnalysisContext, Action<SymbolStartAnalysisContext>, SymbolKind> CreateRegisterSymbolStartAnalysisWrapper()
    {
        if (typeof(CompilationStartAnalysisContext).GetMethod(nameof(RegisterSymbolStartAction)) is not { } registerMethod)
        {
            return static (_, _, _) => { };
        }
        var contextParameter = Parameter(typeof(CompilationStartAnalysisContext));
        var shimmedActionParameter = Parameter(typeof(Action<SymbolStartAnalysisContext>));
        var symbolKindParameter = Parameter(typeof(SymbolKind));

        var symbolStartAnalysisContextType = typeof(CompilationStartAnalysisContext).Assembly.GetType("Microsoft.CodeAnalysis.Diagnostics.SymbolStartAnalysisContext");
        var symbolStartAnalysisActionType = typeof(Action<>).MakeGenericType(symbolStartAnalysisContextType);
        var symbolStartAnalysisContextParameter = Parameter(symbolStartAnalysisContextType);
        var symbolStartAnalysisContextCtor = typeof(SymbolStartAnalysisContext).GetConstructors().Single();

        // Action<Roslyn.SymbolStartAnalysisContext> lambda = symbolStartAnalysisContextParameter =>
        //    shimmedActionParameter(new Sonar.SymbolStartAnalysisContext(
        //        symbolStartAnalysisContextParameter.CancellationToken,
        //        symbolStartAnalysisContextParameter.Compilation,
        //        symbolStartAnalysisContextParameter.Options,
        //        symbolStartAnalysisContextParameter.Symbol,
        //        (Action<CodeBlockAnalysisContext> registerActionParameter) =>  symbolStartAnalysisContextParameter.RegisterCodeBlockAction(registerActionParameter),
        var lambda = Lambda(symbolStartAnalysisActionType, Call(shimmedActionParameter, nameof(Action.Invoke), [],
                New(symbolStartAnalysisContextCtor,
                    Property(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.CancellationToken)),
                    Property(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.Compilation)),
                    Property(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.Options)),
                    Property(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.Symbol)),
                    RegisterLambda<CodeBlockAnalysisContext>(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.RegisterCodeBlockAction)),
                    RegisterLambda<CodeBlockStartAnalysisContext<CS.SyntaxKind>>(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.RegisterCodeBlockStartAction), typeof(CS.SyntaxKind)),
                    RegisterLambdaWithAdditionalParameter<OperationAnalysisContext, ImmutableArray<OperationKind>>(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.RegisterOperationAction))
                    )),
                symbolStartAnalysisContextParameter);

        // (contextParameter, shimmedActionParameter, symbolKindParameter) => contextParameter.RegisterSymbolStartAction(lambda), symbolKindParameter)
        return Lambda<Action<CompilationStartAnalysisContext, Action<SymbolStartAnalysisContext>, SymbolKind>>(
            Call(contextParameter, registerMethod, lambda, symbolKindParameter),
            contextParameter, shimmedActionParameter, symbolKindParameter).Compile();

        static Expression<Action<Action<TContext>>> RegisterLambda<TContext>(ParameterExpression symbolStartAnalysisContextParameter, string registrationMethodName, params Type[] typeArguments)
        {
            var registerActionParameter = Parameter(typeof(Action<TContext>));
            return Lambda<Action<Action<TContext>>>(Call(symbolStartAnalysisContextParameter, registrationMethodName, typeArguments, registerActionParameter), registerActionParameter);
        }

        static Expression<Action<Action<TContext>, TParameter>> RegisterLambdaWithAdditionalParameter<TContext, TParameter>(
            ParameterExpression symbolStartAnalysisContextParameter, string registrationMethodName, params Type[] typeArguments)
        {
            var registerActionParameter = Parameter(typeof(Action<TContext>));
            var additionalParameter = Parameter(typeof(TParameter));
            return Lambda<Action<Action<TContext>, TParameter>>(
                Call(symbolStartAnalysisContextParameter, registrationMethodName, typeArguments, registerActionParameter, additionalParameter), registerActionParameter, additionalParameter);
        }
    }

    public static void RegisterSymbolStartAction(this CompilationStartAnalysisContext context, Action<SymbolStartAnalysisContext> action, SymbolKind symbolKind) =>
        RegisterSymbolStartAnalysisWrapper(context, action, symbolKind);
}
