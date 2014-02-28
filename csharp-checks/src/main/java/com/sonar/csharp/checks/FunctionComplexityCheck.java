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

import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.source.SourceMember;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.check.RuleProperty;
import org.sonar.squid.api.SourceCode;

@Rule(
  key = "FunctionComplexity",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class FunctionComplexityCheck extends SquidCheck<Grammar> {

  private static final int DEFAULT_MAXIMUM_FUNCTION_COMPLEXITY_THRESHOLD = 10;

  @RuleProperty(
    key = "maximumFunctionComplexityThreshold",
    defaultValue = "" + DEFAULT_MAXIMUM_FUNCTION_COMPLEXITY_THRESHOLD)
  public int maximumFunctionComplexityThreshold = DEFAULT_MAXIMUM_FUNCTION_COMPLEXITY_THRESHOLD;

  @Override
  public void init() {
    subscribeTo(
        CSharpGrammar.METHOD_DECLARATION,
        CSharpGrammar.CONSTRUCTOR_BODY,
        CSharpGrammar.STATIC_CONSTRUCTOR_BODY,
        CSharpGrammar.DESTRUCTOR_BODY,
        CSharpGrammar.ACCESSOR_BODY,
        CSharpGrammar.ADD_ACCESSOR_DECLARATION,
        CSharpGrammar.REMOVE_ACCESSOR_DECLARATION,
        CSharpGrammar.OPERATOR_BODY);
  }

  @Override
  public void leaveNode(AstNode node) {
    SourceCode source = getContext().peekSourceCode();
    if (source instanceof SourceMember) {
      SourceMember member = (SourceMember) source;
      if (member.getInt(CSharpMetric.COMPLEXITY) > maximumFunctionComplexityThreshold) {
        getContext().createLineViolation(this,
            "Refactor this method that has a complexity of {0} (which is greater than {1} authorized).",
            node,
            member.getInt(CSharpMetric.COMPLEXITY),
            maximumFunctionComplexityThreshold);
      }
    }
  }

}
