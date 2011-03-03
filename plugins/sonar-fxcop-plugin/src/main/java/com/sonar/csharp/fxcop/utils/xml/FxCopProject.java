/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

/*
 * Created on Jul 2, 2009
 */
package com.sonar.csharp.fxcop.utils.xml;

import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;

@XmlRootElement(name = "FxCopProject")
@XmlType(name = "FxCopProject")
public class FxCopProject {

  @XmlAttribute(name = "Version")
  private String version = "1.36";

  @XmlAttribute(name = "Name")
  private String name;

  @XmlElement(name = "ProjectOptions")
  private ProjectOptions projectOptions;

  @XmlElement(name = "Rules")
  private RuleSet rules;

  /**
   * Constructs a @link{FxCopReport}.
   */
  public FxCopProject() {
  }

  /**
   * Returns the projectOptions.
   * 
   * @return The projectOptions to return.
   */
  public ProjectOptions getProjectOptions() {
    return this.projectOptions;
  }

  /**
   * Sets the projectOptions.
   * 
   * @param projectOptions
   *          The projectOptions to set.
   */
  public void setProjectOptions(ProjectOptions projectOptions) {
    this.projectOptions = projectOptions;
  }

  /**
   * Returns the rules.
   * 
   * @return The rules to return.
   */
  public RuleSet getRules() {
    return this.rules;
  }

  /**
   * Sets the rules.
   * 
   * @param rules
   *          The rules to set.
   */
  public void setRules(RuleSet rules) {
    this.rules = rules;
  }

  /**
   * Returns the version.
   * 
   * @return The version to return.
   */
  public String getVersion() {
    return this.version;
  }

  /**
   * Sets the version.
   * 
   * @param version
   *          The version to set.
   */
  public void setVersion(String version) {
    this.version = version;
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
}
