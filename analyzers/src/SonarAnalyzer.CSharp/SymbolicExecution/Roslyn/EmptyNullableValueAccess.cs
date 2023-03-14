/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public class EmptyNullableValueAccess : SymbolicRuleCheck
{
    private const string DiagnosticId = "S3655";
    private const string MessageFormat = "'{0}' is null on at least one execution path.";

    internal static readonly DiagnosticDescriptor S3655 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S3655;

    public override bool ShouldExecute()
    {
        var finder = new NullableAccessFinder();
        finder.SafeVisit(Node);
        return finder.HasPotentialNullableValueAccess;
    }

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var operationInstance = context.Operation.Instance;
        if (operationInstance.Kind == OperationKindEx.PropertyReference
            && operationInstance.ToPropertyReference() is var reference
            && reference.Property.Name == nameof(Nullable<int>.Value)
            && reference.Instance.Type.IsNullableValueType()
            && context.HasConstraint(reference.Instance, ObjectConstraint.Null))
        {
            ReportIssue(reference.Instance, reference.Instance.Syntax.ToString());
        }
        else if (operationInstance.Kind == OperationKindEx.Conversion
            && operationInstance.ToConversion() is var conversion
            && conversion.Operand.Type.IsNullableValueType()
            && !conversion.Type.IsNullableValueType()
            && conversion.Type.IsStruct()
            && context.HasConstraint(conversion.Operand, ObjectConstraint.Null))
        {
            ReportIssue(conversion.Operand, conversion.Operand.Syntax.ToString());
        }

        return context.State;
    }

    private sealed class NullableAccessFinder : SafeCSharpSyntaxWalker
    {
        public bool HasPotentialNullableValueAccess { get; private set; }

        public override void Visit(SyntaxNode node)
        {
            if (!HasPotentialNullableValueAccess)
            {
                base.Visit(node);
            }
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (node.NameIs(nameof(Nullable<int>.Value)))
            {
                HasPotentialNullableValueAccess = true;
            }
            else
            {
                base.VisitMemberAccessExpression(node);
            }
        }

        public override void VisitCastExpression(CastExpressionSyntax node) =>
            HasPotentialNullableValueAccess = true;
    }
}
