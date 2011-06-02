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

package org.sonar.plugin.dotnet.stylecop;

import org.apache.commons.lang.builder.EqualsBuilder;
import org.apache.commons.lang.builder.HashCodeBuilder;

/**
 * This class was made to modelize violations provided by StyleCop reports
 * 
 * @author Maxime SCHNEIDER-DUFEUTRELLE March 01, 2011
 */
public class StyleCopViolation {

  public StyleCopViolation(String lineNumber, String filePath, String key, String message) {
    super();
    this.lineNumber = lineNumber;
    this.filePath = filePath;
    this.key = key;
    this.message = message;
  }

  private String lineNumber;
  private String filePath;
  private String key;
  private String message;

  public String getLineNumber() {
    return lineNumber;
  }

  public void setLineNumber(String lineNumber) {
    this.lineNumber = lineNumber;
  }

  public String getFilePath() {
    return filePath;
  }

  public void setFilePath(String filePath) {
    this.filePath = filePath;
  }

  public String getKey() {
    return key;
  }

  public void setKey(String key) {
    this.key = key;
  }

  public String getMessage() {
    return message;
  }

  public void setMessage(String message) {
    this.message = message;
  }

  @Override
  public int hashCode() {
    return HashCodeBuilder.reflectionHashCode(this);
  }

  @Override
  @SuppressWarnings("all")
  public boolean equals(Object obj) {
    return EqualsBuilder.reflectionEquals(this, obj);
  }

}
