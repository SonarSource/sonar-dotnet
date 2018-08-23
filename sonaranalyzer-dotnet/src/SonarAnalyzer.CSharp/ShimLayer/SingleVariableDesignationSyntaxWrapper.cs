// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SonarAnalyzer.ShimLayer.CSharp
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    internal struct SingleVariableDesignationSyntaxWrapper : ISyntaxWrapper<CSharpSyntaxNode>
    {
        internal const string WrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.SingleVariableDesignationSyntax";
        private static readonly Type WrappedType;

        private static readonly Func<CSharpSyntaxNode, SyntaxToken> IdentifierAccessor;
        private static readonly Func<CSharpSyntaxNode, SyntaxToken, CSharpSyntaxNode> WithIdentifierAccessor;

        static SingleVariableDesignationSyntaxWrapper()
        {
            WrappedType = WrapperHelper.GetWrappedType(typeof(SingleVariableDesignationSyntaxWrapper));
            IdentifierAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<CSharpSyntaxNode, SyntaxToken>(WrappedType, nameof(Identifier));
            WithIdentifierAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<CSharpSyntaxNode, SyntaxToken>(WrappedType, nameof(Identifier));
        }

        private SingleVariableDesignationSyntaxWrapper(CSharpSyntaxNode node)
        {
            SyntaxNode = node;
        }

        public CSharpSyntaxNode SyntaxNode { get; }

        public SyntaxToken Identifier
        {
            get
            {
                return IdentifierAccessor(SyntaxNode);
            }
        }

        public static explicit operator SingleVariableDesignationSyntaxWrapper(VariableDesignationSyntaxWrapper node)
        {
            return (SingleVariableDesignationSyntaxWrapper)node.SyntaxNode;
        }

        public static explicit operator SingleVariableDesignationSyntaxWrapper(SyntaxNode node)
        {
            if (node == null)
            {
                return default(SingleVariableDesignationSyntaxWrapper);
            }

            if (!IsInstance(node))
            {
                throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
            }

            return new SingleVariableDesignationSyntaxWrapper((CSharpSyntaxNode)node);
        }

        public static implicit operator VariableDesignationSyntaxWrapper(SingleVariableDesignationSyntaxWrapper wrapper)
        {
            return VariableDesignationSyntaxWrapper.FromUpcast(wrapper.SyntaxNode);
        }

        public static implicit operator CSharpSyntaxNode(SingleVariableDesignationSyntaxWrapper wrapper)
        {
            return wrapper.SyntaxNode;
        }

        public static bool IsInstance(SyntaxNode node)
        {
            return node != null && LightupHelpers.CanWrapNode(node, WrappedType);
        }

        public SingleVariableDesignationSyntaxWrapper WithIdentifier(SyntaxToken identifier)
        {
            return new SingleVariableDesignationSyntaxWrapper(WithIdentifierAccessor(SyntaxNode, identifier));
        }
    }
}
