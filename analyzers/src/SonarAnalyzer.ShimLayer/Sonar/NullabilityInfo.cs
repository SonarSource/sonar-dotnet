/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace StyleCop.Analyzers.Lightup;

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
