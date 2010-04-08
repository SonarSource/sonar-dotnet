/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
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
 * Created on Sep 1, 2009
 *
 */
package org.sonar.plugin.dotnet.core.resource;

/**
 * Exception throws when an invalid resource is being created.
 * @author Jose CHILLAN Sep 1, 2009
 */
public class InvalidResourceException extends RuntimeException
{

  /**
   * serialVersionUID
   */
  private static final long serialVersionUID = 615664448518509854L;

  /**
   * Constructs a @link{InvalidResourceException}.
   */
  public InvalidResourceException()
  {
  }

  /**
   * Constructs a @link{InvalidResourceException}.
   * @param message
   */
  public InvalidResourceException(String message)
  {
    super(message);

  }

  /**
   * Constructs a @link{InvalidResourceException}.
   * @param cause
   */
  public InvalidResourceException(Throwable cause)
  {
    super(cause);

  }

  /**
   * Constructs a @link{InvalidResourceException}.
   * @param message
   * @param cause
   */
  public InvalidResourceException(String message, Throwable cause)
  {
    super(message, cause);

  }

}
