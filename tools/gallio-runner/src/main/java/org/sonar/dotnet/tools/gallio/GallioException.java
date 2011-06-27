/*
 * .NET tools :: Gallio Runner
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
package org.sonar.dotnet.tools.gallio;

/**
 * Exception generated for Gallio execution.
 */
public class GallioException extends Exception {

  private static final long serialVersionUID = 4671247905622433478L;

  private static final int FAILURE = 1;
  private static final int CANCELED = 2;
  private static final int FATAL_EXCEPTION = 3;
  private static final int INVALID_ARGUMENTS = 10;

  /**
   * Creates a new exception with the message corresponding to the given exit code.
   * 
   * @param exitCode
   *          the exit code
   */
  public GallioException(int exitCode) {
    super("Gallio analysis failed: " + getSpecificMessage(exitCode));
  }

  private static String getSpecificMessage(int exitCode) {
    String specificMessage = "";
    switch (exitCode) {
      case FAILURE:
        specificMessage = "some tests failed.";
        break;
      case CANCELED:
        specificMessage = "the tests were canceled.";
        break;
      case INVALID_ARGUMENTS:
        specificMessage = "invalid arguments were supplied on the command line. Please contact the support on 'user@sonar.codehaus.org'.";
        break;
      case FATAL_EXCEPTION:
      default:
        specificMessage = "a fatal runtime exception occurred. Check the result file to potentially know more about it.";
        break;
    }
    return specificMessage;
  }

  /**
   * {@inheritDoc}
   */
  public GallioException(String message) {
    super(message);
  }

  /**
   * {@inheritDoc}
   */
  public GallioException(String message, Throwable cause) {
    super(message, cause);
  }

}
