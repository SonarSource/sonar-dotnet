/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2018 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package com.sonar.it.shared;

import com.sonar.orchestrator.locator.FileLocation;
import com.sonar.orchestrator.locator.Location;
import com.sonar.orchestrator.locator.MavenLocation;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;

public class TestUtils {

  final private static Logger LOG = LoggerFactory.getLogger(TestUtils.class);

  public static Location getPluginLocation (String pluginName) {
    Location pluginLocation;

    String version = System.getProperty("csharpVersion"); // C# and VB.Net versions are the same
    LOG.info("Locating plugin: " + pluginName + " Version: " + version);

    if (StringUtils.isEmpty(version)) {
      // use the plugin that was built on local machine
      LOG.info("Using local plugin version");
      pluginLocation = FileLocation.byWildcardMavenFilename(new File("../" + pluginName + "/target"), pluginName + "-*.jar");
    } else {
      // QA environment downloads the plugin built by the CI job
      LOG.info("Using version from Maven");
      pluginLocation = MavenLocation.of("org.sonarsource.dotnet", pluginName, version);
    }

    LOG.info("Plugin location=" + pluginLocation);
    return pluginLocation;
  }

}
