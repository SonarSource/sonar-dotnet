/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

/*
 * Created on Jul 2, 2009
 *
 */
package org.sonar.plugin.dotnet.fxcop.xml;

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
