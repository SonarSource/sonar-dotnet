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

@XmlType(name = "Spelling")
public class Spelling {

  @XmlAttribute(name = "Locale")
  private String locale = "en-US";

  /**
   * Constructs a @link{Spelling}.
   */
  public Spelling() {
    super();
  }

  /**
   * Returns the locale.
   * 
   * @return The locale to return.
   */
  public String getLocale() {
    return this.locale;
  }

  /**
   * Sets the locale.
   * 
   * @param locale
   *          The locale to set.
   */
  public void setLocale(String locale) {
    this.locale = locale;
  }

}
