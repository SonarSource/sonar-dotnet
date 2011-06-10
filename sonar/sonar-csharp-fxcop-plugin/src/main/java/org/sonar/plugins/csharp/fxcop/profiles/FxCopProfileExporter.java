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

package org.sonar.plugins.csharp.fxcop.profiles;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.Writer;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.commons.io.IOUtils;
import org.apache.commons.lang.StringEscapeUtils;
import org.apache.commons.lang.StringUtils;
import org.sonar.api.profiles.ProfileExporter;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulePriority;
import org.sonar.api.utils.SonarException;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.fxcop.FxCopConstants;
import org.sonar.plugins.csharp.fxcop.profiles.utils.FxCopRule;

/**
 * Class that allows to export a Sonar profile into a FxCop rule definition file.
 */
public class FxCopProfileExporter extends ProfileExporter {

  private static final String FXCOP_PROJECT_FILE_HEADER = "fxcop-project-file-header.txt";
  private static final String FXCOP_PROJECT_FILE_FOOTER = "fxcop-project-file-footer.txt";

  public FxCopProfileExporter() {
    super(FxCopConstants.REPOSITORY_KEY, FxCopConstants.PLUGIN_NAME);
    setSupportedLanguages(CSharpConstants.LANGUAGE_KEY);
    setMimeType("application/xml");
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void exportProfile(RulesProfile profile, Writer writer) {
    try {
      printIntoWriter(writer, FXCOP_PROJECT_FILE_HEADER);

      printRules(profile, writer);

      printIntoWriter(writer, FXCOP_PROJECT_FILE_FOOTER);
    } catch (IOException e) {
      throw new SonarException("Error while generating the FxCop profile to export: " + profile, e);
    }
  }

  private void printRules(RulesProfile profile, Writer writer) throws IOException {
    List<ActiveRule> activeRules = profile.getActiveRulesByRepository(FxCopConstants.REPOSITORY_KEY);
    List<FxCopRule> rules = transformIntoFxCopRules(activeRules);

    // We group the rules by RuleFile names
    Map<String, List<FxCopRule>> rulesByFile = groupFxCopRulesByRuleFileName(rules);
    // And then print out each rule
    for (String fileName : rulesByFile.keySet()) {
      printRuleFile(writer, rulesByFile, fileName);
    }
  }

  private void printRuleFile(Writer writer, Map<String, List<FxCopRule>> rulesByFile, String fileName) throws IOException {
    writer.append("            <RuleFile AllRulesEnabled=\"False\" Enabled=\"True\" Name=\"");
    StringEscapeUtils.escapeXml(writer, fileName);
    writer.append("\">\n");
    for (FxCopRule fxCopRule : rulesByFile.get(fileName)) {
      printRule(writer, fxCopRule);
    }
    writer.append("            </RuleFile>\n");
  }

  private void printRule(Writer writer, FxCopRule fxCopRule) throws IOException {
    writer.append("                <Rule Enabled=\"True\" Name=\"");
    StringEscapeUtils.escapeXml(writer, fxCopRule.getName());
    writer.append("\" SonarPriority=\"");
    StringEscapeUtils.escapeXml(writer, fxCopRule.getPriority());
    writer.append("\"/>\n");
  }

  private List<FxCopRule> transformIntoFxCopRules(List<ActiveRule> activeRulesByPlugin) {
    List<FxCopRule> result = new ArrayList<FxCopRule>();

    for (ActiveRule activeRule : activeRulesByPlugin) {
      // Extracts the rule's information
      Rule rule = activeRule.getRule();
      String configKey = rule.getConfigKey();
      String fileName = StringUtils.substringAfter(configKey, "@");
      String name = StringUtils.substringBefore(configKey, "@");

      // Creates the FxCop rule
      FxCopRule fxCopRule = new FxCopRule();
      fxCopRule.setEnabled(true);
      fxCopRule.setFileName(fileName);
      fxCopRule.setName(name);

      RulePriority priority = activeRule.getSeverity();
      if (priority != null) {
        fxCopRule.setPriority(priority.name().toLowerCase());
      }

      result.add(fxCopRule);
    }
    return result;
  }

  private Map<String, List<FxCopRule>> groupFxCopRulesByRuleFileName(List<FxCopRule> rules) {
    Map<String, List<FxCopRule>> rulesByFile = new HashMap<String, List<FxCopRule>>();
    for (FxCopRule fxCopRule : rules) {
      String fileName = fxCopRule.getFileName();
      List<FxCopRule> rulesList = rulesByFile.get(fileName);
      if (rulesList == null) {
        rulesList = new ArrayList<FxCopRule>();
        rulesByFile.put(fileName, rulesList);
      }
      rulesList.add(fxCopRule);
    }
    return rulesByFile;
  }

  private void printIntoWriter(Writer writer, String fileName) throws IOException {
    BufferedReader reader = new BufferedReader(new InputStreamReader(FxCopProfileExporter.class.getResourceAsStream(fileName)));
    try {
      String line = reader.readLine();
      while (line != null) {
        writer.append(line);
        writer.append("\n");
        line = reader.readLine();
      }
      reader.close();
    } finally {
      IOUtils.closeQuietly(reader);
    }
  }

}
