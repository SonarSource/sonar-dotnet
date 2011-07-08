/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid;

import java.util.List;

import org.sonar.api.rules.AnnotationRuleParser;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleRepository;
import org.sonar.plugins.csharp.api.CSharpConstants;

import com.sonar.plugins.csharp.squid.check.CSharpCheck;

/**
 * C# rule repository fed by all the {@link CSharpCheck} found by the container.
 * 
 */
public class CSharpRuleRepository extends RuleRepository {

  private CSharpCheck[] checks;

  /**
   * Creates a {@link CSharpRuleRepository}
   */
  public CSharpRuleRepository() {
    this(new CSharpCheck[] {});
  }

  /**
   * Creates a {@link CSharpRuleRepository}
   * 
   * @param checks
   *          the C# checks provided by the container
   */
  public CSharpRuleRepository(CSharpCheck[] checks) {
    super(CSharpSquidConstants.REPOSITORY_KEY, CSharpConstants.LANGUAGE_KEY);
    setName(CSharpSquidConstants.REPOSITORY_NAME);
    this.checks = checks;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public List<Rule> createRules() {
    return new AnnotationRuleParser().parse(getKey(), CSharpCheck.toCollection(checks));
  }
}
