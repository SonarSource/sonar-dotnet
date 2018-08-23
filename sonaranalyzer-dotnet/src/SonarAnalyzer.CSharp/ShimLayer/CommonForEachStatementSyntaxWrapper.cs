// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SonarAnalyzer.ShimLayer.CSharp
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct CommonForEachStatementSyntaxWrapper : ISyntaxWrapper<StatementSyntax>
    {
        internal const string WrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.CommonForEachStatementSyntax";
        internal const string FallbackWrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax";
        private static readonly Type WrappedType;

        private static readonly Func<StatementSyntax, SyntaxToken> ForEachKeywordAccessor;
        private static readonly Func<StatementSyntax, SyntaxToken> OpenParenTokenAccessor;
        private static readonly Func<StatementSyntax, SyntaxToken> InKeywordAccessor;
        private static readonly Func<StatementSyntax, ExpressionSyntax> ExpressionAccessor;
        private static readonly Func<StatementSyntax, SyntaxToken> CloseParenTokenAccessor;
        private static readonly Func<StatementSyntax, StatementSyntax> StatementAccessor;

        static CommonForEachStatementSyntaxWrapper()
        {
            WrappedType = WrapperHelper.GetWrappedType(typeof(CommonForEachStatementSyntaxWrapper));
            ForEachKeywordAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, SyntaxToken>(WrappedType, nameof(ForEachKeyword));
            OpenParenTokenAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, SyntaxToken>(WrappedType, nameof(OpenParenToken));
            InKeywordAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, SyntaxToken>(WrappedType, nameof(InKeyword));
            ExpressionAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, ExpressionSyntax>(WrappedType, nameof(Expression));
            CloseParenTokenAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, SyntaxToken>(WrappedType, nameof(CloseParenToken));
            StatementAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, StatementSyntax>(WrappedType, nameof(Statement));
        }

        private CommonForEachStatementSyntaxWrapper(StatementSyntax node)
        {
            SyntaxNode = node;
        }

        public StatementSyntax SyntaxNode { get; }

        public SyntaxToken ForEachKeyword
        {
            get
            {
                return ForEachKeywordAccessor(SyntaxNode);
            }
        }

        public SyntaxToken OpenParenToken
        {
            get
            {
                return OpenParenTokenAccessor(SyntaxNode);
            }
        }

        public SyntaxToken InKeyword
        {
            get
            {
                return InKeywordAccessor(SyntaxNode);
            }
        }

        public ExpressionSyntax Expression
        {
            get
            {
                return ExpressionAccessor(SyntaxNode);
            }
        }

        public SyntaxToken CloseParenToken
        {
            get
            {
                return CloseParenTokenAccessor(SyntaxNode);
            }
        }

        public StatementSyntax Statement
        {
            get
            {
                return StatementAccessor(SyntaxNode);
            }
        }

        public static implicit operator CommonForEachStatementSyntaxWrapper(ForEachStatementSyntax node)
        {
            return new CommonForEachStatementSyntaxWrapper(node);
        }

        public static explicit operator CommonForEachStatementSyntaxWrapper(SyntaxNode node)
        {
            if (node == null)
            {
                return default(CommonForEachStatementSyntaxWrapper);
            }

            if (!IsInstance(node))
            {
                throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
            }

            return new CommonForEachStatementSyntaxWrapper((StatementSyntax)node);
        }

        public static implicit operator StatementSyntax(CommonForEachStatementSyntaxWrapper wrapper)
        {
            return wrapper.SyntaxNode;
        }

        public static bool IsInstance(SyntaxNode node)
        {
            return node != null && LightupHelpers.CanWrapNode(node, WrappedType);
        }

        internal static CommonForEachStatementSyntaxWrapper FromUpcast(StatementSyntax node)
        {
            return new CommonForEachStatementSyntaxWrapper(node);
        }
    }
}
