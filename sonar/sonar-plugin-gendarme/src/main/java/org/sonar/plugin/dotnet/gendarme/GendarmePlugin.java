/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

package org.sonar.plugin.dotnet.gendarme;

import static org.sonar.plugin.dotnet.gendarme.Constants.*;

import java.util.ArrayList;
import java.util.List;


import org.sonar.api.Properties;
import org.sonar.api.Property;

import org.sonar.api.Extension;
import org.sonar.api.Plugin;

@Properties({
  @Property(
      key = GENDARME_MODE_KEY,
      defaultValue = GENDARME_DEFAULT_MODE,
      name = "Mono Gendarme activation mode",
      description = "Possible values : enable, skip, reuseReport",
      project = true,
      module = false,
      global = true),
  @Property(
      key = GENDARME_REPORT_KEY,
      defaultValue = GENDARME_REPORT_XML,
      name = "Name of the Mono Gendarme report file",
      description = "Name of the Mono Gendarme report file used when reuse report mode is activated",
      project = true,
      module = false,
      global = true),
  @Property(
      key = GENDARME_CONFIDENCE_KEY,
      defaultValue = "normal+",
      name = "Confidence level used by Mono Gendarme to filter violations",
      description = "Filter defects for the specified confidence levels : [all | [[low | normal | high | total][+|-]] ",
      project = true,
      module = false,
      global = true),
  @Property(
      key = GENDARME_SEVERITY_KEY,
      defaultValue = "all",
      name = "Severity threshold used by Mono Gendarme to filter violations",
      description = "Warning, some rules always fire violation with low severity." +
      		" This severity notion is not linked to the priority notion of Sonar." +
      		" Filter defects for the specified severity levels :" +
      		" [all | [[audit | low | medium | high | critical][+|-]]] ",
      project = true,
      module = false,
      global = true)  
     
})
public class GendarmePlugin implements Plugin {
  public static final String KEY = "gendarme";

  public String getDescription() {
    return "A plugin that collects the Gendarme check results";
  }

  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(GendarmeRuleParserImpl.class);
    list.add(GendarmeRuleMarshallerImpl.class);
    list.add(GendarmeRuleRepository.class);
    list.add(GendarmeSensor.class);
    list.add(GendarmePluginHandler.class);
    return list;
  }

  public String getKey() {
    return KEY;
  }

  public String getName() {
    return "[.net] Gendarme";
  }

  @Override
  public String toString() {
    return getKey();
  }
}