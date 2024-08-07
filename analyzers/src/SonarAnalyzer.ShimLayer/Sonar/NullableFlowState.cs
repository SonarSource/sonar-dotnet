// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup
{
    public enum NullableFlowState : byte
    {
        /// <summary>
        /// Syntax is not an expression, or was not analyzed.
        /// </summary>
        None = 0,

        /// <summary>
        /// Expression is not null.
        /// </summary>
        NotNull = 1,

        /// <summary>
        /// Expression may be null.
        /// </summary>
        MaybeNull = 2,
    }
}
