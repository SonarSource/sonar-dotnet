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
import javax.xml.bind.annotation.XmlValue;

/**
 * The Stylesheet to use for a FxCop report.
 */
@XmlType(name = "Stylesheet")
public class Stylesheet {

  @XmlAttribute(name = "Apply")
  private String apply = "False";
  @XmlValue
  private String path = "c:\\program files\\microsoft fxcop 1.36\\Xml\\FxCopReport.xsl";

  /**
   * Constructs a @link{Stylesheet}.
   */
  public Stylesheet() {
  }

  /**
   * Returns the apply.
   * 
   * @return The apply to return.
   */
  public String getApply() {
    return this.apply;
  }

  /**
   * Sets the apply.
   * 
   * @param apply
   *          The apply to set.
   */
  public void setApply(String apply) {
    this.apply = apply;
  }

  /**
   * Returns the path.
   * 
   * @return The path to return.
   */
  public String getPath() {
    return this.path;
  }

  /**
   * Sets the path.
   * 
   * @param path
   *          The path to set.
   */
  public void setPath(String path) {
    this.path = path;
  }

}
