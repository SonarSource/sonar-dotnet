// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup
{
    public readonly record struct NullabilityInfo
    {
        /// <summary>
        /// The nullable annotation of the expression represented by the syntax node. This represents
        /// the nullability of expressions that can be assigned to this expression, if this expression
        /// can be used as an lvalue.
        /// </summary>
        public NullableAnnotation Annotation { get; }

        /// <summary>
        /// The nullable flow state of the expression represented by the syntax node. This represents
        /// the compiler's understanding of whether this expression can currently contain null, if
        /// this expression can be used as an rvalue.
        /// </summary>
        public NullableFlowState FlowState { get; }

        public NullabilityInfo(NullableAnnotation annotation, NullableFlowState flowState)
        {
            Annotation = annotation;
            FlowState = flowState;
        }
    }
}
