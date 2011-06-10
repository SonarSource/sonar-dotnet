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

package org.sonar.plugins.csharp.gendarme.profiles;

import java.io.IOException;
import java.io.Writer;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.sonar.api.profiles.ProfileExporter;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ActiveRuleParam;
import org.sonar.api.rules.RulePriority;
import org.sonar.api.utils.SonarException;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.gendarme.GendarmeConstants;

/**
 * Class that allows to export a Sonar profile into a Gendarme rule definition file.
 */
public class GendarmeProfileExporter extends ProfileExporter {

  public GendarmeProfileExporter() {
    super(GendarmeConstants.REPOSITORY_KEY, GendarmeConstants.PLUGIN_NAME);
    setSupportedLanguages(CSharpConstants.LANGUAGE_KEY);
    setMimeType("application/xml");
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void exportProfile(RulesProfile profile, Writer writer) {
    try {
      printRules(writer, profile.getActiveRulesByRepository(GendarmeConstants.REPOSITORY_KEY));
    } catch (IOException e) {
      throw new SonarException("Error while generating the Gendarme profile to export: " + profile, e);
    }
  }

  public void printRules(Writer writer, List<ActiveRule> rules) throws IOException {
    writer.append("<gendarme>\n");

    marshall(writer, rules, null);

    RulePriority[] priorities = RulePriority.values();
    for (RulePriority rulePriority : priorities) {
      marshall(writer, rules, rulePriority);
    }

    writer.append("</gendarme>");
  }

  private void marshall(Writer writer, List<ActiveRule> rules, RulePriority priority) throws IOException {
    Map<String, List<ActiveRule>> assemblyRulesMap = new HashMap<String, List<ActiveRule>>();

    boolean assemblyRulesMapEmpty = true;
    for (ActiveRule activeRule : rules) {

      if (priority != null && priority != activeRule.getSeverity()) {
        continue;
      }

      String key = activeRule.getConfigKey();
      String assembly = StringUtils.substringAfter(key, "@");
      List<ActiveRule> assemblyRules = assemblyRulesMap.get(assembly);
      if (assemblyRules == null) {
        assemblyRules = new ArrayList<ActiveRule>();
        assemblyRulesMap.put(assembly, assemblyRules);
        assemblyRulesMapEmpty = false;
      }
      assemblyRules.add(activeRule);
    }

    if ( !assemblyRulesMapEmpty) {
      if (priority == null) {
        writer.append("    <ruleset name=\"default\">\n");
      } else {
        writer.append("    <ruleset name=\"" + priority.name().toLowerCase() + "\">\n");
      }

      for (Map.Entry<String, List<ActiveRule>> entry : assemblyRulesMap.entrySet()) {
        writer.append("        <rules include=\"");
        appendRuleList(writer, entry.getValue());
        writer.append("\" from=\"");
        writer.append(entry.getKey());
        writer.append("\">\n");
        appendRuleParams(writer, entry.getValue());
        writer.append("        </rules>\n");
      }
      writer.append("    </ruleset>\n");
    }
  }

  private void appendRuleParams(Writer writer, List<ActiveRule> assemblyRules) throws IOException {
    for (ActiveRule activeRule : assemblyRules) {
      List<ActiveRuleParam> params = activeRule.getActiveRuleParams();
      for (ActiveRuleParam param : params) {
        String key = activeRule.getConfigKey();
        String ruleName = StringUtils.substringBefore(key, "@");
        String propertyName = param.getRuleParam().getKey();
        String propertyValue = param.getValue();
        writer.append("            <parameter rule=\"");
        writer.append(ruleName);
        writer.append("\" property=\"");
        writer.append(propertyName);
        writer.append("\" value=\"");
        writer.append(propertyValue);
        writer.append("\" />\n");
      }
    }
  }

  private void appendRuleList(Writer writer, List<ActiveRule> rules) throws IOException {
    List<String> ruleNames = new ArrayList<String>();
    for (ActiveRule activeRule : rules) {
      String key = activeRule.getConfigKey();
      String ruleName = StringUtils.substringBefore(key, "@");
      ruleNames.add(ruleName);
    }
    writer.append(StringUtils.join(ruleNames, " | "));
  }

}
