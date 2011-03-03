/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

/*
 * Created on Jul 2, 2009
 *
 */
package com.sonar.csharp.fxcop.utils.xml;

import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlType;

/**
 * A Project or Report for FxCop options.
 */
@XmlType(name = "Entity")
public class Entity {

  @XmlAttribute(name = "Status")
  private String status = "Active";

  @XmlAttribute(name = "NewOnly")
  private String newOnly = "False";

  public Entity() {
  }

  /**
   * Returns the status.
   * 
   * @return The status to return.
   */
  public String getStatus() {
    return this.status;
  }

  /**
   * Sets the status.
   * 
   * @param status
   *          The status to set.
   */
  public void setStatus(String status) {
    this.status = status;
  }

  /**
   * Returns the newOnly.
   * 
   * @return The newOnly to return.
   */
  public String getNewOnly() {
    return this.newOnly;
  }

  /**
   * Sets the newOnly.
   * 
   * @param newOnly
   *          The newOnly to set.
   */
  public void setNewOnly(String newOnly) {
    this.newOnly = newOnly;
  }

}
