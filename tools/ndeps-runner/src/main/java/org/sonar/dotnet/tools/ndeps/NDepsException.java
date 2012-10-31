/*
 * .NET tools :: NDeps Runner
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
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
package org.sonar.dotnet.tools.ndeps;

import org.sonar.plugins.dotnet.api.DotNetException;

/**
 * Exceptions generated for DependencyParser execution.
 */
public class NDepsException extends DotNetException {

  private static final long serialVersionUID = 1L;

  private static final String ERROR_MSG = "DependencyParser analysis failed: ";
  private static final int IO_ERROR = 2;
  private static final int CONFIG_ERROR = 3;
  private static final int UNKNOWN_ERROR = 4;

  /**
   * {@inheritDoc}
   */
  public NDepsException(String message) {
    super(message);
  }

  /**
   * {@inheritDoc}
   */
  public NDepsException(String message, Throwable cause) {
    super(message, cause);
  }

  public static NDepsException createFromCode(int exitCode) {
    final NDepsException ex;
    String message = ERROR_MSG;
    switch (exitCode) {
      case IO_ERROR:
        ex = new NDepsException(message + "execution was interrupted by I/O errors (e.g. missing files).");
        break;
      case CONFIG_ERROR:
        ex = new NDepsException(message + "errors found in the (default or user supplied) configuration files.");
        break;
      case UNKNOWN_ERROR:
      default:
        ex = new NDepsException(message + "execution was interrupted by a non-handled exception. This is likely a bug inside DependencyParser...");
        break;
    }
    return ex;
  }

}
