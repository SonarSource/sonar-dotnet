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
        CSharpGrammar.METHOD_DECLARATION,
        CSharpGrammar.LAMBDA_EXPRESSION,
        CSharpGrammar.ANONYMOUS_METHOD_EXPRESSION,
        CSharpGrammar.ASSIGNMENT);
  }

  @Override
  public void visitNode(AstNode node) {
    if (node.is(CSharpGrammar.METHOD_DECLARATION, CSharpGrammar.LAMBDA_EXPRESSION, CSharpGrammar.ANONYMOUS_METHOD_EXPRESSION)) {
      parameters.addAll(getNonOutNorRefParameters(node));
    } else {
      String target = getAssignmentTarget(node);

      if (parameters.contains(target)) {
        getContext().createLineViolation(this, "Remove this assignment to the method parameter '" + target + "'.", node);
      }
    }
  }

  @Override
  public void leaveNode(AstNode node) {
    if (node.is(CSharpGrammar.METHOD_DECLARATION, CSharpGrammar.LAMBDA_EXPRESSION, CSharpGrammar.ANONYMOUS_METHOD_EXPRESSION)) {
      parameters.removeAll(getNonOutNorRefParameters(node));
    }
  }

  private static Set<String> getNonOutNorRefParameters(AstNode node) {
    if (node.is(CSharpGrammar.METHOD_DECLARATION)) {
      return getNonOutNorRefMethodParameters(node);
    } else if (node.is(CSharpGrammar.LAMBDA_EXPRESSION)) {
      AstNode anonymousFunctionSignature = node.getFirstChild(CSharpGrammar.ANONYMOUS_FUNCTION_SIGNATURE);

      if (anonymousFunctionSignature.hasDirectChildren(CSharpGrammar.EXPLICIT_ANONYMOUS_FUNCTION_SIGNATURE)) {
        AstNode explicitAnonymousFunctionSignature = anonymousFunctionSignature.getFirstChild(CSharpGrammar.EXPLICIT_ANONYMOUS_FUNCTION_SIGNATURE);
        return getNonOutNorRefExplicitAnonymousFunctionParameters(explicitAnonymousFunctionSignature);
      } else {
        AstNode implicitAnonymousFunctionSignature = anonymousFunctionSignature.getFirstChild(CSharpGrammar.IMPLICIT_ANONYMOUS_FUNCTION_SIGNATURE);
        return getImplicitAnonymousFunctionParameters(implicitAnonymousFunctionSignature);
      }
    } else {
      AstNode explicitAnonymousFunctionSignature = node.getFirstChild(CSharpGrammar.EXPLICIT_ANONYMOUS_FUNCTION_SIGNATURE);
      return explicitAnonymousFunctionSignature == null ?
          (Set<String>) Collections.EMPTY_SET : getNonOutNorRefExplicitAnonymousFunctionParameters(explicitAnonymousFunctionSignature);
    }
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
    ImmutableSet.Builder<String> builder = ImmutableSet.builder();

    if (node.hasDirectChildren(CSharpGrammar.FORMAL_PARAMETER_LIST)) {
      Iterable<AstNode> fixedParameters = node
          .getFirstChild(CSharpGrammar.FORMAL_PARAMETER_LIST)
          .select()
          .children(CSharpGrammar.FIXED_PARAMETERS)
          .children(CSharpGrammar.FIXED_PARAMETER);

      for (AstNode fixedParameter : fixedParameters) {
        if (!isOutOrRefFixedParameter(fixedParameter)) {
          builder.add(fixedParameter.getFirstChild(GenericTokenType.IDENTIFIER).getTokenOriginalValue());
        }
      }
    }

    return builder.build();
  }

  private static boolean isOutOrRefFixedParameter(AstNode node) {
    return node.hasDirectChildren(CSharpGrammar.PARAMETER_MODIFIER) &&
      node.getFirstChild(CSharpGrammar.PARAMETER_MODIFIER).hasDirectChildren(CSharpKeyword.OUT, CSharpKeyword.REF);
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
