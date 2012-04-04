/*
 * .NET tools :: DependencyParser Runner
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
package org.sonar.dotnet.tools.dependencyparser;

import org.sonar.dotnet.tools.commons.DotNetToolsException;

/**
 * Exceptions generated for DependencyParser execution.
 */
public abstract class DependencyParserException extends DotNetToolsException {

  private static final long serialVersionUID = 1L;
  
  private static final String ERROR_MSG = "DependencyParser analysis failed: ";
  private static final int IO_ERROR = 2;
  private static final int CONFIG_ERROR = 3;
  private static final int UNKNOWN_ERROR = 4;

  public static DependencyParserException createFromCode(int exitCode) {
    final DependencyParserException ex;
    switch (exitCode) {
      case IO_ERROR:
        ex = new IODependencyParserException();
        break;
      case CONFIG_ERROR:
        ex = new ConfigDependencyParserException();
        break;
      case UNKNOWN_ERROR:
      default:
        ex = new UnknownDependencyParserException();
        break;
    }
    return ex;
  }

  /**
   * {@inheritDoc}
   */
  protected DependencyParserException(String message) {
    super(message);
  }

  public static class IODependencyParserException extends DependencyParserException {

    private static final long serialVersionUID = 1L;

    public IODependencyParserException() {
      super(ERROR_MSG + "execution was interrupted by I/O errors (e.g. missing files).");
    }
  }
  
  public static class ConfigDependencyParserException extends DependencyParserException {

    private static final long serialVersionUID = 1L;

    public ConfigDependencyParserException() {
      super(ERROR_MSG + "errors found in the (default or user supplied) configuration files.");
    }
  }
  
  public static class UnknownDependencyParserException extends DependencyParserException {

    private static final long serialVersionUID = 1L;

    public UnknownDependencyParserException() {
      super(ERROR_MSG + "execution was interrupted by a non-handled exception. This is likely a bug inside DependencyParser...");
    }
  }

}
