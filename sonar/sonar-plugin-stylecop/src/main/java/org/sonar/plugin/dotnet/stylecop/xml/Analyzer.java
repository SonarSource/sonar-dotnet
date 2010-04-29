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
 * Created on Jul 21, 2009
 */
package org.sonar.plugin.dotnet.stylecop.xml;

import java.util.List;

import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElementWrapper;
import javax.xml.bind.annotation.XmlType;

/**
 * A definition of a rule analyzer.
 * 
 * @author Jose CHILLAN Jul 21, 2009
 */
@XmlType(name = "Analyzer")
public class Analyzer
{
  @XmlAttribute(name = "AnalyzerId")
  private String        id;

  @XmlElementWrapper(name = "Rules")
  @XmlElement(name = "Rule")
  private List<RuleDef> rules;

  /**
   * Constructs a @link{Analyser}.
   */
  public Analyzer()
  {
  }

  /**
   * Returns the id.
   * 
   * @return The id to return.
   */
  public String getId()
  {
    return this.id;
  }

  /**
   * Sets the id.
   * 
   * @param id The id to set.
   */
  public void setId(String id)
  {
    this.id = id;
  }

  /**
   * Returns the rules.
   * 
   * @return The rules to return.
   */
  public List<RuleDef> getRules()
  {
    return this.rules;
  }

  /**
   * Sets the rules.
   * 
   * @param rules The rules to set.
   */
  public void setRules(List<RuleDef> rules)
  {
    this.rules = rules;
  }

}
