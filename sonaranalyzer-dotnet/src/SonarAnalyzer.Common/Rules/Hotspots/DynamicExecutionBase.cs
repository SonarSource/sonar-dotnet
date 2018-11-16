/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DynamicExecutionBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S1523";
        protected const string MessageFormat = "Make sure that this dynamic injection or execution of code is safe.";

        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            // Special case - Assembly.Load
            InvocationTracker.Track(context,
                InvocationTracker.MatchSimpleNames(
                    new MemberDescriptor(KnownType.System_Reflection_Assembly, "Load"),
                    new MemberDescriptor(KnownType.System_Reflection_Assembly, "LoadFile"),
                    new MemberDescriptor(KnownType.System_Reflection_Assembly, "LoadFrom"),
                    new MemberDescriptor(KnownType.System_Reflection_Assembly, "LoadWithPartialName")),
                InvocationTracker.IsStatic()
                );

            // Special case - Type.GetType() without paramters is ok, but
            // and Type.GetType(...) with parameters is not ok
            InvocationTracker.Track(context,
                InvocationTracker.MatchSimpleNames(
                    new MemberDescriptor(KnownType.System_Type, "GetType")),
                InvocationTracker.IsStatic(),
                InvocationTracker.HasParameters(),
                Conditions.ExceptWhen(
                    Conditions.And(InvocationTracker.FirstParameterIsString,
                    InvocationTracker.FirstParameterIsConstant())));

            // Special case - Activator.CreateXXX
            InvocationTracker.Track(context,
                InvocationTracker.MatchSimpleNames(
                    new MemberDescriptor(KnownType.System_Activator, "CreateComInstanceFrom"),
                    new MemberDescriptor(KnownType.System_Activator, "CreateInstance"),
                    new MemberDescriptor(KnownType.System_Activator, "CreateInstanceFrom")),
                InvocationTracker.IsStatic(),
                InvocationTracker.HasParameters(),
                Conditions.ExceptWhen(InvocationTracker.FirstParameterIsOfType(KnownType.System_Type)));

            // All other method invocations
            InvocationTracker.Track(context,
                Conditions.ExceptWhen(InvocationTracker.IsTypeOfExpression()),
                InvocationTracker.MatchSimpleNames(
                    // Methods on assembly that are safe to call with constants
                    new MemberDescriptor(KnownType.System_Reflection_Assembly, "GetType"),
                    new MemberDescriptor(KnownType.System_Reflection_Assembly, "GetTypes"),
                    new MemberDescriptor(KnownType.System_Reflection_Assembly, "GetModule"),
                    new MemberDescriptor(KnownType.System_Reflection_Assembly, "GetLoadedModules"),
                    new MemberDescriptor(KnownType.System_Reflection_Assembly, "GetModules"),
                    new MemberDescriptor(KnownType.System_Reflection_Assembly, "CreateInstance"),
                    new MemberDescriptor(KnownType.System_Reflection_Assembly, "GetExportedTypes"),

                    new MemberDescriptor(KnownType.System_Type, "GetInterface"),
                    new MemberDescriptor(KnownType.System_Type, "GetNestedType"),
                    new MemberDescriptor(KnownType.System_Type, "GetNestedTypes"),
                    new MemberDescriptor(KnownType.System_Type, "GetInterfaces"),
                    new MemberDescriptor(KnownType.System_Type, "GetMethod"),
                    new MemberDescriptor(KnownType.System_Type, "GetField"),
                    new MemberDescriptor(KnownType.System_Type, "GetProperty"),
                    new MemberDescriptor(KnownType.System_Type, "GetMember"),

                    new MemberDescriptor(KnownType.System_Type, "GetMethods"),
                    new MemberDescriptor(KnownType.System_Type, "GetFields"),
                    new MemberDescriptor(KnownType.System_Type, "GetProperties"),
                    new MemberDescriptor(KnownType.System_Type, "GetMembers"),

                    new MemberDescriptor(KnownType.System_Type, "GetDefaultMembers"),
                    new MemberDescriptor(KnownType.System_Type, "InvokeMember")),
                Conditions.ExceptWhen(
                    Conditions.And(InvocationTracker.FirstParameterIsString,
                    InvocationTracker.FirstParameterIsConstant())));
        }
    }
}
