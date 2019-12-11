// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SonarAnalyzer.ShimLayer.CSharp
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a light-up wrapper for a type derived from a known back syntax kind <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The base syntax kind which is exposed in the referenced API.</typeparam>
    public interface ISyntaxWrapper<T>
        where T : SyntaxNode
    {
        /// <summary>
        /// Gets the wrapped syntax node.
        /// </summary>
        /// <value>
        /// The wrapped syntax node.
        /// </value>
        T SyntaxNode
        {
            get;
        }
    }
}
