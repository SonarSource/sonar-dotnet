/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

/*
 * Created on Jul 16, 2009
 *
 */
package org.sonar.plugin.dotnet.fxcop.xml;

import java.util.List;

import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElementWrapper;
import javax.xml.bind.annotation.XmlType;

/**
 * The set of rules activated.
 * 
 * @author Jose CHILLAN Jul 16, 2009
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
