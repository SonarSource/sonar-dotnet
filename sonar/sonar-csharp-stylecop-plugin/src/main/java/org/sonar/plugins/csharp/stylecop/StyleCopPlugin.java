/*
 * Sonar C# Plugin :: StyleCop
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

package org.sonar.plugins.csharp.stylecop;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.api.SonarPlugin;
import org.sonar.plugins.csharp.stylecop.profiles.SonarWayProfile;
import org.sonar.plugins.csharp.stylecop.profiles.StyleCopProfileExporter;
import org.sonar.plugins.csharp.stylecop.profiles.StyleCopProfileImporter;

/**
 * Main class of the StyleCop plugin.
 */
@Properties({
    @Property(key = StyleCopConstants.INSTALL_DIR_KEY, defaultValue = StyleCopConstants.INSTALL_DIR_DEFVALUE,
        name = "StyleCop install directory", description = "Absolute path of the StyleCop program install directory.", global = true,
        project = false),
    @Property(key = StyleCopConstants.TIMEOUT_MINUTES_KEY, defaultValue = StyleCopConstants.TIMEOUT_MINUTES_DEFVALUE + "",
        name = "StyleCop program timeout", description = "Maximum number of minutes before the StyleCop program will be stopped.",
        global = true, project = true),
    @Property(key = StyleCopConstants.MODE, defaultValue = "", name = "StyleCop activation mode",
        description = "Possible values : empty (means active), 'skip' and 'reuseReport'.", global = true, project = true),
    @Property(key = StyleCopConstants.REPORTS_PATH_KEY, defaultValue = "", name = "Name of the StyleCop report files",
        description = "Name of the StyleCop report file used when reuse report mode is activated.", global = true, project = true) })
public class StyleCopPlugin extends SonarPlugin {

  /**
   * {@inheritDoc}
   */
  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(StyleCopSensor.class);

    // Rules and profiles
    list.add(StyleCopRuleRepository.class);
    list.add(StyleCopProfileImporter.class);
    list.add(StyleCopProfileExporter.class);
    list.add(SonarWayProfile.class);

    // Running StyleCop
    list.add(StyleCopResultParser.class);
    return list;
  }
}
