/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop.rules;

import org.sonar.api.rules.RulePriority;
import org.sonar.api.rules.RulePriorityMapper;

/**
 * A default implementation for the {@link RulePriorityMapper} interface.
 */
public class DefaultRuleMapper implements RulePriorityMapper<String, String> {

  /**
   * Constructs a @link{DefaultRuleMapper}.
   */
  public DefaultRuleMapper() {
  }

  /**
   * @param priority
   * @return
   */
  public RulePriority from(String level) {
    if ("1".equals(level)) {
      return RulePriority.BLOCKER;
    }
    if ("2".equals(level)) {
      return RulePriority.CRITICAL;
    }
    if ("3".equals(level)) {
      return RulePriority.MAJOR;
    }
    if ("4".equals(level)) {
      return RulePriority.MINOR;
    }
    if ("5".equals(level)) {
      return RulePriority.INFO;
    }
    return null;
  }

  /**
   * @param priority
   * @return
   */
  public String to(RulePriority priority) {
    if (priority.equals(RulePriority.BLOCKER)) {
      return "1";
    }
    if (priority.equals(RulePriority.CRITICAL)) {
      return "2";
    }
    if (priority.equals(RulePriority.MAJOR)) {
      return "3";
    }
    if (priority.equals(RulePriority.MINOR)) {
      return "4";
    }
    if (priority.equals(RulePriority.INFO)) {
      return "5";
    }
    throw new IllegalArgumentException("Level not supported: " + priority);

  }

}
