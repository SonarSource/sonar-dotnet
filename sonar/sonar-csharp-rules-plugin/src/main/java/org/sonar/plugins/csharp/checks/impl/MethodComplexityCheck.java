/*
 * Sonar C# Plugin :: Rules
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
package org.sonar.plugins.csharp.checks.impl;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.check.RuleProperty;
import org.sonar.plugins.csharp.api.CSharpConstants;

import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.source.SourceMember;
import com.sonar.plugins.csharp.squid.check.CSharpCheck;
import com.sonar.sslr.api.AstNode;

@Rule(key = "CSharp.MethodComplexity", name = "Avoid too complex methods", priority = Priority.MAJOR,
    description = "<p>The cyclomatic complexity of a method should not exceed a defined threshold. "
        + "Complex code can perform poorly and will in any case be difficult to understand and therefore to maintain.</p>")
@BelongsToProfile(title = CSharpConstants.CSHARP_WAY_PROFILE, priority = Priority.MAJOR)
public class MethodComplexityCheck extends CSharpCheck {

  private static final Logger LOG = LoggerFactory.getLogger(MethodComplexityCheck.class);

  private final static int DEFAULT_MAXIMUM_METHOD_COMPLEXITY_THRESHOLD = 20;

  @RuleProperty(key = "maximumMethodComplexityThreshold", description = "The maximum authorized complexity in methods.", defaultValue = ""
      + DEFAULT_MAXIMUM_METHOD_COMPLEXITY_THRESHOLD)
  private int maximumMethodComplexityThreshold = DEFAULT_MAXIMUM_METHOD_COMPLEXITY_THRESHOLD;

  @Override
  public void init() {
    LOG.debug("... Init MethodComplexityCheck with max value to {}", maximumMethodComplexityThreshold);
    subscribeTo(getCSharpGrammar().methodBody);
  }

  public void leaveNode(AstNode node) {
    SourceMember method = (SourceMember) peekSourceCode();
    if (method.getInt(CSharpMetric.COMPLEXITY) > maximumMethodComplexityThreshold) {
      log("Method has a complexity of {0,number,integer} which is greater than {1,number,integer} authorized.", node,
          method.getInt(CSharpMetric.COMPLEXITY), maximumMethodComplexityThreshold);
    }
  }

  public void setMaximumMethodComplexityThreshold(int threshold) {
    this.maximumMethodComplexityThreshold = threshold;
  }
}
