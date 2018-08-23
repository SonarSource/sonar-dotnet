// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SonarAnalyzer.ShimLayer.CSharp
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct LocalFunctionStatementSyntaxWrapper : ISyntaxWrapper<StatementSyntax>
    {
        internal const string WrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax";
        private static readonly Type WrappedType;

        private static readonly Func<StatementSyntax, SyntaxTokenList> ModifiersAccessor;
        private static readonly Func<StatementSyntax, TypeSyntax> ReturnTypeAccessor;
        private static readonly Func<StatementSyntax, SyntaxToken> IdentifierAccessor;
        private static readonly Func<StatementSyntax, TypeParameterListSyntax> TypeParameterListAccessor;
        private static readonly Func<StatementSyntax, ParameterListSyntax> ParameterListAccessor;
        private static readonly Func<StatementSyntax, SyntaxList<TypeParameterConstraintClauseSyntax>> ConstraintClausesAccessor;
        private static readonly Func<StatementSyntax, BlockSyntax> BodyAccessor;
        private static readonly Func<StatementSyntax, ArrowExpressionClauseSyntax> ExpressionBodyAccessor;
        private static readonly Func<StatementSyntax, SyntaxToken> SemicolonTokenAccessor;
        private static readonly Func<StatementSyntax, SyntaxTokenList, StatementSyntax> WithModifiersAccessor;
        private static readonly Func<StatementSyntax, TypeSyntax, StatementSyntax> WithReturnTypeAccessor;
        private static readonly Func<StatementSyntax, SyntaxToken, StatementSyntax> WithIdentifierAccessor;
        private static readonly Func<StatementSyntax, TypeParameterListSyntax, StatementSyntax> WithTypeParameterListAccessor;
        private static readonly Func<StatementSyntax, ParameterListSyntax, StatementSyntax> WithParameterListAccessor;
        private static readonly Func<StatementSyntax, SyntaxList<TypeParameterConstraintClauseSyntax>, StatementSyntax> WithConstraintClausesAccessor;
        private static readonly Func<StatementSyntax, BlockSyntax, StatementSyntax> WithBodyAccessor;
        private static readonly Func<StatementSyntax, ArrowExpressionClauseSyntax, StatementSyntax> WithExpressionBodyAccessor;
        private static readonly Func<StatementSyntax, SyntaxToken, StatementSyntax> WithSemicolonTokenAccessor;

        static LocalFunctionStatementSyntaxWrapper()
        {
            WrappedType = WrapperHelper.GetWrappedType(typeof(LocalFunctionStatementSyntaxWrapper));
            ModifiersAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, SyntaxTokenList>(WrappedType, nameof(Modifiers));
            ReturnTypeAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, TypeSyntax>(WrappedType, nameof(ReturnType));
            IdentifierAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, SyntaxToken>(WrappedType, nameof(Identifier));
            TypeParameterListAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, TypeParameterListSyntax>(WrappedType, nameof(TypeParameterList));
            ParameterListAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, ParameterListSyntax>(WrappedType, nameof(ParameterList));
            ConstraintClausesAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, SyntaxList<TypeParameterConstraintClauseSyntax>>(WrappedType, nameof(ConstraintClauses));
            BodyAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, BlockSyntax>(WrappedType, nameof(Body));
            ExpressionBodyAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, ArrowExpressionClauseSyntax>(WrappedType, nameof(ExpressionBody));
            SemicolonTokenAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, SyntaxToken>(WrappedType, nameof(SemicolonToken));
            WithModifiersAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, SyntaxTokenList>(WrappedType, nameof(Modifiers));
            WithReturnTypeAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, TypeSyntax>(WrappedType, nameof(ReturnType));
            WithIdentifierAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, SyntaxToken>(WrappedType, nameof(Identifier));
            WithTypeParameterListAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, TypeParameterListSyntax>(WrappedType, nameof(TypeParameterList));
            WithParameterListAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, ParameterListSyntax>(WrappedType, nameof(ParameterList));
            WithConstraintClausesAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, SyntaxList<TypeParameterConstraintClauseSyntax>>(WrappedType, nameof(ConstraintClauses));
            WithBodyAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, BlockSyntax>(WrappedType, nameof(Body));
            WithExpressionBodyAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, ArrowExpressionClauseSyntax>(WrappedType, nameof(ExpressionBody));
            WithSemicolonTokenAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, SyntaxToken>(WrappedType, nameof(SemicolonToken));
        }

        private LocalFunctionStatementSyntaxWrapper(StatementSyntax node)
        {
            SyntaxNode = node;
        }

        public StatementSyntax SyntaxNode { get; }

        public SyntaxTokenList Modifiers
        {
            get
            {
                return ModifiersAccessor(SyntaxNode);
            }
        }

        public TypeSyntax ReturnType
        {
            get
            {
                return ReturnTypeAccessor(SyntaxNode);
            }
        }

        public SyntaxToken Identifier
        {
            get
            {
                return IdentifierAccessor(SyntaxNode);
            }
        }

        public TypeParameterListSyntax TypeParameterList
        {
            get
            {
                return TypeParameterListAccessor(SyntaxNode);
            }
        }

        public ParameterListSyntax ParameterList
        {
            get
            {
                return ParameterListAccessor(SyntaxNode);
            }
        }

        public SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses
        {
            get
            {
                return ConstraintClausesAccessor(SyntaxNode);
            }
        }

        public BlockSyntax Body
        {
            get
            {
                return BodyAccessor(SyntaxNode);
            }
        }

        public ArrowExpressionClauseSyntax ExpressionBody
        {
            get
            {
                return ExpressionBodyAccessor(SyntaxNode);
            }
        }

        public SyntaxToken SemicolonToken
        {
            get
            {
                return SemicolonTokenAccessor(SyntaxNode);
            }
        }

        public static explicit operator LocalFunctionStatementSyntaxWrapper(SyntaxNode node)
        {
            if (node == null)
            {
                return default(LocalFunctionStatementSyntaxWrapper);
            }

            if (!IsInstance(node))
            {
                throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
            }

            return new LocalFunctionStatementSyntaxWrapper((StatementSyntax)node);
        }

        public static implicit operator StatementSyntax(LocalFunctionStatementSyntaxWrapper wrapper)
        {
            return wrapper.SyntaxNode;
        }

        public static bool IsInstance(SyntaxNode node)
        {
            return node != null && LightupHelpers.CanWrapNode(node, WrappedType);
        }

        public LocalFunctionStatementSyntaxWrapper WithModifiers(SyntaxTokenList modifiers)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithModifiersAccessor(SyntaxNode, modifiers));
        }

        public LocalFunctionStatementSyntaxWrapper WithReturnType(TypeSyntax returnType)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithReturnTypeAccessor(SyntaxNode, returnType));
        }

        public LocalFunctionStatementSyntaxWrapper WithIdentifier(SyntaxToken identifier)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithIdentifierAccessor(SyntaxNode, identifier));
        }

        public LocalFunctionStatementSyntaxWrapper WithTypeParameterList(TypeParameterListSyntax typeParameterList)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithTypeParameterListAccessor(SyntaxNode, typeParameterList));
        }

        public LocalFunctionStatementSyntaxWrapper WithParameterList(ParameterListSyntax parameterList)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithParameterListAccessor(SyntaxNode, parameterList));
        }

        public LocalFunctionStatementSyntaxWrapper WithConstraintClauses(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithConstraintClausesAccessor(SyntaxNode, constraintClauses));
        }

        public LocalFunctionStatementSyntaxWrapper WithBody(BlockSyntax body)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithBodyAccessor(SyntaxNode, body));
        }

        public LocalFunctionStatementSyntaxWrapper WithExpressionBody(ArrowExpressionClauseSyntax expressionBody)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithExpressionBodyAccessor(SyntaxNode, expressionBody));
        }

        public LocalFunctionStatementSyntaxWrapper WithSemicolonToken(SyntaxToken semicolonToken)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithSemicolonTokenAccessor(SyntaxNode, semicolonToken));
        }

        public LocalFunctionStatementSyntaxWrapper AddModifiers(params SyntaxToken[] items)
        {
            return WithModifiers(Modifiers.AddRange(items));
        }

        public LocalFunctionStatementSyntaxWrapper AddTypeParameterListParameters(params TypeParameterSyntax[] items)
        {
            var typeParameterList = TypeParameterList ?? SyntaxFactory.TypeParameterList();
            return WithTypeParameterList(typeParameterList.WithParameters(typeParameterList.Parameters.AddRange(items)));
        }

        public LocalFunctionStatementSyntaxWrapper AddParameterListParameters(params ParameterSyntax[] items)
        {
            return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
        }

        public LocalFunctionStatementSyntaxWrapper AddConstraintClauses(params TypeParameterConstraintClauseSyntax[] items)
        {
            return WithConstraintClauses(ConstraintClauses.AddRange(items));
        }

        public LocalFunctionStatementSyntaxWrapper AddBodyStatements(params StatementSyntax[] items)
        {
            var body = Body ?? SyntaxFactory.Block();
            return WithBody(body.WithStatements(body.Statements.AddRange(items)));
        }
    }
}
