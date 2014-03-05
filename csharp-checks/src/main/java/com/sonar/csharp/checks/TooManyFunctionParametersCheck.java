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

import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.check.RuleProperty;
import org.sonar.sslr.grammar.GrammarRuleKey;

@Rule(
  key = "S107",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class TooManyFunctionParametersCheck extends SquidCheck<Grammar> {

  public static final int DEFAULT = 7;
  private static final GrammarRuleKey[] METHODS_AND_FUNCTIONS_DEC = {
    CSharpGrammar.METHOD_DECLARATION,
    CSharpGrammar.CONSTRUCTOR_DECLARATOR,
    CSharpGrammar.INTERFACE_METHOD_DECLARATION,
    CSharpGrammar.DELEGATE_DECLARATION
  };

  @RuleProperty(
    key = "max",
    defaultValue = "" + DEFAULT)
  int max = DEFAULT;

  @Override
  public void init() {
    subscribeTo(METHODS_AND_FUNCTIONS_DEC);
    subscribeTo(
      CSharpGrammar.EXPLICIT_ANONYMOUS_FUNCTION_SIGNATURE,
      CSharpGrammar.IMPLICIT_ANONYMOUS_FUNCTION_SIGNATURE);
  }

  @Override
  public void visitNode(AstNode node) {
    int nbParameters = getNumberOfParameters(node);

    if (nbParameters > max) {
      getContext().createLineViolation(this, "{0} has {1} parameters, which is greater than the {2} authorized.",
        getReportNode(node), getFunctionTitle(node), nbParameters, max);
    }
  }

  private int getNumberOfParameters(AstNode node) {
    if (node.is(CSharpGrammar.EXPLICIT_ANONYMOUS_FUNCTION_SIGNATURE)) {
      return node.getChildren(CSharpGrammar.EXPLICIT_ANONYMOUS_FUNCTION_PARAMETER).size();

    } else if (node.is(CSharpGrammar.IMPLICIT_ANONYMOUS_FUNCTION_SIGNATURE)) {
      return node.getChildren(CSharpGrammar.IMPLICIT_ANONYMOUS_FUNCTION_PARAMETER).size();

    } else {
      return getNumberOfFormalParameter(node.getFirstChild(CSharpGrammar.FORMAL_PARAMETER_LIST));
    }
  }

  private int getNumberOfFormalParameter(AstNode formalParameterList) {
    int nbParameters = 0;

    if (formalParameterList != null) {
      AstNode fixedParameters = formalParameterList.getFirstChild(CSharpGrammar.FIXED_PARAMETERS);

      if (fixedParameters != null) {
        nbParameters += fixedParameters.getChildren(CSharpGrammar.FIXED_PARAMETER).size();
      }

      if (formalParameterList.hasDirectChildren(CSharpGrammar.PARAMETER_ARRAY)) {
        nbParameters++;
      }
    }
    return nbParameters;
  }

  private AstNode getReportNode(AstNode node) {
    // If method has attribute, issue will not be reported on the first attribute line.
    if (node.is(METHODS_AND_FUNCTIONS_DEC)) {
      return node.getFirstChild(GenericTokenType.IDENTIFIER, CSharpGrammar.MEMBER_NAME);
    }
    return node;
  }

  private String getFunctionTitle(AstNode node) {
    String title;

    if (node.getParent().is(CSharpGrammar.ANONYMOUS_FUNCTION_SIGNATURE)) {
      title = "Lambda";
    } else if (node.getParent().is(CSharpGrammar.ANONYMOUS_METHOD_EXPRESSION)) {
      title = "Function";
    } else if (node.is(CSharpGrammar.DELEGATE_DECLARATION)) {
      title = "Delegate \"" + node.getFirstChild(GenericTokenType.IDENTIFIER).getTokenValue() + "\"";
    } else {
      title = "Method \"" + node.getFirstChild(GenericTokenType.IDENTIFIER, CSharpGrammar.MEMBER_NAME).getTokenValue() + "\"";
    }

    return title;
  }
}
