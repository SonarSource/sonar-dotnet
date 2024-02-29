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
    private static readonly Action<CompilationStartAnalysisContext, Action<SymbolStartAnalysisContext>, SymbolKind> RegisterSymbolStartActionWrapper = CreateRegisterSymbolStartAnalysisWrapper();

    public static void RegisterSymbolStartAction(this CompilationStartAnalysisContext context, Action<SymbolStartAnalysisContext> action, SymbolKind symbolKind) =>
        RegisterSymbolStartActionWrapper(context, action, symbolKind);

    private static Action<CompilationStartAnalysisContext, Action<SymbolStartAnalysisContext>, SymbolKind> CreateRegisterSymbolStartAnalysisWrapper()
    {
#pragma warning disable S103 // Lines should not be too long
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

        // The Sonar.SymbolStartAnalysisContext is a copy of the Roslyn.SymbolStartAnalysisContext because Roslyn.SymbolStartAnalysisContext is not available in our Roslyn version.
        // The Lambda below creates a method, that takes the Roslyn.SymbolStartAnalysisContext, creates a Sonar.SymbolStartAnalysisContext copy of it, and passes the copy to the
        // Action<Sonar.SymbolStartAnalysisContext> provided by the caller. The tricky part is "copying" the Register.. methods. We do so by wrapping them into delegates and passing these.
        // These delegates are very simple: They just take the parameters for the registration method and pass them to the corresponding Roslyn.SymbolStartAnalysisContext.Register.. method.
        // This doesn't work particular well for generic methods (like CodeBlockStartAction<TLanguageKindEnum>) because we can not pass an open generic Action<> to the Sonar.SymbolStartAnalysisContext
        // constructor. But the affected methods only allow CS.SyntaxKind and VB.SyntaxKind as type parameters anyway, so we can created two closed generic versions for these methods. The VB
        // version is not supported (yet) because the Microsoft.CodeAnalysis.VisualBasic.Workspaces is not referenced in this project.

        // Action<Roslyn.SymbolStartAnalysisContext> lambda = symbolStartAnalysisContextParameter =>
        //    shimmedActionParameter(new Sonar.SymbolStartAnalysisContext(
        //        symbolStartAnalysisContextParameter.CancellationToken,
        //        symbolStartAnalysisContextParameter.Compilation,
        //        symbolStartAnalysisContextParameter.Options,
        //        symbolStartAnalysisContextParameter.Symbol,
        //        (Action<CodeBlockAnalysisContext> registerActionParameter) =>  symbolStartAnalysisContextParameter.RegisterCodeBlockAction(registerActionParameter),
        //        (Action<CodeBlockStartAnalysisContext<CS.SyntaxKind>> registerActionParameter) =>  symbolStartAnalysisContextParameter.CodeBlockStartAction<CS.SyntaxKind>(registerActionParameter),
        //        (Action<OperationAnalysisContext> registerActionParameter, ImmutableArray<OperationKind> additionalParameter) =>  symbolStartAnalysisContextParameter.RegisterOperationAction(registerActionParameter, additionalParameter),
        //        (Action<OperationBlockAnalysisContext> registerActionParameter) =>  symbolStartAnalysisContextParameter.RegisterOperationBlockAction(registerActionParameter),
        //        (Action<OperationBlockStartAnalysisContext> registerActionParameter) =>  symbolStartAnalysisContextParameter.RegisterOperationBlockStartAction(registerActionParameter),
        //        (Action<SymbolAnalysisContext> registerActionParameter) =>  symbolStartAnalysisContextParameter.RegisterSymbolEndAction(registerActionParameter),
        //        (Action<SyntaxNodeAnalysisContext> registerActionParameter, ImmutableArray<CS.SyntaxKind> additionalParameter) =>  symbolStartAnalysisContextParameter.RegisterSyntaxNodeAction<CS.SyntaxKind>(registerActionParameter, additionalParameter))
        var lambda = Lambda(symbolStartAnalysisActionType, Call(shimmedActionParameter, nameof(Action.Invoke), [],
                New(symbolStartAnalysisContextCtor,
                    Property(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.CancellationToken)),
                    Property(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.Compilation)),
                    Property(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.Options)),
                    Property(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.Symbol)),
                    RegisterLambda<CodeBlockAnalysisContext>(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.RegisterCodeBlockAction)),
                    RegisterLambda<CodeBlockStartAnalysisContext<CS.SyntaxKind>>(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.RegisterCodeBlockStartAction), typeof(CS.SyntaxKind)),
                    RegisterLambdaWithAdditionalParameter<OperationAnalysisContext, ImmutableArray<OperationKind>>(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.RegisterOperationAction)),
                    RegisterLambda<OperationBlockAnalysisContext>(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.RegisterOperationBlockAction)),
                    RegisterLambda<OperationBlockStartAnalysisContext>(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.RegisterOperationBlockStartAction)),
                    RegisterLambda<SymbolAnalysisContext>(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.RegisterSymbolEndAction)),
                    RegisterLambdaWithAdditionalParameter<SyntaxNodeAnalysisContext, ImmutableArray<CS.SyntaxKind>>(symbolStartAnalysisContextParameter, nameof(SymbolStartAnalysisContext.RegisterSyntaxNodeAction), typeof(CS.SyntaxKind)))),
                symbolStartAnalysisContextParameter);

        // (contextParameter, shimmedActionParameter, symbolKindParameter) => contextParameter.RegisterSymbolStartAction(lambda), symbolKindParameter)
        return Lambda<Action<CompilationStartAnalysisContext, Action<SymbolStartAnalysisContext>, SymbolKind>>(
            Call(contextParameter, registerMethod, lambda, symbolKindParameter),
            contextParameter, shimmedActionParameter, symbolKindParameter).Compile();

        // registerActionParameter => symbolStartAnalysisContextParameter."registrationMethodName"<typeArguments>(registerActionParameter)
        static Expression<Action<Action<TContext>>> RegisterLambda<TContext>(ParameterExpression symbolStartAnalysisContextParameter, string registrationMethodName, params Type[] typeArguments)
        {
            var registerActionParameter = Parameter(typeof(Action<TContext>));
            return Lambda<Action<Action<TContext>>>(Call(symbolStartAnalysisContextParameter, registrationMethodName, typeArguments, registerActionParameter), registerActionParameter);
        }

        // (registerActionParameter, additionalParameter) => symbolStartAnalysisContextParameter."registrationMethodName"<typeArguments>(registerActionParameter, additionalParameter)
        static Expression<Action<Action<TContext>, TParameter>> RegisterLambdaWithAdditionalParameter<TContext, TParameter>(
            ParameterExpression symbolStartAnalysisContextParameter, string registrationMethodName, params Type[] typeArguments)
        {
            var registerActionParameter = Parameter(typeof(Action<TContext>));
            var additionalParameter = Parameter(typeof(TParameter));
            return Lambda<Action<Action<TContext>, TParameter>>(
                Call(symbolStartAnalysisContextParameter, registrationMethodName, typeArguments, registerActionParameter, additionalParameter), registerActionParameter, additionalParameter);
        }
#pragma warning restore S103 // Lines should not be too long
    }
}
