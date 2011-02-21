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
 * Created on Jul 2, 2009
 *
 */
package org.sonar.plugin.dotnet.fxcop.xml;

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
