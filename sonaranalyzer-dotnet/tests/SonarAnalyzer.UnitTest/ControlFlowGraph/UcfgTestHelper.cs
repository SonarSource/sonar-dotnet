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

extern alias csharp;

using System;
using System.Linq;
using csharp::SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.UnitTest.ControlFlowGraph
{
    internal static class UcfgTestHelper
    {
        public static string ToTestString(this Variable variable) =>
            variable.Name;

        public static string ToTestString(this This @this) =>
            "this";

        public static string ToTestString(this Constant constant) =>
            constant.Value == UcfgExpressionService.DefaultConstantValue ? "const" : constant.Value;

        public static string ToTestString(this ClassName className) =>
            className.Classname;

        public static string ToTestString(this FieldAccess fieldAccess) =>
            $"{fieldAccess.This?.ToTestString() ?? fieldAccess.Object?.ToTestString() ?? fieldAccess.Classname.ToTestString()}" +
            $".{fieldAccess.Field.ToTestString()}";

        public static string ToTestString(this Expression expression) =>
            expression.Classname?.ToTestString()
            ?? expression.Const?.ToTestString()
            ?? expression.FieldAccess?.ToTestString()
            ?? expression.This?.ToTestString()
            ?? expression.Var?.ToTestString()
            ?? throw new ArgumentOutOfRangeException(nameof(expression));

        public static string ToTestString(this AssignCall assignCall)
        {
            var targetCall = assignCall.Variable?.ToTestString()
                ?? assignCall.FieldAccess?.ToTestString()
                ?? throw new ArgumentOutOfRangeException(nameof(assignCall));

            return $"{targetCall} := {assignCall.MethodId} [ {string.Join(" ", assignCall.Args.Select(ToTestString))} ]";
        }

        public static string ToTestString(this NewObject newObject)
        {
            var targetCall = newObject.Variable?.ToTestString()
                ?? newObject.FieldAccess?.ToTestString()
                ?? throw new ArgumentOutOfRangeException(nameof(newObject));

            return $"{targetCall} := new {newObject.Type}";
        }

        public static string ToTestString(this Instruction instruction) =>
            instruction.Assigncall?.ToTestString()
            ?? instruction.NewObject?.ToTestString()
            ?? throw new ArgumentOutOfRangeException(nameof(instruction));
    }
}
