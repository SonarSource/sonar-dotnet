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
import org.sonar.api.utils.SonarException;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.check.RuleProperty;

import java.util.List;
import java.util.regex.Pattern;

@Rule(
  key = "MethodName",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class MethodNameCheck extends SquidCheck<Grammar> {

  private static final String DEFAULT_FORMAT = "[A-Z][a-zA-Z]++";

  @RuleProperty(
    key = "format",
    defaultValue = "" + DEFAULT_FORMAT)
  public String format = DEFAULT_FORMAT;

  private Pattern pattern;

  @Override
  public void init() {
    subscribeTo(
        CSharpGrammar.METHOD_DECLARATION,
        CSharpGrammar.INTERFACE_METHOD_DECLARATION);

    try {
      pattern = Pattern.compile(format, Pattern.DOTALL);
    } catch (RuntimeException e) {
      throw new SonarException("[" + getClass().getSimpleName() + "] Unable to compile the regular expression: " + format, e);
    }
  }

  @Override
  public void visitNode(AstNode node) {
    AstNode identifier;

    if (node.is(CSharpGrammar.METHOD_DECLARATION)) {
      List<AstNode> identifiers = node.getFirstChild(CSharpGrammar.MEMBER_NAME).getChildren(GenericTokenType.IDENTIFIER);
      AstNode lastIdentifier = identifiers.get(identifiers.size() - 1);
      identifier = lastIdentifier;
    } else {
      identifier = node.getFirstChild(GenericTokenType.IDENTIFIER);
    }

    if (!pattern.matcher(identifier.getTokenOriginalValue()).matches()) {
      getContext().createLineViolation(this, "Rename this method to match the regular expression: " + format, identifier);
    }
  }

}
