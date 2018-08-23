// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SonarAnalyzer.ShimLayer.CSharp
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct TupleTypeSyntaxWrapper : ISyntaxWrapper<TypeSyntax>
    {
        internal const string WrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.TupleTypeSyntax";
        private static readonly Type WrappedType;

        private static readonly Func<TypeSyntax, SyntaxToken> OpenParenTokenAccessor;
        private static readonly Func<TypeSyntax, SeparatedSyntaxListWrapper<TupleElementSyntaxWrapper>> ElementsAccessor;
        private static readonly Func<TypeSyntax, SyntaxToken> CloseParenTokenAccessor;
        private static readonly Func<TypeSyntax, SyntaxToken, TypeSyntax> WithOpenParenTokenAccessor;
        private static readonly Func<TypeSyntax, SeparatedSyntaxListWrapper<TupleElementSyntaxWrapper>, TypeSyntax> WithElementsAccessor;
        private static readonly Func<TypeSyntax, SyntaxToken, TypeSyntax> WithCloseParenTokenAccessor;

        static TupleTypeSyntaxWrapper()
        {
            WrappedType = WrapperHelper.GetWrappedType(typeof(TupleTypeSyntaxWrapper));
            OpenParenTokenAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<TypeSyntax, SyntaxToken>(WrappedType, nameof(OpenParenToken));
            ElementsAccessor = LightupHelpers.CreateSeparatedSyntaxListPropertyAccessor<TypeSyntax, TupleElementSyntaxWrapper>(WrappedType, nameof(Elements));
            CloseParenTokenAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<TypeSyntax, SyntaxToken>(WrappedType, nameof(CloseParenToken));
            WithOpenParenTokenAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<TypeSyntax, SyntaxToken>(WrappedType, nameof(OpenParenToken));
            WithElementsAccessor = LightupHelpers.CreateSeparatedSyntaxListWithPropertyAccessor<TypeSyntax, TupleElementSyntaxWrapper>(WrappedType, nameof(Elements));
            WithCloseParenTokenAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<TypeSyntax, SyntaxToken>(WrappedType, nameof(CloseParenToken));
        }

        private TupleTypeSyntaxWrapper(TypeSyntax node)
        {
            SyntaxNode = node;
        }

        public TypeSyntax SyntaxNode { get; }

        public SyntaxToken OpenParenToken
        {
            get
            {
                return OpenParenTokenAccessor(SyntaxNode);
            }
        }

        public SeparatedSyntaxListWrapper<TupleElementSyntaxWrapper> Elements
        {
            get
            {
                return ElementsAccessor(SyntaxNode);
            }
        }

        public SyntaxToken CloseParenToken
        {
            get
            {
                return CloseParenTokenAccessor(SyntaxNode);
            }
        }

        public static explicit operator TupleTypeSyntaxWrapper(SyntaxNode node)
        {
            if (node == null)
            {
                return default(TupleTypeSyntaxWrapper);
            }

            if (!IsInstance(node))
            {
                throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
            }

            return new TupleTypeSyntaxWrapper((TypeSyntax)node);
        }

        public static implicit operator TypeSyntax(TupleTypeSyntaxWrapper wrapper)
        {
            return wrapper.SyntaxNode;
        }

        public static bool IsInstance(SyntaxNode node)
        {
            return node != null && LightupHelpers.CanWrapNode(node, WrappedType);
        }

        public TupleTypeSyntaxWrapper AddElements(params TupleElementSyntaxWrapper[] items)
        {
            return new TupleTypeSyntaxWrapper(WithElements(Elements.AddRange(items)));
        }

        public TupleTypeSyntaxWrapper WithOpenParenToken(SyntaxToken openParenToken)
        {
            return new TupleTypeSyntaxWrapper(WithOpenParenTokenAccessor(SyntaxNode, openParenToken));
        }

        public TupleTypeSyntaxWrapper WithElements(SeparatedSyntaxListWrapper<TupleElementSyntaxWrapper> elements)
        {
            return new TupleTypeSyntaxWrapper(WithElementsAccessor(SyntaxNode, elements));
        }

        public TupleTypeSyntaxWrapper WithCloseParenToken(SyntaxToken closeParenToken)
        {
            return new TupleTypeSyntaxWrapper(WithCloseParenTokenAccessor(SyntaxNode, closeParenToken));
        }
    }
}
