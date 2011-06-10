/*
 * .NET tools :: Commons
 * Copyright (C) 2011 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
/**
 * 
 */
package org.sonar.plugins.csharp.api;

/**
 * Exception class for the .NET Tools
 * 
 */
public class DotNetToolsException extends Exception {

  private static final long serialVersionUID = -2730236966462112505L;

  /**
   * Creates a {@link DotNetToolsException}
   * 
   * @param message
   *          the message
   * @param cause
   *          the cause
   */
  public DotNetToolsException(String message, Throwable cause) {
    super(message, cause);
  }

  /**
   * Creates a {@link DotNetToolsException}
   * 
   * @param cause
   *          the cause
   */
  public DotNetToolsException(String cause) {
    super(cause);
  }

}
