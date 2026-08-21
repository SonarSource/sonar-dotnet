// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace StyleCop.Analyzers.Lightup
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public static class SyntaxFactsEx
    {
        private static readonly Func<SyntaxNode, string> TryGetInferredMemberNameAccessor;

        static SyntaxFactsEx()
        {
            string FallbackAccessor(SyntaxNode syntax)
            {
                if (syntax == null)
                {
                    // Unlike an extension method which would throw ArgumentNullException here, the light-up
                    // behavior needs to match behavior of the underlying property.
                    throw new NullReferenceException();
                }

                return null;
            }

            var tryGetInferredMemberNameMethod = typeof(SyntaxFacts).GetTypeInfo().GetDeclaredMethod(nameof(TryGetInferredMemberName));
            if (tryGetInferredMemberNameMethod is object)
            {
                var syntaxParameter = Expression.Parameter(typeof(SyntaxNode), "syntax");
                Expression<Func<SyntaxNode, string>> expression =
                    Expression.Lambda<Func<SyntaxNode, string>>(
                        Expression.Call(tryGetInferredMemberNameMethod, syntaxParameter),
                        syntaxParameter);
                TryGetInferredMemberNameAccessor = expression.Compile();
            }
            else
            {
                TryGetInferredMemberNameAccessor = FallbackAccessor;
            }
        }

        public static string TryGetInferredMemberName(this SyntaxNode syntax)
        {
            return TryGetInferredMemberNameAccessor(syntax);
        }
    }
}
