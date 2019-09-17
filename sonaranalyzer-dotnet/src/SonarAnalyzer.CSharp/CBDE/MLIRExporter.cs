using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.ControlFlowGraph;
using SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer
{
    public static class SyntaxNodeExtension
    {
        public static string Dump(this SyntaxNode node)
        {
            return Regex.Replace(node.ToString(), @"\t|\n|\r", " ");
        }
    }

    public class MLIRExporter
    {
        public MLIRExporter(TextWriter w, SemanticModel model, bool withLoc)
        {
            writer = w;
            semanticModel = model;
            exportsLocations = withLoc;
        }

        public void ExportFunction(MethodDeclarationSyntax method)
        {
            if (method.Body == null)
            {
                return;
            }

            if (IsTooClomplexForMLIROrTheCFG(method))
            {
                writer.WriteLine($"// Skipping function {method.Identifier.ValueText}{GetAnonymousArgumentsString(method)}, it contains poisonous unsupported syntaxes");
                writer.WriteLine();
                return;
            }
            blockCounter = 0;
            blockMap.Clear();

            opCounter = 0;
            opMap.Clear();

            var returnType = HasNoReturn(method) ?
                "()" :
                MLIRType(method.ReturnType);
            writer.WriteLine($"func @{GetMangling(method)}{GetAnonymousArgumentsString(method)} -> {returnType} {GetLocation(method)} {{");
            CreateEntryBlock(method);

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            foreach (var block in cfg.Blocks)
            {
                ExportBlock(block, method, returnType);
            }
            writer.WriteLine("}");
        }

        private string GetMangling(MethodDeclarationSyntax method)
        {
            var prettyName = EncodeName(semanticModel.GetDeclaredSymbol(method).ToDisplayString());
            var sb = new StringBuilder(prettyName.Length);
            foreach(char c in prettyName)
            {
                if (char.IsLetterOrDigit(c) || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
                else if (char.IsSeparator(c))
                {
                    // Ignore it
                }
                else if(c == ',')
                {
                    sb.Append('.');
                }
                else
                {
                    sb.Append('$');
                }
            }
            return sb.ToString();
        }
        private bool IsTooClomplexForMLIROrTheCFG(MethodDeclarationSyntax method)
        {
            var symbol = semanticModel.GetDeclaredSymbol(method);
            if (symbol.IsAsync)
            {
                return true;
            }
            return method.DescendantNodes().Any(n =>
            n.IsKind(SyntaxKind.ForEachStatement) ||
            n.IsKind(SyntaxKind.AwaitExpression) ||
            n.IsKind(SyntaxKind.YieldReturnStatement) ||
            n.IsKind(SyntaxKind.YieldBreakStatement) ||
            n.IsKind(SyntaxKind.TryStatement) ||
            n.IsKind(SyntaxKind.UsingStatement) ||
            n.IsKind(SyntaxKind.LogicalAndExpression) ||
            n.IsKind(SyntaxKind.LogicalOrExpression) ||
            n.IsKind(SyntaxKind.ConditionalExpression) ||
            n.IsKind(SyntaxKind.ConditionalAccessExpression) ||
            n.IsKind(SyntaxKind.CoalesceExpression) ||
            n.IsKind(SyntaxKind.SwitchStatement) ||
            n.IsKind(SyntaxKind.ParenthesizedLambdaExpression) ||
            n.IsKind(SyntaxKind.SimpleLambdaExpression) ||
            n.IsKind(SyntaxKind.FixedStatement) ||
            n.IsKind(SyntaxKind.CheckedStatement) ||
            n.IsKind(SyntaxKind.CheckedExpression) ||
            n.IsKind(SyntaxKind.UncheckedExpression) ||
            n.IsKind(SyntaxKind.UncheckedStatement) ||
            n.IsKind(SyntaxKind.GotoStatement) ||
            n.IsKind(SyntaxKind.AnonymousMethodExpression) ||
            n.IsKind(SyntaxKindEx.UnderscoreToken) ||
            n.IsKind(SyntaxKindEx.IsPatternExpression) ||
            n.IsKind(SyntaxKindEx.DefaultLiteralExpression) ||
            n.IsKind(SyntaxKindEx.LocalFunctionStatement) ||
            n.IsKind(SyntaxKindEx.TupleType) ||
            n.IsKind(SyntaxKindEx.TupleElement) ||
            n.IsKind(SyntaxKindEx.TupleExpression) ||
            n.IsKind(SyntaxKindEx.SingleVariableDesignation) ||
            n.IsKind(SyntaxKindEx.ParenthesizedVariableDesignation) ||
            n.IsKind(SyntaxKindEx.ForEachVariableStatement) ||
            n.IsKind(SyntaxKindEx.DeclarationPattern) ||
            n.IsKind(SyntaxKindEx.ConstantPattern) ||
            n.IsKind(SyntaxKindEx.CasePatternSwitchLabel) ||
            n.IsKind(SyntaxKindEx.WhenClause) ||
            n.IsKind(SyntaxKindEx.DiscardDesignation) ||
            n.IsKind(SyntaxKindEx.DeclarationExpression) ||
            n.IsKind(SyntaxKindEx.RefExpression) ||
            n.IsKind(SyntaxKindEx.RefType) ||
            n.IsKind(SyntaxKindEx.ThrowExpression)
            );
        }

        private void CreateEntryBlock(MethodDeclarationSyntax method)
        {
            writer.WriteLine($"^entry {GetArgumentsString(method)}:");
            foreach (var param in method.ParameterList.Parameters)
            {
                if(string.IsNullOrEmpty(param.Identifier.ValueText))
                {
                    // An unnamed parameter cannot be used inside the function
                    continue;
                }
                var id = OpId(param);
                writer.WriteLine($"%{id} = cbde.alloca {MLIRType(param)} {GetLocation(param)}");
                writer.WriteLine($"cbde.store %{EncodeName(param.Identifier.ValueText)}, %{id} : memref<{MLIRType(param)}> {GetLocation(param)}");
            }
            writer.WriteLine("br ^0");
            writer.WriteLine();
        }

        private bool HasNoReturn(MethodDeclarationSyntax method)
        {
            return semanticModel.GetTypeInfo(method.ReturnType).Type.SpecialType == SpecialType.System_Void;
        }

        private void ExportBlock(Block block, MethodDeclarationSyntax parentMethod, string functionReturnType)
        {
            writer.WriteLine($"^{BlockId(block)}: // {block.GetType().Name}"); // TODO: Block arguments...
            // MLIR encodes blocks relationships in operations, not in blocks themselves
            foreach(var op in block.Instructions)
            {
                ExtractInstruction(op);
            }
            // MLIR encodes blocks relationships in operations, not in blocks themselves
            // So we need to add the corresponding operations at the end...
            switch (block)
            {
                case JumpBlock jb:
                    switch (jb.JumpNode)
                    {
                        case ReturnStatementSyntax ret:
                            if (ret.Expression == null)
                            {
                                writer.WriteLine($"return {GetLocation(ret)}");
                            }
                            else
                            {
                                Debug.Assert(functionReturnType!="()","Returning value in function declared with no return type");
                                var returnedVal = ret.Expression.RemoveParentheses();
                                if (semanticModel.GetTypeInfo(returnedVal).Type == null &&
                                    returnedVal.Kind() == SyntaxKind.SimpleMemberAccessExpression)
                                {
                                    // Special case a returning a metod group that will be cast into a func
                                    writer.WriteLine($"%{OpId(ret)} = cbde.unknown : none {GetLocation(ret)} // return method group");
                                    writer.WriteLine($"return %{OpId(ret)} : none {GetLocation(ret)}");
                                    break;
                                }
                                string returnType = MLIRType(ret.Expression);
                                if (returnType == functionReturnType)
                                {
                                    writer.WriteLine($"return %{OpId(getAssignmentValue(ret.Expression))} : {returnType} {GetLocation(ret)}");
                                }
                                else
                                {
                                    writer.WriteLine($"%{OpId(ret)} = cbde.unknown : {functionReturnType} {GetLocation(ret)} // cast return value from unsupported type");
                                    writer.WriteLine($"return %{OpId(ret)} : {functionReturnType} {GetLocation(ret)}");
                                }

                            }
                            break;
                        case BreakStatementSyntax breakStmt:
                            writer.WriteLine($"br ^{BlockId(jb.SuccessorBlock)} {GetLocation(breakStmt)} // break");
                            break;
                        case ContinueStatementSyntax continueStmt:
                            writer.WriteLine($"br ^{BlockId(jb.SuccessorBlock)} {GetLocation(continueStmt)} // continue");
                            break;
                        case ThrowStatementSyntax throwStmt:
                            // TODO : Should we transfert to a catch block if we are inside a try/catch?
                            writer.WriteLine($"cbde.throw %{OpId(throwStmt.Expression)} :  {MLIRType(throwStmt.Expression)} {GetLocation(throwStmt)}");
                            break;
                        default:
                            Debug.Assert(false, "Unknown kind of JumpBlock");
                            break;
                    }
                    break;
                case BinaryBranchBlock bbb:
                    var cond = GetCondition(bbb);
                    if (null == cond)
                    {
                        Debug.Assert(bbb.BranchingNode.Kind() == SyntaxKind.ForStatement);
                        writer.WriteLine($"br ^{BlockId(bbb.TrueSuccessorBlock)}");
                    }
                    else
                    {
                        var id = EnforceBoolOpId(cond as ExpressionSyntax);
                        writer.WriteLine($"cond_br %{id}, ^{BlockId(bbb.TrueSuccessorBlock)}, ^{BlockId(bbb.FalseSuccessorBlock)} {GetLocation(cond)}");
                    }
                    /*
                     * Up to now, we do exactly the same for all cases that may have created a BinaryBranchBlock
                     * maybe later, depending on the reason (if vs for?) we'll do something different
                     *
                    var condStatement = bbb.BranchingNode.Parent;
                    switch (condStatement.Kind())
                    {
                        case SyntaxKind.ConditionalExpression: // a ? b : c
                            var cond = condStatement as ConditionalExpressionSyntax;
                            writer.WriteLine($"cond_br %{OpId(cond.Condition)}, ^{BlockId(bbb.TrueSuccessorBlock)}, ^{BlockId(bbb.FalseSuccessorBlock)}");
                            break;
                        case SyntaxKind.IfStatement:
                            var ifCond = condStatement as IfStatementSyntax;
                            writer.WriteLine($"cond_br %{OpId(ifCond.Condition)}, ^{BlockId(bbb.TrueSuccessorBlock)}, ^{BlockId(bbb.FalseSuccessorBlock)}");
                            break;
                        case SyntaxKind.ForEachStatement:
                        case SyntaxKind.CoalesceExpression:
                        case SyntaxKind.ConditionalAccessExpression:
                        case SyntaxKind.LogicalAndExpression:
                        case SyntaxKind.LogicalOrExpression:
                        case SyntaxKind.ForStatement:
                        case SyntaxKind.CatchFilterClause:
                        default:
                            writer.WriteLine($"// Unhandled branch {bbb.BranchingNode.Kind().ToString()}");
                            break;

                    }*/
                    break;
                case SimpleBlock sb:
                    writer.WriteLine($"br ^{BlockId(sb.SuccessorBlock)}");
                    break;
                case ExitBlock eb:
                    // If we reach this point, it means the function has no return, we must manually add one
                    if (HasNoReturn(parentMethod))
                    {
                        writer.WriteLine("return");
                    }
                    else
                    {
                        writer.WriteLine("cbde.unreachable");
                    }
                    break;

            }
            writer.WriteLine();
        }

        private SyntaxNode GetCondition(BinaryBranchBlock bbb)
        {
            // For an if or a while, bbb.BranchingNode represent the condition, not the statement that holds the condition
            // For a for, bbb.BranchingNode represents the for. Since for is a statement, not an expression, if we
            // see a for, we know it's at the top level of the expression tree, so it cannot be a for inside of a if condition
            switch (bbb.BranchingNode.Kind())
            {
                case SyntaxKind.ForStatement:
                    var forStmt = bbb.BranchingNode as ForStatementSyntax;
                    return forStmt.Condition;
                case SyntaxKind.ForEachStatement:
                    Debug.Assert(false, "Not ready to handle those");
                    return null;
                default:
                    return bbb.BranchingNode;
            }
        }

        private string GetArgumentsString(MethodDeclarationSyntax method)
        {
            if (method.ParameterList.Parameters.Count == 0)
            {
                return string.Empty;
            }
            int paramCount = 0;
            var args = method.ParameterList.Parameters.Select(
                p => {
                    ++paramCount;
                    var paramName = string.IsNullOrEmpty(p.Identifier.ValueText) ?
                        ".param" + paramCount.ToString() :
                        EncodeName(p.Identifier.ValueText);
                    return $"%{paramName} : {MLIRType(p)}";
                }
                );
            return '(' + string.Join(", ", args) + ')';
        }

        private string GetAnonymousArgumentsString(MethodDeclarationSyntax method)
        {
            var args = method.ParameterList.Parameters.Select(p => MLIRType(p));
            return '(' + string.Join(", ", args) + ')';
        }

        private string MLIRType(ParameterSyntax p)
        {
            var symbolType = semanticModel.GetDeclaredSymbol(p).GetSymbolType();
            return symbolType == null ? "none" : MLIRType(symbolType);
        }

        private string MLIRType(ExpressionSyntax e)
        {
            switch (e.RemoveParentheses().Kind())
            {
                case SyntaxKind.NullLiteralExpression:
                    return "none";
                case SyntaxKind.SimpleMemberAccessExpression:
                    var type = semanticModel.GetTypeInfo(e).Type;
                    if (type == null && !e.Parent.IsKind(SyntaxKind.InvocationExpression))
                    {
                        // Case of a method group that will get transformed into at Func<>, but does not have a type
                        return "none";
                    }
                    return MLIRType(semanticModel.GetTypeInfo(e).Type);
                default:
                    return MLIRType(semanticModel.GetTypeInfo(e).Type);
            }
        }

        private string MLIRType(VariableDeclaratorSyntax v) => MLIRType(semanticModel.GetDeclaredSymbol(v).GetSymbolType());

        private string MLIRType(ITypeSymbol csType)
        {
            Debug.Assert(csType != null);
            if (csType.SpecialType == SpecialType.System_Boolean)
            {
                return "i1";
            }
            else if (csType.SpecialType == SpecialType.System_Int32)
            {
                return "i32";
            }
            else
            {
                return "none";
            }
        }

        private bool IsTypeKnown(ITypeSymbol csType)
        {
            return csType != null &&
                (csType.SpecialType == SpecialType.System_Boolean ||
                csType.SpecialType == SpecialType.System_Int32);
        }

        private ExpressionSyntax getAssignmentValue(ExpressionSyntax rhs)
        {
            rhs = rhs.RemoveParentheses();
            while (rhs.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                rhs = (rhs as AssignmentExpressionSyntax).Right.RemoveParentheses();
            }
            return rhs;
        }

        private void ExportConstant(SyntaxNode op, ITypeSymbol type, string value)
        {
            if (type.SpecialType == SpecialType.System_Boolean)
            {
                value = Convert.ToInt32(value.ToLower() == "true").ToString();
            }

            if (!IsTypeKnown(type))
            {
                writer.WriteLine($"%{OpId(op)} = constant unit {GetLocation(op)} // {op.Dump()} ({op.Kind()})");
                return;
            }
            writer.WriteLine($"%{OpId(op)} = constant {value} : {MLIRType(type)} {GetLocation(op)}");
        }

        private void ExtractInstruction(SyntaxNode op)
        {
            switch (op.Kind())
            {
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubtractExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModuloExpression:
                    {
                        var binExpr = op as BinaryExpressionSyntax;
                        ExtractBinaryExpression(binExpr, binExpr.Left, binExpr.Right);
                        break;
                    }
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                    ExtractBinaryAssignmentExpression(op);
                    break;
                case SyntaxKind.TrueLiteralExpression:
                    writer.WriteLine($"%{OpId(op)} = constant 1 : i1 {GetLocation(op)} // true");
                    break;
                case SyntaxKind.FalseLiteralExpression:
                    writer.WriteLine($"%{OpId(op)} = constant 0 : i1 {GetLocation(op)} // false");
                    break;
                case SyntaxKind.NumericLiteralExpression:
                    {
                        var lit = op as LiteralExpressionSyntax;
                        ExportConstant(op, semanticModel.GetTypeInfo(lit).Type, lit.Token.ValueText);
                        break;
                    }
                case SyntaxKind.EqualsExpression:
                    ExportComparison("eq", op);
                    break;
                case SyntaxKind.NotEqualsExpression:
                    ExportComparison("ne", op);
                    break;
                case SyntaxKind.GreaterThanExpression:
                    ExportComparison("sgt", op);
                    break;
                case SyntaxKind.GreaterThanOrEqualExpression:
                    ExportComparison("sge", op);
                    break;
                case SyntaxKind.LessThanExpression:
                    ExportComparison("slt", op);
                    break;
                case SyntaxKind.LessThanOrEqualExpression:
                    ExportComparison("sle", op);
                    break;
                case SyntaxKind.IdentifierName:
                    ExportIdentifierName(op);
                    break;
                case SyntaxKind.VariableDeclarator:
                    {
                        var decl = op as VariableDeclaratorSyntax;
                        var id = OpId(decl);
                        if (!IsTypeKnown(semanticModel.GetDeclaredSymbol(decl).GetSymbolType()))
                        {
                            // No need to write the variable, all references to it will be replaced by "unknown"
                            return;
                        }
                        writer.WriteLine($"%{id} = cbde.alloca {MLIRType(decl)} {GetLocation(decl)} // {decl.Identifier.ValueText}");
                        if (decl.Initializer != null)
                        {
                            if (!AreTypesSupported(decl.Initializer.Value))
                            {
                                writer.WriteLine("// Initialized with unknown data");
                                break;
                            }
                            var value = getAssignmentValue(decl.Initializer.Value);
                            writer.WriteLine($"cbde.store %{OpId(value)}, %{id} : memref<{MLIRType(decl)}> {GetLocation(decl)}");
                        }
                    }
                    break;
                case SyntaxKind.SimpleAssignmentExpression:
                    {
                        ExportSimpleAssignment(op);
                        break;
                    }
                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.PreDecrementExpression:
                    {
                        var prefixExp = op as PrefixUnaryExpressionSyntax;
                        ExportPrePostIncrementDecrement(prefixExp, prefixExp.OperatorToken, prefixExp.Operand, false);
                        break;
                    }
                case SyntaxKind.PostIncrementExpression:
                case SyntaxKind.PostDecrementExpression:
                    {
                        var postfixExp = op as PostfixUnaryExpressionSyntax;
                        ExportPrePostIncrementDecrement(postfixExp, postfixExp.OperatorToken, postfixExp.Operand, true);
                        break;
                    }
                default:
                    if (op is ExpressionSyntax expr && !(op.Kind() is SyntaxKind.NullLiteralExpression))
                    {
                        var exprType = semanticModel.GetTypeInfo(expr).Type;
                        if (exprType == null)
                        {
                            // Some intermediate expressions have no type (member access, initialization of member...)
                            // and therefore, they have no real value associated to them, we can just ignore them
                            break;
                        }
                        writer.WriteLine($"%{OpId(op)} = cbde.unknown : {MLIRType(exprType)} {GetLocation(op)} // {op.Dump()} ({op.Kind()})");
                    }
                    else
                    {
                        writer.WriteLine($"%{OpId(op)} = cbde.unknown : none {GetLocation(op)} // {op.Dump()} ({op.Kind()})");
                    }
                    break;
            }
        }

        private void ExportSimpleAssignment(SyntaxNode op)
        {
            var assign = op as AssignmentExpressionSyntax;
            if (!AreTypesSupported(assign))
            {
                return;
            }

            var symbolInfo = semanticModel.GetSymbolInfo(assign.Left);
            if (!IsSymbolSupportedForAssignment(symbolInfo))
            {
                return;
            }

            var lhs = symbolInfo.Symbol.DeclaringSyntaxReferences[0].GetSyntax();
            var rhsType = semanticModel.GetTypeInfo(assign.Right).Type;
            string rhsId;
            if (rhsType.Kind == SymbolKind.ErrorType)
            {
                rhsId = UniqueOpId();
                writer.WriteLine($"%{rhsId} = cbde.unknown  : {MLIRType(assign)}");
            }
            else
            {
                rhsId = OpId(getAssignmentValue(assign.Right));
            }
            writer.WriteLine($"cbde.store %{rhsId}, %{OpId(lhs)} : memref<{MLIRType(assign)}> {GetLocation(op)}");
        }

        private bool IsSymbolSupportedForAssignment(SymbolInfo symbolInfo)
        {
            // We ignore the case where lhs is not a parameter or a local variable (ie field, property...) because we currently do not support these yet
            if (symbolInfo.Symbol == null || !(symbolInfo.Symbol is ILocalSymbol || symbolInfo.Symbol is IParameterSymbol))
            {
                return false;
            }
            return true;
        }

        private void ExportIdentifierName(SyntaxNode op)
        {
            var id = op as IdentifierNameSyntax;
            var declSymbol = semanticModel.GetSymbolInfo(id).Symbol;
            if (declSymbol == null)
            {
                // In case of an unresolved call, just skip it
                writer.WriteLine($"// Unresolved: {id.Identifier.ValueText}");
                return;
            }
            if (declSymbol.DeclaringSyntaxReferences.Length == 0)
            {
                // The entity comes from another assembly
                // We can't ignore it if it is a property or a field because it may be used inside an operation (addi, subi, return...)
                // So if we ignore it, the next operation will use an unknown register
                // In case of a method, we can ignore it
                if (declSymbol is IPropertySymbol || declSymbol is IFieldSymbol)
                {
                    writer.WriteLine($"%{OpId(op)} = cbde.unknown : {MLIRType(id)} {GetLocation(op)} // Identifier from another assembly: {id.Identifier.ValueText}");
                }
                else
                {
                    writer.WriteLine($"// Entity from another assembly: {id.Identifier.ValueText}");
                }
                return;
            }
            var decl = declSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            if (decl == null ||                    // Not sure if we can be in this situation...
                decl is MethodDeclarationSyntax || // We will fetch the function only when looking at the function call itself
                decl is ClassDeclarationSyntax  || // In "Class.member", we are not interested in the "Class" part
                decl is NamespaceDeclarationSyntax
                )
            {
                // We will fetch the function only when looking at the function call itself, we just skip the identifier
                writer.WriteLine($"// Skipped because MethodDeclarationSyntax or ClassDeclarationSyntax or NamespaceDeclarationSyntax: {id.Identifier.ValueText}");
                return;
            }

            if (declSymbol is IFieldSymbol fieldSymbol && fieldSymbol.HasConstantValue)
            {
                var constValue = fieldSymbol.ConstantValue != null ? fieldSymbol.ConstantValue.ToString() : "null";
                ExportConstant(op, fieldSymbol.Type, constValue);
                return;
            }
            // IPropertySymbol could be either in a getter context (we should generate unknown) or in a setter
            // context (we should do nothing). However, it appears that in setter context, the CFG does not have an
            // instruction for fetching the property, so we should focus only on getter context.
            else if (declSymbol is IFieldSymbol || declSymbol is IPropertySymbol || !AreTypesSupported(id))
            {
                writer.WriteLine($"%{OpId(op)} = cbde.unknown : {MLIRType(id)} {GetLocation(op)} // Not a variable of known type: {id.Identifier.ValueText}");
                return;
            }
            writer.WriteLine($"%{OpId(op)} = cbde.load %{OpId(decl)} : memref<{MLIRType(id)}> {GetLocation(op)}");
        }

        private void ExtractBinaryExpression(ExpressionSyntax expr, ExpressionSyntax lhs, ExpressionSyntax rhs)
        {
            if (!AreTypesSupported(lhs, rhs, expr))
            {
                writer.WriteLine($"%{OpId(expr)} = cbde.unknown : {MLIRType(expr)} {GetLocation(expr)} // Binary expression on unsupported types {expr.Dump()}");
                return;
            }
            // TODO : C#8 : Use switch expression
            string opName;
            switch (expr.Kind())
            {
                case SyntaxKind.AddExpression:
                case SyntaxKind.AddAssignmentExpression:
                    opName = "addi";
                    break;
                case SyntaxKind.SubtractExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                    opName = "subi";
                    break;
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                    opName = "muli";
                    break;
                case SyntaxKind.DivideExpression:
                case SyntaxKind.DivideAssignmentExpression:
                    opName = "divis";
                    break;
                case SyntaxKind.ModuloExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                    opName = "remis";
                    break;
                default:
                    {
                        writer.WriteLine($"%{OpId(expr)} = cbde.unknown : {MLIRType(expr)} {GetLocation(expr)} // Unknown operator {expr.Dump()}");
                        return;
                    }
            }

            writer.WriteLine($"%{OpId(expr)} = {opName} %{OpId(getAssignmentValue(lhs))}, %{OpId(getAssignmentValue(rhs))} : {MLIRType(expr)} {GetLocation(expr)}");
        }

        private void ExtractBinaryAssignmentExpression(SyntaxNode op)
        {
            var assignExpr = op as AssignmentExpressionSyntax;
            ExtractBinaryExpression(assignExpr, assignExpr.Left, assignExpr.Right);

            var id = assignExpr.Left as IdentifierNameSyntax;
            if (null == id)
            {
                writer.WriteLine($"// No identifier name for binary assignment expression");
                return;
            }

            var declSymbol = semanticModel.GetSymbolInfo(id);
            if (!IsSymbolSupportedForAssignment(declSymbol) || !AreTypesSupported(assignExpr))
            {
                return;
            }

            var decl = declSymbol.Symbol.DeclaringSyntaxReferences[0].GetSyntax();
            writer.WriteLine($"cbde.store %{OpId(assignExpr)}, %{OpId(decl)} : memref<{MLIRType(assignExpr)}> {GetLocation(op)}");
        }

        private void ExportPrePostIncrementDecrement(ExpressionSyntax op, SyntaxToken opToken, ExpressionSyntax operand, bool isPostOperation)
        {
            // For now we only handle IdentifierNameSyntax (not ElementAccessExpressionSyntax or other)
            var id = operand as IdentifierNameSyntax;
            if (null == id)
            {
                writer.WriteLine($"%{OpId(op)} = cbde.unknown : {MLIRType(operand)} {GetLocation(op)} // Inc/Decrement of unknown identifier");
                return;
            }

            var declSymbol = semanticModel.GetSymbolInfo(id);
            if (!IsSymbolSupportedForAssignment(declSymbol) || !AreTypesSupported(operand))
            {
                writer.WriteLine($"%{OpId(op)} = cbde.unknown : {MLIRType(operand)} {GetLocation(op)} // Inc/Decrement of field or property {id.Identifier.ValueText}");
                return;
            }

            var decl = declSymbol.Symbol.DeclaringSyntaxReferences[0].GetSyntax();
            if (isPostOperation)
            {
                opMap[op] = opMap[operand];
            }

            var newCstId = UniqueOpId();
            writer.WriteLine($"%{newCstId} = constant 1 : {MLIRType(operand)} {GetLocation(op)}");

            var opId = isPostOperation ? UniqueOpId() : OpId(op);
            var opName = opToken.IsKind(SyntaxKind.PlusPlusToken) ? "addi" : "subi";
            writer.WriteLine($"%{opId} = {opName} %{OpId(operand)}, %{newCstId} : {MLIRType(operand)} {GetLocation(op)}");

            writer.WriteLine($"cbde.store %{opId}, %{OpId(decl)} : memref<{MLIRType(operand)}> {GetLocation(op)}");
        }

        private bool AreTypesSupported(params ExpressionSyntax [] exprs)
        {
            return exprs.All(expr => IsTypeKnown(semanticModel.GetTypeInfo(expr).Type));
        }

        private void ExportComparison(string compName, SyntaxNode op)
        {
            var binExpr = op as BinaryExpressionSyntax;
            if (!AreTypesSupported(binExpr.Left, binExpr.Right))
            {
                writer.WriteLine($"%{OpId(op)} = cbde.unknown : i1  {GetLocation(binExpr)} // comparison of unknown type: {op.Dump()}");
                return;
            }
            // The type is the type of the operands, not of the result, which is always i1
            writer.WriteLine($"%{OpId(op)} = cmpi \"{compName}\", %{OpId(getAssignmentValue(binExpr.Left))}, %{OpId(getAssignmentValue(binExpr.Right))} : {MLIRType(binExpr.Left)} {GetLocation(binExpr)}");
        }

        private string GetLocation(SyntaxNode node)
        {
            if (!exportsLocations)
            {
                return string.Empty;
            }
            // TODO: We should decide which of GetLineSpan or GetMappedLineSpan is better to use
            var loc = node.GetLocation().GetLineSpan();
            var location = $"loc(\"{loc.Path}\"" +
                $" :{loc.StartLinePosition.Line}" +
                $" :{loc.StartLinePosition.Character})";

            return location.Replace("\\", "\\\\");
        }

        private readonly TextWriter writer;
        private readonly SemanticModel semanticModel;
        private readonly bool exportsLocations;
        private readonly Dictionary<Block, int> blockMap = new Dictionary<Block, int>();
        private int blockCounter = 0;
        private readonly Dictionary<SyntaxNode, int> opMap = new Dictionary<SyntaxNode, int>();
        private int opCounter = 0;
        private readonly Encoding encoder = System.Text.Encoding.GetEncoding("ASCII", new PreservingEncodingFallback(), DecoderFallback.ExceptionFallback);

        public int BlockId(Block cfgBlock) =>
            this.blockMap.GetOrAdd(cfgBlock, b => this.blockCounter++);
        public string OpId(SyntaxNode node)
        {
            return this.opMap.GetOrAdd(node.RemoveParentheses(), b => this.opCounter++).ToString();
        }

        // In some cases, we need an OpId that referes to a boolean variable, even if the variable happens not to be
        // a boolean (for instance, it could be a dynamic). In such a case, we just create an unknown bool...
        // Beware not to call this function in the middle of writing some text, because it can add some of its own
        public string EnforceBoolOpId(ExpressionSyntax e)
        {
            if (MLIRType(e) != "i1")
            {
                var newId = UniqueOpId();
                writer.WriteLine($"%{newId} = cbde.unknown : i1 // Creating necessary bool for conversion");
                return newId;
            }
            return OpId(e);
        }
        public string UniqueOpId()
        {
            return (opCounter++).ToString();
        }

        public string EncodeName(string name)
        {
            Byte[] encodedBytes = encoder.GetBytes(name);
            return '_' + encoder.GetString(encodedBytes);

        }
    }
}
