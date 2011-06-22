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

package org.sonar.plugins.csharp.stylecop.profiles;

import java.io.IOException;
import java.io.Writer;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.commons.lang.StringEscapeUtils;
import org.apache.commons.lang.StringUtils;
import org.sonar.api.profiles.ProfileExporter;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulePriority;
import org.sonar.api.rules.XMLRuleParser;
import org.sonar.api.utils.SonarException;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.stylecop.StyleCopConstants;
import org.sonar.plugins.csharp.stylecop.profiles.utils.StyleCopRule;

import com.google.common.collect.Maps;

/**
 * Class that allows to export a Sonar profile into a StyleCop rule definition file.
 */
public class StyleCopProfileExporter extends ProfileExporter {

  public StyleCopProfileExporter() {
    super(StyleCopConstants.REPOSITORY_KEY, StyleCopConstants.REPOSITORY_NAME);
    setSupportedLanguages(CSharpConstants.LANGUAGE_KEY);
    setMimeType("application/xml");
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void exportProfile(RulesProfile profile, Writer writer) {
    try {
      printStartOfFile(writer);

      printRules(profile, writer);

      printEndOfFile(writer);
    } catch (IOException e) {
      throw new SonarException("Error while generating the StyleCop profile to export: " + profile, e);
    }
  }

  private void printStartOfFile(Writer writer) throws IOException {
    writer.append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n");
    writer.append("<StyleCopSettings Version=\"4.3\">\n");
    writer.append("    <Analyzers>\n");
  }

  private void printEndOfFile(Writer writer) throws IOException {
    writer.append("    </Analyzers>\n");
    writer.append("</StyleCopSettings>");
  }

  private void printRules(RulesProfile profile, Writer writer) throws IOException {
    List<ActiveRule> activeRules = profile.getActiveRulesByRepository(StyleCopConstants.REPOSITORY_KEY);
    List<StyleCopRule> rules = transformIntoStyleCopRules(activeRules);

    // We group the rules by RuleFile names
    Map<String, List<StyleCopRule>> rulesByFile = groupStyleCopRulesByAnalyzer(rules);
    // And then print out each rule
    for (String fileName : rulesByFile.keySet()) {
      printRuleFile(writer, rulesByFile, fileName);
    }
  }

  private List<StyleCopRule> transformIntoStyleCopRules(List<ActiveRule> activeRulesByPlugin) {
    List<StyleCopRule> result = new ArrayList<StyleCopRule>();

    Map<String, ActiveRule> activeRuleMap = Maps.newHashMap();
    for (ActiveRule activeRule : activeRulesByPlugin) {
      activeRuleMap.put(activeRule.getRule().getKey(), activeRule);
    }

    List<Rule> initialRules = new XMLRuleParser().parse(StyleCopProfileExporter.class
        .getResourceAsStream("/org/sonar/plugins/csharp/stylecop/rules/rules.xml"));
    for (Rule rule : initialRules) {
      // Extracts the rule's information
      String configKey = rule.getConfigKey();
      String analyzerName = StringUtils.substringBefore(configKey, "#");
      String name = StringUtils.substringAfter(configKey, "#");

      // Creates the StyleCop rule
      StyleCopRule styleCopRule = new StyleCopRule();
      styleCopRule.setAnalyzerId(analyzerName);
      styleCopRule.setName(name);

      ActiveRule activeRule = activeRuleMap.get(rule.getKey());
      if (activeRule != null) {
        styleCopRule.setEnabled(true);
        RulePriority priority = activeRule.getSeverity();
        if (priority != null) {
          styleCopRule.setPriority(priority.name().toLowerCase());
        }
      }

      result.add(styleCopRule);
    }

    return result;
  }

  private Map<String, List<StyleCopRule>> groupStyleCopRulesByAnalyzer(List<StyleCopRule> rules) {
    Map<String, List<StyleCopRule>> rulesByAnalyzer = new HashMap<String, List<StyleCopRule>>();
    for (StyleCopRule styleCopRule : rules) {
      String analyzerId = styleCopRule.getAnalyzerId();
      List<StyleCopRule> rulesList = rulesByAnalyzer.get(analyzerId);
      if (rulesList == null) {
        rulesList = new ArrayList<StyleCopRule>();
        rulesByAnalyzer.put(analyzerId, rulesList);
      }
      rulesList.add(styleCopRule);
    }
    return rulesByAnalyzer;
  }

  private void printRuleFile(Writer writer, Map<String, List<StyleCopRule>> rulesByAnalyzer, String analyzerId) throws IOException {
    writer.append("        <Analyzer AnalyzerId=\"");
    StringEscapeUtils.escapeXml(writer, analyzerId);
    writer.append("\">\n");
    writer.append("            <Rules>\n");
    for (StyleCopRule styleCopRule : rulesByAnalyzer.get(analyzerId)) {
      printRule(writer, styleCopRule);
    }
    writer.append("            </Rules>\n");
    writer.append("        </Analyzer>\n");
  }

  private void printRule(Writer writer, StyleCopRule styleCopRule) throws IOException {
    writer.append("                <Rule Name=\"");
    StringEscapeUtils.escapeXml(writer, styleCopRule.getName());
    writer.append("\" SonarPriority=\"");
    StringEscapeUtils.escapeXml(writer, styleCopRule.getPriority());
    writer.append("\">\n");
    writer.append("                    <RuleSettings>\n");
    writer.append("                        <BooleanProperty Name=\"Enabled\">");
    writer.append(styleCopRule.isEnabled() ? "True" : "False");
    writer.append("</BooleanProperty>\n");
    writer.append("                    </RuleSettings>\n");
    writer.append("                </Rule>\n");
  }

}
