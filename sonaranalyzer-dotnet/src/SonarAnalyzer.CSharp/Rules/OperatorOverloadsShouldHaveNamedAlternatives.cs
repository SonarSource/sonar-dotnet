/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class OperatorOverloadsShouldHaveNamedAlternatives : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4069";
        private const string MessageFormat = "Implement alternative method '{0}' for the operator '{1}'.";

        private static readonly Dictionary<string, string> operatorAlternatives = new Dictionary<string, string>
        {
            ["op_Addition"] = "Add",
            ["op_BitwiseAnd"] = "BitwiseAnd",
            ["op_BitwiseOr"] = "BitwiseOr",
            ["op_Division"] = "Divide",
            ["op_ExclusiveOr"] = "Xor",
            ["op_Equality"] = "Equals",
            ["op_Inequality"] = "Equals",
            ["op_GreaterThan"] = "Compare",
            ["op_LessThan"] = "Compare",
            ["op_GreaterThanOrEqual"] = "Compare",
            ["op_LessThanOrEqual"] = "Compare",
            ["op_Decrement"] = "Decrement",
            ["op_Increment"] = "Increment",
            ["op_LeftShift"] = "LeftShift",
            ["op_RightShift"] = "RightShift",
            ["op_LogicalNot"] = "LogicalNot",
            ["op_Modulus"] = "Mod",
            ["op_Multiply"] = "Multiply",
            ["op_OnesComplement"] = "OnesComplement",
            ["op_Subtraction"] = "Subtract",
            ["op_UnaryNegation"] = "Negate",
            ["op_UnaryPlus"] = "Plus"
        };

        private static readonly Dictionary<string, string> operatorNames = new Dictionary<string, string>
        {
            ["op_Addition"] = "+",
            ["op_BitwiseAnd"] = "&",
            ["op_BitwiseOr"] = "|",
            ["op_Division"] = "/",
            ["op_ExclusiveOr"] = "^",
            ["op_Equality"] = "==",
            ["op_Inequality"] = "!=",
            ["op_GreaterThan"] = ">",
            ["op_LessThan"] = "<",
            ["op_GreaterThanOrEqual"] = ">=",
            ["op_LessThanOrEqual"] = "<=",
            ["op_Decrement"] = "--",
            ["op_Increment"] = "++",
            ["op_LeftShift"] = "<<",
            ["op_RightShift"] = ">>",
            ["op_LogicalNot"] = "!",
            ["op_Modulus"] = "%",
            ["op_Multiply"] = "*",
            ["op_OnesComplement"] = "~",
            ["op_Subtraction"] = "-",
            ["op_UnaryNegation"] = "-",
            ["op_UnaryPlus"] = "+"
        };

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var operatorDeclaration = (OperatorDeclarationSyntax)c.Node;
                var operatorSymbol = c.SemanticModel.GetDeclaredSymbol(operatorDeclaration);
                if (operatorSymbol == null ||
                    operatorSymbol.MethodKind != MethodKind.UserDefinedOperator)
                {
                    return;
                }

                var operatorAlternativeMethodName = operatorAlternatives.GetValueOrDefault(operatorSymbol.Name);
                var operatorName = operatorNames.GetValueOrDefault(operatorSymbol.Name);
                if (operatorName != null &&
                    !HasAlternativeMethod(operatorSymbol, operatorAlternativeMethodName))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, operatorDeclaration.OperatorToken.GetLocation(),
                        operatorAlternativeMethodName, operatorName));
                }
            },
            SyntaxKind.OperatorDeclaration);
        }

        private static bool HasAlternativeMethod(IMethodSymbol operatorMethod, string operatorAlternativeMethodName)
        {
            return operatorAlternativeMethodName == null ||
                operatorMethod.ContainingType
                    .GetMembers(operatorAlternativeMethodName)
                    .OfType<IMethodSymbol>()
                    .Any();
        }
    }
}
