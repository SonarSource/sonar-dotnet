// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SonarAnalyzer.ShimLayer.CSharp
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public struct VariableDesignationSyntaxWrapper : ISyntaxWrapper<CSharpSyntaxNode>
    {
        public const string WrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.VariableDesignationSyntax";
        private static readonly Type WrappedType;

        private readonly CSharpSyntaxNode node;

        static VariableDesignationSyntaxWrapper()
        {
            WrappedType = WrapperHelper.GetWrappedType(typeof(VariableDesignationSyntaxWrapper));
        }

        private VariableDesignationSyntaxWrapper(CSharpSyntaxNode node)
        {
            this.node = node;
        }

        public CSharpSyntaxNode SyntaxNode => this.node;

        public static explicit operator VariableDesignationSyntaxWrapper(SyntaxNode node)
        {
            if (node == null)
            {
                return default;
            }

            if (!IsInstance(node))
            {
                throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
            }

            return new VariableDesignationSyntaxWrapper((CSharpSyntaxNode)node);
        }

        public static implicit operator CSharpSyntaxNode(VariableDesignationSyntaxWrapper wrapper)
        {
            return wrapper.node;
        }

        public static bool IsInstance(SyntaxNode node)
        {
            return node != null && LightupHelpers.CanWrapNode(node, WrappedType);
        }

        public static VariableDesignationSyntaxWrapper FromUpcast(CSharpSyntaxNode node)
        {
            return new VariableDesignationSyntaxWrapper(node);
        }
    }
}
