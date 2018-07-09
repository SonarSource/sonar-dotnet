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

using System;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    internal abstract class UcfgExpression
    {
        public static UcfgExpression This { get; } = new ThisExpression();
        public static UcfgExpression Constant { get; } = new ConstantExpression();
        public static UcfgExpression Unknown { get; } = new UnknownExpression();

        protected UcfgExpression(ISymbol symbol)
        {
            TypeSymbol = symbol.GetSymbolType();
        }

        public abstract Expression Expression { get; }
        public ITypeSymbol TypeSymbol { get; }

        public virtual void ApplyAsTarget(Instruction instruction) =>
            throw new UcfgException($"Type {GetType().Name} could not be applied as target of the instruction.");

        private class UnknownExpression : ConstantExpression
        {
            public UnknownExpression()
                : base(null)
            {
            }
        }

        internal class VariableExpression : UcfgExpression
        {
            public VariableExpression(string variableName, bool isString)
                : base(null)
            {
                Expression = new Expression { Var = new Variable { Name = variableName } };
            }

            public VariableExpression(string variableName, ITypeSymbol returnType)
                : base(returnType)
            {
                Expression = new Expression { Var = new Variable { Name = variableName } };
            }

            public VariableExpression(ISymbol symbol)
                : base(symbol)
            {
                Expression = new Expression { Var = new Variable { Name = symbol.Name } };
            }

            public override Expression Expression { get; }

            public override void ApplyAsTarget(Instruction instruction)
            {
                if (instruction.Assigncall != null)
                {
                    instruction.Assigncall.Variable = Expression.Var;
                }
                else if (instruction.NewObject != null)
                {
                    instruction.NewObject.Variable = Expression.Var;
                }
                else
                {
                    throw new UcfgException();
                }
            }
        }

        internal class ConstantExpression : UcfgExpression
        {
            private const string value = "\"\"";

            public ConstantExpression(ISymbol symbol = null)
                : base(symbol)
            {
                Expression = new Expression { Const = new Constant { Value = value } };
            }

            public override Expression Expression { get; }
        }

        private class ThisExpression : UcfgExpression
        {
            public ThisExpression()
                : base(null)
            {
                Expression = new Expression { This = new This() };
            }

            public override Expression Expression { get; }
        }

        internal class ClassNameExpression : UcfgExpression
        {
            public ClassNameExpression(INamedTypeSymbol namedTypeSymbol)
                : base(namedTypeSymbol)
            {
                Expression = new Expression { Classname = new ClassName { Classname = UcfgIdentifier.CreateTypeId(namedTypeSymbol).ToString() } };
            }

            public override Expression Expression { get; }
        }

        internal class MemberAccessExpression : UcfgExpression
        {
            public MemberAccessExpression(ISymbol symbol, UcfgExpression target)
                : base(symbol)
            {
                Target = target;
            }

            public override Expression Expression { get; } = null;
            public UcfgExpression Target { get; }
        }

        internal class FieldAccessExpression : MemberAccessExpression
        {
            public FieldAccessExpression(IFieldSymbol fieldSymbol, UcfgExpression target)
                : base(fieldSymbol, target)
            {
                var fieldAccess = new FieldAccess { Field = new Variable { Name = fieldSymbol.Name } };

                switch (Target)
                {
                    case ThisExpression thisExpression:
                        fieldAccess.This = Target.Expression.This;
                        break;

                    case VariableExpression variableExpression:
                        fieldAccess.Object = Target.Expression.Var;
                        break;

                    case ClassNameExpression classNameExpression:
                        fieldAccess.Classname = Target.Expression.Classname;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), $"Expecting target object to be 'This', " +
                            $"'Variable' or 'ClassName' but got '{Target}'.");
                }

                Expression = new Expression { FieldAccess = fieldAccess };
            }

            public override Expression Expression { get; }

            public override void ApplyAsTarget(Instruction instruction)
            {
                if (instruction.Assigncall != null)
                {
                    instruction.Assigncall.FieldAccess = Expression.FieldAccess;
                }
                else if (instruction.NewObject != null)
                {
                    instruction.NewObject.FieldAccess = Expression.FieldAccess;
                }
                else
                {
                    throw new UcfgException();
                }
            }
        }

        internal class PropertyAccessExpression : MemberAccessExpression
        {
            public PropertyAccessExpression(IPropertySymbol propertySymbol, UcfgExpression target)
                : base(propertySymbol, target)
            {
                GetMethodSymbol = propertySymbol.GetMethod;
                SetMethodSymbol = propertySymbol.SetMethod;
            }

            public IMethodSymbol GetMethodSymbol { get; }
            public IMethodSymbol SetMethodSymbol { get; }
        }

        internal class MethodAccessExpression : MemberAccessExpression
        {
            public MethodAccessExpression(IMethodSymbol methodSymbol, UcfgExpression target)
                : base(methodSymbol, target)
            {
            }
        }
    }
}
