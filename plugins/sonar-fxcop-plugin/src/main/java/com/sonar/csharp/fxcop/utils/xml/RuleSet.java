/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

/*
 * Created on Jul 16, 2009
 *
 */
package com.sonar.csharp.fxcop.utils.xml;

import java.util.List;

import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElementWrapper;
import javax.xml.bind.annotation.XmlType;

/**
 * The set of rules activated.
 */
@XmlType(name = "Rules")
public class RuleSet {

  @XmlElementWrapper(name = "RuleFiles")
  @XmlElement(name = "RuleFile")
  private List<RuleFile> rules;

  /**
   * Constructs a @link{RuleSet}.
   */
  public RuleSet() {
  }

  /**
   * Returns the rules.
   * 
   * @return The rules to return.
   */
  public List<RuleFile> getRules() {
    return this.rules;
  }

  /**
   * Sets the rules.
   * 
   * @param rules
   *          The rules to set.
   */
  public void setRules(List<RuleFile> rules) {
    this.rules = rules;
  }

}
