/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
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
