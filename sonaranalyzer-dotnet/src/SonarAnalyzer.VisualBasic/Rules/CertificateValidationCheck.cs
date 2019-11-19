/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.Common;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class CertificateValidationCheck : CertificateValidationCheckBase<MethodBlockSyntax, ArgumentSyntax, ExpressionSyntax, IdentifierNameSyntax, AssignmentStatementSyntax, InvocationExpressionSyntax, ParameterSyntax, VariableDeclaratorSyntax, LambdaExpressionSyntax>
    {
        public CertificateValidationCheck() : base(RspecStrings.ResourceManager) { }

        protected override void Initialize(SonarAnalysisContext context)
        {
            //Handling of = syntax
            context.RegisterSyntaxNodeActionInNonGenerated(c => CheckAssignmentSyntax(c), SyntaxKind.SimpleAssignmentStatement);

            //Handling of constructor parameter syntax (SslStream)
            context.RegisterSyntaxNodeActionInNonGenerated(c => CheckConstructorParameterSyntax(c), SyntaxKind.ObjectCreationExpression);
        }

        internal override AbstractMethodParameterLookup<ArgumentSyntax> CreateParameterLookup(SyntaxNode argumentListNode, IMethodSymbol method)
        {
            throw new System.NotImplementedException();
        }

        protected override Location ArgumentLocation(ArgumentSyntax argument)
        {
            //For Lambda expression extract location of the parentheses only to separate them from secondary location of "true"
            var expression = argument.GetExpression();
            if (expression == null)
            {
                return argument.GetLocation();
            }
            return ((expression is LambdaExpressionSyntax lambda) ? (SyntaxNode)lambda.SubOrFunctionHeader.ParameterList : argument).GetLocation();
        }

        protected override ExpressionSyntax ArgumentExpression(ArgumentSyntax argument)
        {
            return argument.GetExpression();
        }

        protected override void SplitAssignment(AssignmentStatementSyntax assignment, out IdentifierNameSyntax leftIdentifier, out ExpressionSyntax right)
        {
            leftIdentifier = assignment.Left.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().LastOrDefault();
            right = assignment.Right;
        }

        protected override IEqualityComparer<ExpressionSyntax> CreateNodeEqualityComparer()
        {
            return new Helpers.VisualBasic.VisualBasicSyntaxNodeEqualityComparer<ExpressionSyntax>();
        }



        protected override ExpressionSyntax[] FindReturnExpressions(SyntaxNode block)
        {
            //FIXME: VB.NET vs. return by assign to function name
            //Najit spravnou variable
            //Najit vsechny assignmenty, ktere maji Exit Function nebo jen ta posledni
            //Loops/For

            return block.DescendantNodes().OfType<ReturnStatementSyntax>().Select(x => x.Expression).ToArray();
        }

        protected override bool IsTrueLiteral(ExpressionSyntax expression)
        {
            return expression.Kind() == SyntaxKind.TrueLiteralExpression;
        }
        
        protected override string IdentifierText(SyntaxNode node)
        {
            switch (node)
            {
                case IdentifierNameSyntax ident:
                    return ident.Identifier.ValueText;
                case ParameterSyntax param:
                    return param.Identifier.Identifier.ValueText;
                case VariableDeclaratorSyntax variable:
                    return variable.Names.Count == 1 ? variable.Names[0].Identifier.ValueText : null;   //FIXME: Revize, jestli je ta synatxe na Dim a, b, c As Integer, takto nebude fungovat spravne
                default:
                    return null;
            }
        }

        protected override ExpressionSyntax VariableInitializer(VariableDeclaratorSyntax variable)
        {
            return variable.Initializer?.Value;
        }

        //protected override SyntaxNode InvocationArgumentList(InvocationExpressionSyntax invocation)
        //{
        //    return invocation.ArgumentList;
        //}

        protected override ImmutableArray<Location> LambdaLocations(LambdaExpressionSyntax lambda)
        {
            System.Diagnostics.Debugger.Break();

            //FIXME: Doresit
            

            //if ((lambda.Body as LiteralExpressionSyntax)?.Kind() == SyntaxKind.TrueLiteralExpression)
            //{
            //    return new[] { lambda.Body.GetLocation() }.ToImmutableArray();   //Code was found guilty for lambda (...) => true
            //}
            //else if (lambda.Body is BlockSyntax block)
            //{
            //    return BlockLocations(block).ToImmutableArray();
            //}
            //else
            //{
                return ImmutableArray<Location>.Empty;
            //}
        }

        protected override ImmutableArray<SyntaxNode> LocalVariableScopeStatements(VariableDeclaratorSyntax variable)
        {
            //FIXME: Top declaration a pak jeho parent

            throw new System.NotImplementedException();

        }

        protected override SyntaxNode FindRootClassOrModule(SyntaxNode node)
        {
            SyntaxNode current, candidate;
            current = node.FirstAncestorOrSelf<ModuleBlockSyntax>();
            if (current != null)
            {
                return current; //Modules can't be nested. If there's one, it's the Root
            }
            current = node.FirstAncestorOrSelf<ClassBlockSyntax>();
            while (current != null && (candidate = current.Parent?.FirstAncestorOrSelf<ClassBlockSyntax>()) != null)  //Search for parent of nested class
            {
                current = candidate;
            }
            return current;
        }

    }
}

