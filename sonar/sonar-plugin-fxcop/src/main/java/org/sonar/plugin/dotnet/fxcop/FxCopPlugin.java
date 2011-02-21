/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

/*
 * Created on May 7, 2009
 */
package org.sonar.plugin.dotnet.fxcop;

import static org.sonar.plugin.dotnet.fxcop.Constants.*;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Plugin;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;

/**
 * A plugin responsible of FXCop reporting in sonar.
 * 
 * @author Jose CHILLAN Feb 16, 2010
 */
@Properties({
  @Property(
      key = FXCOP_MODE_KEY,
      defaultValue = FXCOP_DEFAULT_MODE,
      name = "FxCop activation mode",
      description = "Possible values : enable, skip, reuseReport",
      project = true,
      module = false,
      global = true),
  @Property(
      key = FXCOP_REPORT_KEY,
      defaultValue = FXCOP_REPORT_XML,
      name = "Name of the FxCop report file",
      description = "Name of the FxCop report file used when reuse report mode is activated. " +
      		"If several reports need to be analysed (may happen with silverlight), several path " +
      		"may be specified using ';' as a delimiter",
      project = true,
      module = false,
      global = true)
})
public class FxCopPlugin implements Plugin {
  public static final String KEY = "fxcop";

  /**
   * Constructs a @link{FxCopPlugin}.
   */
  public FxCopPlugin() {
  }

  public String getDescription() {
    return "A plugin that collects the FxCop check results";
  }

  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(FxCopRuleRepository.class);
    list.add(FxCopSensor.class);
    list.add(FxCopPluginHandler.class);
    list.add(CSharpFileLocator.class);

    return list;
  }

  public String getKey() {
    return KEY;
  }

  public String getName() {
    return "[.net] FxCop";
  }

  @Override
  public String toString() {
    return getKey();
  }
}
