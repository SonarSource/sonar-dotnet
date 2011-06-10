/*
 * .NET tools :: Gendarme Runner
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
package org.sonar.dotnet.tools.gendarme;

/**
 * Exception generated for Gendarme execution.
 */
public class GendarmeException extends Exception {

  private static final long serialVersionUID = -8744090490644930724L;

  private static final int IO_ERROR = 2;
  private static final int CONFIG_ERROR = 3;
  private static final int UNKNOWN_ERROR = 4;

  /**
   * Creates a new exception with the message corresponding to the given exit code.
   * 
   * @param exitCode
   *          the exit code
   */
  public GendarmeException(int exitCode) {
    super("Gendarme analysis failed: " + getSpecificMessage(exitCode));
  }

  private static String getSpecificMessage(int exitCode) {
    String specificMessage = "";
    switch (exitCode) {
      case IO_ERROR:
        specificMessage = "execution was interrupted by I/O errors (e.g. missing files).";
        break;
      case CONFIG_ERROR:
        specificMessage = "errors found in the (default or user supplied) configuration files.";
        break;
      case UNKNOWN_ERROR:
      default:
        specificMessage = "execution was interrupted by a non-handled exception. This is likely a bug inside Gendarme and should be reported on Novell's bugzilla (http://bugzilla.novell.com) or on the mailing-list.";
        break;
    }
    return specificMessage;
  }

  /**
   * {@inheritDoc}
   */
  public GendarmeException(String message) {
    super(message);
  }

  /**
   * {@inheritDoc}
   */
  public GendarmeException(String message, Throwable cause) {
    super(message, cause);
  }

}
