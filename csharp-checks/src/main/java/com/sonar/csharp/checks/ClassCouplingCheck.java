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

import com.google.common.collect.Sets;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.api.Token;
import org.sonar.squidbridge.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.check.RuleProperty;

import java.util.List;
import java.util.Set;

@Rule(
  key = "ClassCoupling",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class ClassCouplingCheck extends SquidCheck<Grammar> {

  private static final int DEFAULT_COUPLING_THRESHOLD = 20;

  @RuleProperty(
    key = "couplingThreshold",
    defaultValue = "" + DEFAULT_COUPLING_THRESHOLD)
  public int couplingThreshold = DEFAULT_COUPLING_THRESHOLD;

  private final Set<String> types = Sets.newHashSet();

  @Override
  public void init() {
    subscribeTo(
        CSharpGrammar.CLASS_DECLARATION,
        CSharpGrammar.TYPE);
  }

  @Override
  public void visitNode(AstNode node) {
    if (node.is(CSharpGrammar.CLASS_DECLARATION)) {
      types.clear();
    } else {
      types.add(joinTokens(node.getTokens()));
    }
  }

  @Override
  public void leaveNode(AstNode node) {
    if (node.is(CSharpGrammar.CLASS_DECLARATION) && types.size() > couplingThreshold) {
      getContext().createLineViolation(
          this,
          "Refactor this class that is coupled to " + types.size() + " other classes (which is higher than " + couplingThreshold + " authorized).",
          node);
    }
  }

  private String joinTokens(List<Token> tokens) {
    StringBuilder sb = new StringBuilder();
    for (Token token : tokens) {
      sb.append(token.getOriginalValue());
    }
    return sb.toString();
  }

}
