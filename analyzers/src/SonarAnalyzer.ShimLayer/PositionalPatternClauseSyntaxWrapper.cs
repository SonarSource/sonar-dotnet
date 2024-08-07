// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup
{
    using Microsoft.CodeAnalysis.CSharp;

    public partial struct PositionalPatternClauseSyntaxWrapper : ISyntaxWrapper<CSharpSyntaxNode>
    {
        public PositionalPatternClauseSyntaxWrapper AddSubpatterns(params SubpatternSyntaxWrapper[] items)
        {
            return new PositionalPatternClauseSyntaxWrapper(this.WithSubpatterns(this.Subpatterns.AddRange(items)));
        }
    }
}
