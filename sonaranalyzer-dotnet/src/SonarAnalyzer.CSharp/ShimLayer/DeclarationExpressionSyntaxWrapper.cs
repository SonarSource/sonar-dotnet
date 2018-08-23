// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SonarAnalyzer.ShimLayer.CSharp
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct DeclarationExpressionSyntaxWrapper : ISyntaxWrapper<ExpressionSyntax>
    {
        internal const string WrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.DeclarationExpressionSyntax";
        private static readonly Type WrappedType;

        private static readonly Func<ExpressionSyntax, TypeSyntax> TypeAccessor;
        private static readonly Func<ExpressionSyntax, CSharpSyntaxNode> DesignationAccessor;
        private static readonly Func<ExpressionSyntax, TypeSyntax, ExpressionSyntax> WithTypeAccessor;
        private static readonly Func<ExpressionSyntax, CSharpSyntaxNode, ExpressionSyntax> WithDesignationAccessor;

        static DeclarationExpressionSyntaxWrapper()
        {
            WrappedType = WrapperHelper.GetWrappedType(typeof(DeclarationExpressionSyntaxWrapper));
            TypeAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<ExpressionSyntax, TypeSyntax>(WrappedType, nameof(Type));
            DesignationAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<ExpressionSyntax, CSharpSyntaxNode>(WrappedType, nameof(Designation));
            WithTypeAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<ExpressionSyntax, TypeSyntax>(WrappedType, nameof(Type));
            WithDesignationAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<ExpressionSyntax, CSharpSyntaxNode>(WrappedType, nameof(Designation));
        }

        private DeclarationExpressionSyntaxWrapper(ExpressionSyntax node)
        {
            SyntaxNode = node;
        }

        public ExpressionSyntax SyntaxNode { get; }

        public TypeSyntax Type
        {
            get
            {
                return TypeAccessor(SyntaxNode);
            }
        }

        public VariableDesignationSyntaxWrapper Designation
        {
            get
            {
                return (VariableDesignationSyntaxWrapper)DesignationAccessor(SyntaxNode);
            }
        }

        public static explicit operator DeclarationExpressionSyntaxWrapper(SyntaxNode node)
        {
            if (node == null)
            {
                return default(DeclarationExpressionSyntaxWrapper);
            }

            if (!IsInstance(node))
            {
                throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
            }

            return new DeclarationExpressionSyntaxWrapper((ExpressionSyntax)node);
        }

        public static implicit operator ExpressionSyntax(DeclarationExpressionSyntaxWrapper wrapper)
        {
            return wrapper.SyntaxNode;
        }

        public static bool IsInstance(SyntaxNode node)
        {
            return node != null && LightupHelpers.CanWrapNode(node, WrappedType);
        }

        public DeclarationExpressionSyntaxWrapper WithType(TypeSyntax type)
        {
            return new DeclarationExpressionSyntaxWrapper(WithTypeAccessor(SyntaxNode, type));
        }

        public DeclarationExpressionSyntaxWrapper WithDesignation(VariableDesignationSyntaxWrapper designation)
        {
            return new DeclarationExpressionSyntaxWrapper(WithDesignationAccessor(SyntaxNode, designation.SyntaxNode));
        }
    }
}
