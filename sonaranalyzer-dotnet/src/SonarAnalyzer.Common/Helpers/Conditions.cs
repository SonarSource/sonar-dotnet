/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

namespace SonarAnalyzer.Helpers
{
    public static class Conditions
    {
        public static PropertyAccessCondition ExceptWhen(PropertyAccessCondition condition) =>
            (value) => !condition(value);

        public static InvocationCondition ExceptWhen(InvocationCondition condition) =>
            (value) => !condition(value);

        public static ObjectCreationCondition ExceptWhen(ObjectCreationCondition condition) =>
            (value) => !condition(value);

        public static PropertyAccessCondition And(PropertyAccessCondition condition1, PropertyAccessCondition condition2) =>
            (value) => condition1(value) && condition2(value);

        public static InvocationCondition And(InvocationCondition condition1, InvocationCondition condition2) =>
            (value) => condition1(value) && condition2(value);

        public static ObjectCreationCondition And(ObjectCreationCondition condition1, ObjectCreationCondition condition2) =>
            (value) => condition1(value) && condition2(value);

        public static PropertyAccessCondition Or(PropertyAccessCondition condition1, PropertyAccessCondition condition2) =>
            (value) => condition1(value) || condition2(value);

        public static PropertyAccessCondition Or(PropertyAccessCondition condition1, PropertyAccessCondition condition2, PropertyAccessCondition condition3) =>
            (value) => condition1(value) || condition2(value) || condition3(value);

        public static InvocationCondition Or(InvocationCondition condition1, InvocationCondition condition2) =>
            (value) => condition1(value) || condition2(value);

        public static InvocationCondition Or(InvocationCondition condition1, InvocationCondition condition2, InvocationCondition condition3) =>
            (value) => condition1(value) || condition2(value) || condition3(value);

        public static ObjectCreationCondition Or(ObjectCreationCondition condition1, ObjectCreationCondition condition2) =>
            (value) => condition1(value) || condition2(value);

        public static ObjectCreationCondition Or(ObjectCreationCondition condition1, ObjectCreationCondition condition2, ObjectCreationCondition condition3) =>
            (value) => condition1(value) || condition2(value) || condition3(value);

    }
}
