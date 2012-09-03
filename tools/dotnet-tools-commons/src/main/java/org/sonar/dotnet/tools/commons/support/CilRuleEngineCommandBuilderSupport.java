/*
 * .NET tools :: Commons
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
package org.sonar.dotnet.tools.commons.support;

import java.io.File;
import java.util.Collection;

public abstract class CilRuleEngineCommandBuilderSupport extends CilToolCommandBuilderSupport {

  protected File configFile;

  /**
   * Set the configuration file that must be used to perform the analysis. It is mandatory.
   * 
   * @param configFile
   *          the file
   * @return the current builder
   */
  public void setConfigFile(File configFile) {
    this.configFile = configFile;
  }

  @Override
  protected void validate(Collection<File> assemblyToScanFiles) {
    if (configFile == null || !configFile.exists()) {
      throw new IllegalStateException("The configuration file does not exist.");
    }
    super.validate(assemblyToScanFiles);
  }

}
