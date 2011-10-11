/*
 * Sonar C# Plugin :: FxCop
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

package org.sonar.plugins.csharp.fxcop;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.api.SonarPlugin;
import org.sonar.plugins.csharp.fxcop.profiles.FxCopProfileExporter;
import org.sonar.plugins.csharp.fxcop.profiles.FxCopProfileImporter;
import org.sonar.plugins.csharp.fxcop.profiles.SonarWayProfile;

/**
 * Main class of the FxCop plugin.
 */
@Properties({
    @Property(key = FxCopConstants.INSTALL_DIR_KEY, defaultValue = FxCopConstants.INSTALL_DIR_DEFVALUE, name = "FxCop install directory",
        description = "Absolute path of the FxCop installation folder.", global = true, project = false),
    @Property(key = FxCopConstants.ASSEMBLIES_TO_SCAN_KEY, defaultValue = "", name = "Assemblies to scan",
        description = "Comma-seperated list of paths of assemblies that should be scanned. "
            + "If empty, the plugin will try to get this list from the Visual Studio 'csproj' files (if any).", global = false,
        project = true),
    @Property(key = FxCopConstants.ASSEMBLY_DEPENDENCY_DIRECTORIES_KEY, defaultValue = "", name = "Assembly dependency directories",
        description = "Comma-seperated list of folders to search for assembly dependencies.", global = true, project = true),
    @Property(key = FxCopConstants.IGNORE_GENERATED_CODE_KEY, defaultValue = FxCopConstants.IGNORE_GENERATED_CODE_DEFVALUE + "",
        name = "Ignore generated code", description = "Suppress analysis results against generated code.", global = true, project = true),
    @Property(key = FxCopConstants.TIMEOUT_MINUTES_KEY, defaultValue = FxCopConstants.TIMEOUT_MINUTES_DEFVALUE + "",
        name = "FxCop program timeout", description = "Maximum number of minutes before the FxCop program will be stopped.", global = true,
        project = true),
    @Property(key = FxCopConstants.MODE, defaultValue = "", name = "FxCop activation mode",
        description = "Possible values : empty (means active), 'skip' and 'reuseReport'.", global = false, project = false),
    @Property(key = FxCopConstants.REPORTS_PATH_KEY, defaultValue = "", name = "Name of the FxCop report files",
        description = "Name of the FxCop report file used when reuse report mode is activated. "
            + "This can be an absolute path, or a path relative to each project base directory.", global = false, project = false) })
public class FxCopPlugin extends SonarPlugin {

  /**
   * {@inheritDoc}
   */
  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(FxCopSensor.class);

    // Rules and profiles
    list.add(FxCopRuleRepository.class);
    list.add(FxCopProfileImporter.class);
    list.add(FxCopProfileExporter.class);
    list.add(SonarWayProfile.class);

    // Running FxCop
    list.add(FxCopResultParser.class);

    return list;
  }
}
