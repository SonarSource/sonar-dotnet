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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed class DeclarationPattern : SimpleProcessor<IDeclarationPatternOperationWrapper>
{
    protected override IDeclarationPatternOperationWrapper Convert(IOperation operation) =>
        IDeclarationPatternOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, IDeclarationPatternOperationWrapper declaration)
    {
        if (declaration.DeclaredSymbol == null)
        {
            return context.State;
        }
        else
        {
            var state = context.Operation.Parent.AsIsPattern() is { } parentIsPattern && parentIsPattern.Value.TrackedSymbol(context.State) is { } sourceSymbol
                ? context.State.SetSymbolValue(declaration.DeclaredSymbol, context.State[sourceSymbol])  // ToDo: MMF-2563 should define relation between tested and declared symbol
                : context.State;
            return declaration.MatchesNull
                ? state     // "... is var ..." should not set NotNull
                : state.SetSymbolConstraint(declaration.DeclaredSymbol, ObjectConstraint.NotNull);
        }
    }
}
