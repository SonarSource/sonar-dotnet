/*
 * Sonar C# Plugin :: C# Squid :: Checks
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package com.sonar.csharp.checks;

import com.google.common.collect.ImmutableSet;
import com.google.common.collect.Sets;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;

import java.util.Collections;
import java.util.List;
import java.util.Set;

@Rule(
  key = "ParameterAssignedTo",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class ParameterAssignedToCheck extends SquidCheck<Grammar> {

  private final Set<String> parameters = Sets.newHashSet();

  @Override
  public void init() {
    subscribeTo(
        CSharpGrammar.ASSIGNMENT,

        CSharpGrammar.METHOD_DECLARATION,
        CSharpGrammar.INDEXER_DECLARATION,
        CSharpGrammar.SET_ACCESSOR_DECLARATION,
        CSharpGrammar.OPERATOR_DECLARATION,
        CSharpGrammar.LAMBDA_EXPRESSION,
        CSharpGrammar.ANONYMOUS_METHOD_EXPRESSION);
  }

  @Override
  public void visitNode(AstNode node) {
    if (node.is(CSharpGrammar.ASSIGNMENT)) {
      String target = getAssignmentTarget(node);

      if (parameters.contains(target)) {
        getContext().createLineViolation(this, "Remove this assignment to the method parameter '" + target + "'.", node);
      }
    } else {
      parameters.addAll(getNonOutNorRefParameters(node));
    }
  }

  @Override
  public void leaveNode(AstNode node) {
    if (!node.is(CSharpGrammar.ASSIGNMENT)) {
      parameters.removeAll(getNonOutNorRefParameters(node));
    }
  }

  private static Set<String> getNonOutNorRefParameters(AstNode node) {
    Set<String> parameters;

    if (node.is(CSharpGrammar.METHOD_DECLARATION)) {
      parameters = getNonOutNorRefMethodParameters(node);
    } else if (node.is(CSharpGrammar.INDEXER_DECLARATION)) {
      parameters = getNonOutNorRefIndexerParameters(node);
    } else if (node.is(CSharpGrammar.SET_ACCESSOR_DECLARATION)) {
      parameters = ImmutableSet.of("value");
    } else if (node.is(CSharpGrammar.OPERATOR_DECLARATION)) {
      parameters = getOperatorParameters(node);
    } else if (node.is(CSharpGrammar.LAMBDA_EXPRESSION)) {
      AstNode anonymousFunctionSignature = node.getFirstChild(CSharpGrammar.ANONYMOUS_FUNCTION_SIGNATURE);

      if (anonymousFunctionSignature.hasDirectChildren(CSharpGrammar.EXPLICIT_ANONYMOUS_FUNCTION_SIGNATURE)) {
        AstNode explicitAnonymousFunctionSignature = anonymousFunctionSignature.getFirstChild(CSharpGrammar.EXPLICIT_ANONYMOUS_FUNCTION_SIGNATURE);
        parameters = getNonOutNorRefExplicitAnonymousFunctionParameters(explicitAnonymousFunctionSignature);
      } else {
        AstNode implicitAnonymousFunctionSignature = anonymousFunctionSignature.getFirstChild(CSharpGrammar.IMPLICIT_ANONYMOUS_FUNCTION_SIGNATURE);
        parameters = getImplicitAnonymousFunctionParameters(implicitAnonymousFunctionSignature);
      }
    } else if (node.is(CSharpGrammar.ANONYMOUS_METHOD_EXPRESSION)) {
      AstNode explicitAnonymousFunctionSignature = node.getFirstChild(CSharpGrammar.EXPLICIT_ANONYMOUS_FUNCTION_SIGNATURE);
      parameters = explicitAnonymousFunctionSignature == null ?
          (Set<String>) Collections.EMPTY_SET :
          getNonOutNorRefExplicitAnonymousFunctionParameters(explicitAnonymousFunctionSignature);
    } else {
      throw new IllegalArgumentException("Unexpected node type: " + node.getType() + ", " + node);
    }

    return parameters;
  }

  private static Set<String> getImplicitAnonymousFunctionParameters(AstNode node) {
    ImmutableSet.Builder<String> builder = ImmutableSet.builder();

    for (AstNode identifier : node.getDescendants(GenericTokenType.IDENTIFIER)) {
      builder.add(identifier.getTokenOriginalValue());
    }

    return builder.build();
  }

  private static Set<String> getNonOutNorRefExplicitAnonymousFunctionParameters(AstNode node) {
    ImmutableSet.Builder<String> builder = ImmutableSet.builder();

    for (AstNode explicitAnonymousFunctionParameter : node.getChildren(CSharpGrammar.EXPLICIT_ANONYMOUS_FUNCTION_PARAMETER)) {
      if (!isOutOrRefExplicitAnonymousFunctionParameter(explicitAnonymousFunctionParameter)) {
        builder.add(explicitAnonymousFunctionParameter.getFirstChild(GenericTokenType.IDENTIFIER).getTokenOriginalValue());
      }
    }

    return builder.build();
  }

  private static boolean isOutOrRefExplicitAnonymousFunctionParameter(AstNode node) {
    return node.hasDirectChildren(CSharpGrammar.ANONYMOUS_FUNCTION_PARAMETER_MODIFIER) &&
      node.getFirstChild(CSharpGrammar.ANONYMOUS_FUNCTION_PARAMETER_MODIFIER).hasDirectChildren(CSharpKeyword.OUT, CSharpKeyword.REF);
  }

  private static Set<String> getNonOutNorRefMethodParameters(AstNode node) {
    return node.hasDirectChildren(CSharpGrammar.FORMAL_PARAMETER_LIST) ?
        getNonOutNorRefFormalParameters(node.getFirstChild(CSharpGrammar.FORMAL_PARAMETER_LIST)) :
        Collections.EMPTY_SET;
  }

  private static Set<String> getNonOutNorRefIndexerParameters(AstNode node) {
    return getNonOutNorRefFormalParameters(node.getFirstChild(CSharpGrammar.INDEXER_DECLARATOR).getFirstChild(CSharpGrammar.FORMAL_PARAMETER_LIST));
  }

  private static Set<String> getNonOutNorRefFormalParameters(AstNode node) {
    ImmutableSet.Builder<String> builder = ImmutableSet.builder();

    Iterable<AstNode> fixedParameters = node
        .select()
        .children(CSharpGrammar.FIXED_PARAMETERS)
        .children(CSharpGrammar.FIXED_PARAMETER);

    for (AstNode fixedParameter : fixedParameters) {
      if (!isOutOrRefFixedParameter(fixedParameter)) {
        builder.add(fixedParameter.getFirstChild(GenericTokenType.IDENTIFIER).getTokenOriginalValue());
      }
    }

    return builder.build();
  }

  private static boolean isOutOrRefFixedParameter(AstNode node) {
    return node.hasDirectChildren(CSharpGrammar.PARAMETER_MODIFIER) &&
      node.getFirstChild(CSharpGrammar.PARAMETER_MODIFIER).hasDirectChildren(CSharpKeyword.OUT, CSharpKeyword.REF);
  }

  private static Set<String> getOperatorParameters(AstNode node) {
    ImmutableSet.Builder<String> builder = ImmutableSet.builder();

    List<AstNode> identifiers = node
        .getFirstChild(CSharpGrammar.OPERATOR_DECLARATOR)
        .getFirstChild(CSharpGrammar.UNARY_OPERATOR_DECLARATOR, CSharpGrammar.BINARY_OPERATOR_DECLARATOR, CSharpGrammar.CONVERSION_OPERATOR_DECLARATOR)
        .getChildren(GenericTokenType.IDENTIFIER);

    for (AstNode identifier : identifiers) {
      builder.add(identifier.getTokenOriginalValue());
    }

    return builder.build();
  }

  private String getAssignmentTarget(AstNode node) {
    return joinTokens(node.getFirstChild(CSharpGrammar.ASSIGNMENT_TARGET).getTokens());
  }

  private String joinTokens(List<Token> tokens) {
    StringBuilder sb = new StringBuilder();
    for (Token token : tokens) {
      sb.append(token.getOriginalValue());
    }
    return sb.toString();
  }

}
