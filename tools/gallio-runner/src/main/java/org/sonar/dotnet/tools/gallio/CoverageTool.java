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

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * Coverage tools available with Gallio:
 * <ul>
 * <li>PartCover</li>
 * <li>NCover</li>
 * <li>OpenCover</li>
 * </ul>
 *
 */
public enum CoverageTool {

  /**
   * No coverage tool
   */
  NONE(GallioRunnerConstants.COVERAGE_TOOL_NONE_KEY, GallioRunnerType.ISOLATED_PROCESS),

  /**
   * "PartCover" tool
   */
  PARTCOVER(GallioRunnerConstants.COVERAGE_TOOL_PARTCOVER_KEY, GallioRunnerType.ISOLATED_APP_DOMAIN),
  /**
   * "NCover" tool
   */
  NCOVER(GallioRunnerConstants.COVERAGE_TOOL_NCOVER_KEY, GallioRunnerType.NCOVER),

  /**
   * Jetbrains "dotCover" tool
   */
  DOTCOVER(GallioRunnerConstants.COVERAGE_TOOL_DOTCOVER_KEY, GallioRunnerType.LOCAL),

  /**
   * "OpenCover" tool
   */
  OPENCOVER(GallioRunnerConstants.COVERAGE_TOOL_OPENCOVER_KEY, GallioRunnerType.ISOLATED_APP_DOMAIN);

  private static final Logger LOG = LoggerFactory.getLogger(CoverageTool.class);

  private String name;
  private GallioRunnerType gallioRunner;

  private CoverageTool(String toolName, GallioRunnerType correspondingGallioRunner) {
    this.name = toolName;
    this.gallioRunner = correspondingGallioRunner;
  }

  /**
   * Returns the Gallio runner used for this coverage tool
   *
   * @return the name of the runner
   */
  public GallioRunnerType getGallioRunner() {
    return gallioRunner;
  }

  /**
   * Returns the name of the coverage tool.
   *
   * @return the name
   */
  public String getName() {
    return name;
  }

  /**
   * Returns the coverage tool corresponding to the given name (case insensitive), or null if none found.
   *
   * @param name
   *          the name of the tool
   * @return the coverage tool if one is found, otherwise null
   */
  public static CoverageTool findFromName(String name) {
    final CoverageTool tool;
    try {
      tool = CoverageTool.valueOf(name.toUpperCase());
    } catch (IllegalArgumentException e) {
      LOG.error("Tried to get a CoverageTool with name {}, but such a tool is not supported.", name);
      throw e;
    }
    return tool;
  }

}
