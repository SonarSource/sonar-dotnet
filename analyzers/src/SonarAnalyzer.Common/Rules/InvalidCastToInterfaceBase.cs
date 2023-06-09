/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules;

public abstract class InvalidCastToInterfaceBase<TSyntaxKind> : SonarDiagnosticAnalyzer
    where TSyntaxKind : struct
{
    protected const string DiagnosticId = "S1944";
    protected const string MessageFormat = "{0}";

    // Once we remove the old SE engine, this should inherit SonarDiagnosticAnalyzer<TSyntaxKind> to delegate and simplify this scaffolding.
    public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
    protected abstract DiagnosticDescriptor Rule { get; }
}
