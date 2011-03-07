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

import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlType;

@XmlType(name = "Rule")
public class RuleDef {

  @XmlAttribute(name = "Name")
  private String name;

  @XmlAttribute(name = "Enabled")
  private String enabled = "True";

  @XmlAttribute(name = "SonarPriority")
  private String priority = "major";

  /**
   * Constructs a @link{RulesDef}.
   */
  public RuleDef() {
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

  public String getPriority() {
    return priority;
  }

  public void setPriority(String priority) {
    this.priority = priority;
  }

}
