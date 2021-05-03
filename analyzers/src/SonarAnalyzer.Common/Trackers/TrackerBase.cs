/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;

namespace SonarAnalyzer.Helpers
{
    public abstract class TrackerBase<TSyntaxKind, TContext>
        where TSyntaxKind : struct
        where TContext : BaseContext
    {
        public readonly struct Condition
        {
            private Func<TContext, bool> Function { get; }

            public Condition(Func<TContext, bool> func) => Function = func;

            public bool Invoke(TContext context) => !(Function is null) && Function(context);

            public static Condition operator |(Condition l, Condition r) => new Condition((c) => l.Function(c) || r.Function(c));
            public static Condition operator &(Condition l, Condition r) => new Condition((c) => l.Function(c) && r.Function(c));
            public static Condition operator !(Condition condition) => new Condition((c) => !condition.Function(c));
        }

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
    }
}
