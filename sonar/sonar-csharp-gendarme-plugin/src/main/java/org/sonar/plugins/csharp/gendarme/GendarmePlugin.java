/*
 * Sonar C# Plugin :: Gendarme
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

package org.sonar.plugins.csharp.gendarme;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.api.SonarPlugin;
import org.sonar.plugins.csharp.gendarme.profiles.GendarmeProfileExporter;
import org.sonar.plugins.csharp.gendarme.profiles.GendarmeProfileImporter;
import org.sonar.plugins.csharp.gendarme.profiles.SonarWayProfile;
import org.sonar.plugins.csharp.gendarme.results.GendarmeResultParser;
import org.sonar.plugins.csharp.gendarme.results.GendarmeViolationMaker;

/**
 * Main class of the Gendarme plugin.
 */
@Properties({
    @Property(key = GendarmeConstants.EXECUTABLE_KEY, defaultValue = GendarmeConstants.EXECUTABLE_DEFVALUE, name = "Gendarme executable",
        description = "Absolute path of the Gendarme program.", global = true, project = false),
    @Property(key = GendarmeConstants.ASSEMBLIES_TO_SCAN_KEY, defaultValue = GendarmeConstants.ASSEMBLIES_TO_SCAN_DEFVALUE,
        name = "Assemblies to scan", description = "Comma-seperated list of paths of assemblies that should be scanned. "
            + "If empty, the plugin will try to get this list from the Visual Studio 'csproj' files (if any).", global = false,
        project = true),
    @Property(key = GendarmeConstants.GENDARME_CONFIDENCE_KEY, defaultValue = GendarmeConstants.GENDARME_CONFIDENCE_DEFVALUE,
        name = "Gendarme confidence", description = "Filter defects for the specified confidence levels. (low/normal/high/total with +/-)",
        global = true, project = true),
    @Property(key = GendarmeConstants.TIMEOUT_MINUTES_KEY, defaultValue = GendarmeConstants.TIMEOUT_MINUTES_DEFVALUE + "",
        name = "Gendarme program timeout", description = "Maximum number of minutes before the Gendarme program will be stopped.",
        global = true, project = true),
    @Property(key = GendarmeConstants.MODE, defaultValue = "", name = "Gendarme activation mode",
        description = "Possible values : empty (means active), 'skip' and 'reuseReport'.", global = true, project = true),
    @Property(key = GendarmeConstants.REPORTS_PATH_KEY, defaultValue = "", name = "Name of the Gendarme report files",
        description = "Name of the Gendarme report file used when reuse report mode is activated.", global = true, project = true) })
public class GendarmePlugin extends SonarPlugin {

  /**
   * {@inheritDoc}
   */
  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(GendarmeSensor.class);

    // Rules and profiles
    list.add(GendarmeRuleRepository.class);
    list.add(GendarmeProfileImporter.class);
    list.add(GendarmeProfileExporter.class);
    list.add(SonarWayProfile.class);

    // Running Gendarme
    list.add(GendarmeResultParser.class);
    list.add(GendarmeViolationMaker.class);
    return list;
  }
}
