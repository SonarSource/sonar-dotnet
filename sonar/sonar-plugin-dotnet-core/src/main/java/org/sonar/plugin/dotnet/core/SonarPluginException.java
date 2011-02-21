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
 * Created on May 14, 2009
 *
 */
package org.sonar.plugin.dotnet.core;

/**
 * Base exception for the sonar plugins.
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public class SonarPluginException extends RuntimeException {

  /**
   * serialVersionUID.
   */
  private static final long serialVersionUID = 3888004082525400546L;

  /**
   * Constructs a @link{SonarPluginException}.
   */
  public SonarPluginException() {
    super();

  }

  /**
   * Constructs a @link{SonarPluginException}.
   * 
   * @param message
   * @param cause
   */
  public SonarPluginException(String message, Throwable cause) {
    super(message, cause);

  }

  /**
   * Constructs a @link{SonarPluginException}.
   * 
   * @param message
   */
  public SonarPluginException(String message) {
    super(message);

  }

  /**
   * Constructs a @link{SonarPluginException}.
   * 
   * @param cause
   */
  public SonarPluginException(Throwable cause) {
    super(cause);

  }

}
