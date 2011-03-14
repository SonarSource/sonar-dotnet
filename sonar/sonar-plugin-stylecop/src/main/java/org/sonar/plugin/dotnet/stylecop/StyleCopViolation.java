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
    final int prime = 31;
    int result = 1;
    result = prime * result + ((filePath == null) ? 0 : filePath.hashCode());
    result = prime * result + ((key == null) ? 0 : key.hashCode());
    result = prime * result + ((lineNumber == null) ? 0 : lineNumber.hashCode());
    result = prime * result + ((message == null) ? 0 : message.hashCode());
    return result;
  }

  @Override
  @SuppressWarnings("all")
  public boolean equals(Object obj) {
    if (this == obj)
      return true;
    if (obj == null)
      return false;
    if (getClass() != obj.getClass())
      return false;
    StyleCopViolation other = (StyleCopViolation) obj;
    if (filePath == null) {
      if (other.filePath != null)
        return false;
    } else if ( !filePath.equals(other.filePath))
      return false;
    if (key == null) {
      if (other.key != null)
        return false;
    } else if ( !key.equals(other.key))
      return false;
    if (lineNumber == null) {
      if (other.lineNumber != null)
        return false;
    } else if ( !lineNumber.equals(other.lineNumber))
      return false;
    if (message == null) {
      if (other.message != null)
        return false;
    } else if ( !message.equals(other.message))
      return false;
    return true;
  }

}
