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

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    internal abstract class UcfgExpression
    {
        public static UcfgExpression This { get; } = new ThisExpression();
        public static UcfgExpression Unknown { get; } = new UnknownExpression();

        protected UcfgExpression(ISymbol symbol, SyntaxNode node)
        {
            TypeSymbol = symbol.GetSymbolType();
            Node = node;
        }

        public SyntaxNode Node { get; }

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
            public VariableExpression(string variableName, ITypeSymbol returnType)
                : base(returnType, null)
            {
                Expression = new Expression { Var = new Variable { Name = variableName } };
            }

            public VariableExpression(ISymbol symbol, SyntaxNode node)
                : base(symbol, node)
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
            internal const string DefaultValue = "\"\"";

            public ConstantExpression(ISymbol symbol = null)
                : this(DefaultValue, symbol)
            {
            }

            public ConstantExpression(IMethodSymbol methodSymbol)
                : this(methodSymbol.ToUcfgMethodId(), methodSymbol)
            {
            }

            private ConstantExpression(string value, ISymbol symbol)
                : base(symbol, null)
            {
                Expression = new Expression { Const = new Constant { Value = value } };
            }

            public override Expression Expression { get; }
        }

        private class ThisExpression : UcfgExpression
        {
            public ThisExpression()
                : base(null, null)
            {
                Expression = new Expression { This = new This() };
            }

            public override Expression Expression { get; }
        }

        internal class ClassNameExpression : UcfgExpression
        {
            public ClassNameExpression(INamedTypeSymbol namedTypeSymbol)
                : base(namedTypeSymbol, null)
            {
                Expression = new Expression
                {
                    Classname = new ClassName { Classname = namedTypeSymbol.ConstructedFrom.ToDisplayString() }
                };
            }

            public override Expression Expression { get; }
        }

        internal class MemberAccessExpression : UcfgExpression
        {
            public MemberAccessExpression(ISymbol symbol, SyntaxNode node, UcfgExpression target)
                : base(symbol, node)
            {
                Target = target;
            }

            public override Expression Expression { get; } = null;
            public UcfgExpression Target { get; }
        }

        internal class FieldAccessExpression : MemberAccessExpression
        {
            public FieldAccessExpression(IFieldSymbol fieldSymbol, SyntaxNode node, UcfgExpression target)
                : base(fieldSymbol, node, target)
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
                        throw new UcfgException("Expecting target object to be 'This', 'Variable' or 'ClassName' but " +
                            $"got '{Target}'. " +
                            $"Node: {node}" +
                            $"File: {node.GetLocation()?.GetLineSpan().Path ?? "{unknown}"}  " +
                            $"Line: {node.GetLocation()?.GetLineSpan().StartLinePosition}");
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

        internal class ElementAccessExpression : MemberAccessExpression
        {
            public ElementAccessExpression(ISymbol symbol, UcfgExpression target)
                : base(symbol, null, target)
            {
            }
        }

        internal class PropertyAccessExpression : MemberAccessExpression
        {
            public PropertyAccessExpression(IPropertySymbol propertySymbol, SyntaxNode node, UcfgExpression target)
                : base(propertySymbol, node, target)
            {
                GetMethodSymbol = propertySymbol.GetMethod;
                SetMethodSymbol = propertySymbol.SetMethod;
            }

            public IMethodSymbol GetMethodSymbol { get; }
            public IMethodSymbol SetMethodSymbol { get; }
        }

        internal class MethodAccessExpression : MemberAccessExpression
        {
            public MethodAccessExpression(IMethodSymbol methodSymbol, SyntaxNode node, UcfgExpression target)
                : base(methodSymbol, node, target)
            {
            }
        }
    }
}
