/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

/*
 * Created on Jul 2, 2009
 *
 */
package com.sonar.csharp.fxcop.profiles.xml;

import java.util.List;

import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;

@XmlType(name = "RuleFile")
public class RuleFile {

  @XmlAttribute(name = "Name")
  private String name;

  @XmlAttribute(name = "Enabled")
  private String enabled;

  @XmlAttribute(name = "AllRulesEnabled")
  private String allrulesEnabled = "False";

  @XmlElement(type = RuleDef.class, name = "Rule")
  private List<RuleDef> rules;

  /**
   * Constructs a @link{RuleFile}.
   */
  public RuleFile() {
  }

  /**
   * Returns the name.
   * 
   * @return The name to return.
   */
  public String getName() {
    return this.name;
  }

  /**
   * Sets the name.
   * 
   * @param name
   *          The name to set.
   */
  public void setName(String name) {
    this.name = name;
  }

  /**
   * Returns the enabled.
   * 
   * @return The enabled to return.
   */
  public String getEnabled() {
    return this.enabled;
  }

  /**
   * Sets the enabled.
   * 
   * @param enabled
   *          The enabled to set.
   */
  public void setEnabled(String enabled) {
    this.enabled = enabled;
  }

  /**
   * Returns the allrulesEnabled.
   * 
   * @return The allrulesEnabled to return.
   */
  public String getAllrulesEnabled() {
    return this.allrulesEnabled;
  }

  /**
   * Sets the allrulesEnabled.
   * 
   * @param allrulesEnabled
   *          The allrulesEnabled to set.
   */
  public void setAllrulesEnabled(String allrulesEnabled) {
    this.allrulesEnabled = allrulesEnabled;
  }

  /**
   * Returns the rules.
   * 
   * @return The rules to return.
   */
  public List<RuleDef> getRules() {
    return this.rules;
  }

  /**
   * Sets the rules.
   * 
   * @param rules
   *          The rules to set.
   */
  public void setRules(List<RuleDef> rules) {
    this.rules = rules;
  }

}
