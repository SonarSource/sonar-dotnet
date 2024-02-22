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

public static class CompilationStartAnalysisContextExtensions
{
    {
        {
            return static (_, _, _) => { };
        }

            var contextParameter = Parameter(typeof(CompilationStartAnalysisContext));
            var symbolKindParameter = Parameter(typeof(SymbolKind));



                contextParameter, shimmedActionParameter, symbolKindParameter).Compile();
        }
        else
        {
            return static (_, _, _) => { };
        }

        Expression PassThroughLambda<T>(string registrationMethodName)
        {
            var registerParameter = Parameter(typeof(Action<T>));
            return Lambda<Action<Action<T>>>(Call(symbolStartAnalysisContextParameter, registrationMethodName, [], registerParameter), registerParameter);
        }

        MethodCallExpression DebugPrint(Expression expression) =>
            Call(typeof(Debug).GetMethod(nameof(Debug.WriteLine), [typeof(object)]), Convert(expression, typeof(object)));
    }

    public static void RegisterSymbolStartAction(this CompilationStartAnalysisContext context, Action<SymbolStartAnalysisContext> action, SymbolKind symbolKind) =>
        RegisterSymbolStartAnalysisWrapper(context, action, symbolKind);
}
