/*
 * Sonar C# Plugin :: C# Squid :: Sonar C# Squid Plugin
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
package com.sonar.plugins.csharp.squid;

import com.sonar.csharp.checks.CheckList;
import com.sonar.plugins.csharp.squid.check.CSharpCheck;
import org.sonar.api.rules.AnnotationRuleParser;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleRepository;
import org.sonar.plugins.csharp.api.CSharpConstants;

import java.util.Collection;
import java.util.List;

public class CSharpRuleRepository extends RuleRepository {

  /* We have to put those in our own repository, see http://jira.codehaus.org/browse/SONAR-3157 */
  private final CSharpCheck[] customerProvidedChecks;

  public CSharpRuleRepository() {
    this(new CSharpCheck[0]);
  }

  public CSharpRuleRepository(CSharpCheck[] customerProvidedChecks) {
    super(CSharpSquidConstants.REPOSITORY_KEY, CSharpConstants.LANGUAGE_KEY);
    setName(CSharpSquidConstants.REPOSITORY_NAME);
    this.customerProvidedChecks = customerProvidedChecks;
  }

  @Override
  public List<Rule> createRules() {
    return new AnnotationRuleParser().parse(getKey(), getChecks());
  }

  public Collection<Class> getChecks() {
    Collection<Class> allChecks = CSharpCheck.toCollection(customerProvidedChecks);
    allChecks.addAll(CheckList.getChecks());
    return allChecks;
  }

}
