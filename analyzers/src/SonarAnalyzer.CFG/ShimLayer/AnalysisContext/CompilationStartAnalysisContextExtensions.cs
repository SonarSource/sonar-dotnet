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

namespace SonarAnalyzer.ShimLayer.AnalysisContext;

// Code is executed in static initializers and is not detected by the coverage tool
// See the SonarAnalysisContextTest.SonarCompilationStartAnalysisContext_RegisterSymbolStartAction family of tests to check test coverage manually
[ExcludeFromCodeCoverage]
public static class CompilationStartAnalysisContextExtensions
{
    {
        if (typeof(CompilationStartAnalysisContext).GetMethod(nameof(RegisterSymbolStartAction)) is not { } registerMethod)
        {
            return static (_, _, _) => { };
        }

            var contextParameter = Parameter(typeof(CompilationStartAnalysisContext));
            var symbolKindParameter = Parameter(typeof(SymbolKind));
            var symbolStartAnalysisContextCtor = typeof(SymbolStartAnalysisContext).GetConstructors().Single();
                        PassThroughLambda<CodeBlockAnalysisContext>(nameof(SymbolStartAnalysisContext.RegisterCodeBlockAction))))), symbolStartAnalysisContextParameter);

                contextParameter, shimmedActionParameter, symbolKindParameter).Compile();
        }
        else
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
