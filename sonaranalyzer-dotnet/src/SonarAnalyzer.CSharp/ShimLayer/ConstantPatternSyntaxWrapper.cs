// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SonarAnalyzer.ShimLayer.CSharp
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct ConstantPatternSyntaxWrapper : ISyntaxWrapper<CSharpSyntaxNode>
    {
        internal const string WrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.ConstantPatternSyntax";
        private static readonly Type WrappedType;

        private static readonly Func<CSharpSyntaxNode, ExpressionSyntax> ExpressionAccessor;
        private static readonly Func<CSharpSyntaxNode, ExpressionSyntax, CSharpSyntaxNode> WithExpressionAccessor;

        static ConstantPatternSyntaxWrapper()
        {
            WrappedType = WrapperHelper.GetWrappedType(typeof(ConstantPatternSyntaxWrapper));
            ExpressionAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<CSharpSyntaxNode, ExpressionSyntax>(WrappedType, nameof(Expression));
            WithExpressionAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<CSharpSyntaxNode, ExpressionSyntax>(WrappedType, nameof(Expression));
        }

        private ConstantPatternSyntaxWrapper(CSharpSyntaxNode node)
        {
            SyntaxNode = node;
        }

        public CSharpSyntaxNode SyntaxNode { get; }

        public ExpressionSyntax Expression
        {
            get
            {
                return ExpressionAccessor(SyntaxNode);
            }
        }

        public static explicit operator ConstantPatternSyntaxWrapper(PatternSyntaxWrapper node)
        {
            return (ConstantPatternSyntaxWrapper)node.SyntaxNode;
        }

        public static explicit operator ConstantPatternSyntaxWrapper(SyntaxNode node)
        {
            if (node == null)
            {
                return default(ConstantPatternSyntaxWrapper);
            }

            if (!IsInstance(node))
            {
                throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
            }

            return new ConstantPatternSyntaxWrapper((CSharpSyntaxNode)node);
        }

        public static implicit operator PatternSyntaxWrapper(ConstantPatternSyntaxWrapper wrapper)
        {
            return PatternSyntaxWrapper.FromUpcast(wrapper.SyntaxNode);
        }

        public static implicit operator CSharpSyntaxNode(ConstantPatternSyntaxWrapper wrapper)
        {
            return wrapper.SyntaxNode;
        }

        public static bool IsInstance(SyntaxNode node)
        {
            return node != null && LightupHelpers.CanWrapNode(node, WrappedType);
        }

        public ConstantPatternSyntaxWrapper WithExpression(ExpressionSyntax expression)
        {
            return new ConstantPatternSyntaxWrapper(WithExpressionAccessor(SyntaxNode, expression));
        }
    }
}
