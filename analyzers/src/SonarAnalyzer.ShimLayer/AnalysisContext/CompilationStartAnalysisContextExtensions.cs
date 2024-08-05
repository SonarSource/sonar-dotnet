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
    private static readonly Action<CompilationStartAnalysisContext, Action<SymbolStartAnalysisContextWrapper>, SymbolKind> RegisterSymbolStartActionWrapper =
        CreateRegisterSymbolStartAnalysisWrapper();

    public static void RegisterSymbolStartAction(this CompilationStartAnalysisContext context, Action<SymbolStartAnalysisContextWrapper> action, SymbolKind symbolKind) =>
        RegisterSymbolStartActionWrapper(context, action, symbolKind);

    // Code is executed in static initializers and is not detected by the coverage tool
    // See the SonarAnalysisContextTest.SonarCompilationStartAnalysisContext_RegisterSymbolStartAction family of tests to check test coverage manually
    [ExcludeFromCodeCoverage]
    private static Action<CompilationStartAnalysisContext, Action<SymbolStartAnalysisContextWrapper>, SymbolKind> CreateRegisterSymbolStartAnalysisWrapper()
    {
        if (typeof(CompilationStartAnalysisContext).GetMethod(nameof(RegisterSymbolStartAction)) is not { } registerMethod)
        {
            return static (_, _, _) => { };
        }

        var contextParameter = Parameter(typeof(CompilationStartAnalysisContext));
        var shimmedActionParameter = Parameter(typeof(Action<SymbolStartAnalysisContextWrapper>));
        var symbolKindParameter = Parameter(typeof(SymbolKind));

        var roslynSymbolStartAnalysisContextType = typeof(CompilationStartAnalysisContext).Assembly.GetType("Microsoft.CodeAnalysis.Diagnostics.SymbolStartAnalysisContext");
        var roslynSymbolStartAnalysisActionType = typeof(Action<>).MakeGenericType(roslynSymbolStartAnalysisContextType);
        var roslynSymbolStartAnalysisContextParameter = Parameter(roslynSymbolStartAnalysisContextType);
        var sonarSymbolStartAnalysisContextCtor = typeof(SymbolStartAnalysisContextWrapper).GetConstructors().Single();

        // Action<Roslyn.SymbolStartAnalysisContext> registerAction = roslynSymbolStartAnalysisContextParameter =>
        //    shimmedActionParameter.Invoke(new Sonar.SymbolStartAnalysisContextWrapper(roslynSymbolStartAnalysisContextParameter))
        var registerAction = Lambda(
            delegateType: roslynSymbolStartAnalysisActionType,
            body: Call(shimmedActionParameter, nameof(Action.Invoke), [], New(sonarSymbolStartAnalysisContextCtor, roslynSymbolStartAnalysisContextParameter)),
            parameters: roslynSymbolStartAnalysisContextParameter);

        // (contextParameter, shimmedActionParameter, symbolKindParameter) => contextParameter.RegisterSymbolStartAction(registerAction, symbolKindParameter)
        return Lambda<Action<CompilationStartAnalysisContext, Action<SymbolStartAnalysisContextWrapper>, SymbolKind>>(
            Call(contextParameter, registerMethod, registerAction, symbolKindParameter),
            contextParameter, shimmedActionParameter, symbolKindParameter).Compile();
    }
}
