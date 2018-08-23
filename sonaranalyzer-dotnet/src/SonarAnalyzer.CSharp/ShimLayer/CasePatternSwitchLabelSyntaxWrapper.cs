// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SonarAnalyzer.ShimLayer.CSharp
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct CasePatternSwitchLabelSyntaxWrapper : ISyntaxWrapper<SwitchLabelSyntax>
    {
        internal const string WrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.CasePatternSwitchLabelSyntax";
        private static readonly Type WrappedType;

        private static readonly Func<SwitchLabelSyntax, CSharpSyntaxNode> PatternAccessor;
        private static readonly Func<SwitchLabelSyntax, CSharpSyntaxNode> WhenClauseAccessor;
        private static readonly Func<SwitchLabelSyntax, SyntaxToken, SwitchLabelSyntax> WithKeywordAccessor;
        private static readonly Func<SwitchLabelSyntax, SyntaxToken, SwitchLabelSyntax> WithColonTokenAccessor;
        private static readonly Func<SwitchLabelSyntax, CSharpSyntaxNode, SwitchLabelSyntax> WithPatternAccessor;
        private static readonly Func<SwitchLabelSyntax, CSharpSyntaxNode, SwitchLabelSyntax> WithWhenClauseAccessor;

        static CasePatternSwitchLabelSyntaxWrapper()
        {
            WrappedType = WrapperHelper.GetWrappedType(typeof(CasePatternSwitchLabelSyntaxWrapper));
            PatternAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<SwitchLabelSyntax, CSharpSyntaxNode>(WrappedType, nameof(Pattern));
            WhenClauseAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<SwitchLabelSyntax, CSharpSyntaxNode>(WrappedType, nameof(WhenClause));
            WithKeywordAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<SwitchLabelSyntax, SyntaxToken>(WrappedType, nameof(SwitchLabelSyntax.Keyword));
            WithColonTokenAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<SwitchLabelSyntax, SyntaxToken>(WrappedType, nameof(SwitchLabelSyntax.ColonToken));
            WithPatternAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<SwitchLabelSyntax, CSharpSyntaxNode>(WrappedType, nameof(Pattern));
            WithWhenClauseAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<SwitchLabelSyntax, CSharpSyntaxNode>(WrappedType, nameof(WhenClause));
        }

        private CasePatternSwitchLabelSyntaxWrapper(SwitchLabelSyntax node)
        {
            SyntaxNode = node;
        }

        public SwitchLabelSyntax SyntaxNode { get; }

        public PatternSyntaxWrapper Pattern
        {
            get
            {
                return (PatternSyntaxWrapper)PatternAccessor(SyntaxNode);
            }
        }

        public WhenClauseSyntaxWrapper WhenClause
        {
            get
            {
                return (WhenClauseSyntaxWrapper)WhenClauseAccessor(SyntaxNode);
            }
        }

        public static explicit operator CasePatternSwitchLabelSyntaxWrapper(SyntaxNode node)
        {
            if (node == null)
            {
                return default(CasePatternSwitchLabelSyntaxWrapper);
            }

            if (!IsInstance(node))
            {
                throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
            }

            return new CasePatternSwitchLabelSyntaxWrapper((SwitchLabelSyntax)node);
        }

        public static implicit operator SwitchLabelSyntax(CasePatternSwitchLabelSyntaxWrapper wrapper)
        {
            return wrapper.SyntaxNode;
        }

        public static bool IsInstance(SyntaxNode node)
        {
            return node != null && LightupHelpers.CanWrapNode(node, WrappedType);
        }

        public CasePatternSwitchLabelSyntaxWrapper WithKeyword(SyntaxToken keyword)
        {
            return new CasePatternSwitchLabelSyntaxWrapper(WithKeywordAccessor(SyntaxNode, keyword));
        }

        public CasePatternSwitchLabelSyntaxWrapper WithColonToken(SyntaxToken colonToken)
        {
            return new CasePatternSwitchLabelSyntaxWrapper(WithColonTokenAccessor(SyntaxNode, colonToken));
        }

        public CasePatternSwitchLabelSyntaxWrapper WithPattern(PatternSyntaxWrapper pattern)
        {
            return new CasePatternSwitchLabelSyntaxWrapper(WithPatternAccessor(SyntaxNode, pattern));
        }

        public CasePatternSwitchLabelSyntaxWrapper WithWhenClause(WhenClauseSyntaxWrapper whenClause)
        {
            return new CasePatternSwitchLabelSyntaxWrapper(WithWhenClauseAccessor(SyntaxNode, whenClause));
        }
    }
}
