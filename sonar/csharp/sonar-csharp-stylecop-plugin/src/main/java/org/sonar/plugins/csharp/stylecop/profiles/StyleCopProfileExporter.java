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

import com.google.common.collect.Maps;
import org.apache.commons.lang.StringEscapeUtils;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
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
import org.w3c.dom.Attr;
import org.w3c.dom.CharacterData;
import org.w3c.dom.Element;
import org.w3c.dom.NamedNodeMap;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;

import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpression;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.Writer;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * Class that allows to export a Sonar profile into a StyleCop rule definition file.
 */
public class StyleCopProfileExporter extends ProfileExporter {

  private static final Logger LOG = LoggerFactory.getLogger(StyleCopProfileExporter.class);

  public static class RegularStyleCopProfileExporter extends StyleCopProfileExporter {
    public RegularStyleCopProfileExporter() {
      super(StyleCopConstants.REPOSITORY_KEY, StyleCopConstants.REPOSITORY_NAME);
    }
  }

  protected StyleCopProfileExporter(String repositoryKey, String repositoryName) {
    super(repositoryKey, repositoryName);
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

      printRules(getKey(), profile, writer, null);

      printEndOfFile(writer);
    } catch (IOException e) {
      throw new SonarException("Error while generating the StyleCop profile to export: " + profile, e);
    }
  }

  public void exportProfile(RulesProfile profile, Writer writer, File analyzersSettings) {
    try {
      printStartOfFile(writer);

      printRules(getKey(), profile, writer, analyzersSettings);

      printEndOfFile(writer);
    } catch (IOException e) {
      throw new SonarException("Error while generating the StyleCop profile to export: " + profile, e);
    }
  }

  private void printStartOfFile(Writer writer) throws IOException {
    writer.append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n");
    writer.append("  <StyleCopSettings Version=\"105\">\n");
    writer.append("    <GlobalSettings>\n");
    writer.append("      <BooleanProperty Name=\"WriteCache\">False</BooleanProperty>\n");
    writer.append("      <BooleanProperty Name=\"AutoCheckForUpdate\">False</BooleanProperty>\n");
    writer.append("      <IntegerProperty Name=\"MaxViolationCount\">-1</IntegerProperty>\n");
    writer.append("    </GlobalSettings>\n");
    writer.append("    <Analyzers>\n");
  }

  private void printEndOfFile(Writer writer) throws IOException {
    writer.append("    </Analyzers>\n");
    writer.append("</StyleCopSettings>");
  }

  private void printRules(String repositoryKey, RulesProfile profile, Writer writer, File analyzersSettings) throws IOException {
    List<ActiveRule> activeRules = profile.getActiveRulesByRepository(repositoryKey);
    List<StyleCopRule> rules = transformIntoStyleCopRules(activeRules);

    // We group the rules by analyzer/assembly names
    Map<String, List<StyleCopRule>> rulesByAnalyzer = groupStyleCopRulesByAnalyzer(rules);
    // We parse the settings to get settings by analyzers
    Map<String, String> settingsByAnalyzer = parseAnalyzerSettings(analyzersSettings);
    // And then print out each rule
    for (String analyzerId : rulesByAnalyzer.keySet()) {
      String analyzerSettings = settingsByAnalyzer.get(analyzerId);
      printRuleFile(writer, rulesByAnalyzer, analyzerId, analyzerSettings);
    }
  }

  private Map<String, String> parseAnalyzerSettings(File analyzersSettings) {
    Map<String, String> result = Maps.newHashMap();
    if (analyzersSettings == null) {
      LOG.info("No custom analyzers settings");
      return result;
    }
    try {
      XPathFactory factory = XPathFactory.newInstance();
      XPath xpath = factory.newXPath();
      XPathExpression expression = xpath.compile("//AnalyzerSettings");
      InputSource inputSource = new InputSource(new FileInputStream(analyzersSettings));

      NodeList nodeList = (NodeList) expression.evaluate(inputSource, XPathConstants.NODESET);
      for (int i = 0; i < nodeList.getLength(); i++) {
        Node settingsNode = nodeList.item(i);
        Element analyzerNode = (Element) settingsNode.getParentNode();
        String analyzerId = analyzerNode.getAttribute("AnalyzerId");
        StringBuilder builder = new StringBuilder();
        writeNode(builder, settingsNode);
        result.put(analyzerId, builder.toString());
      }

    } catch (XPathExpressionException e) {
      throw new SonarException("Wrong StyleCop analyzer settings format", e);
    } catch (FileNotFoundException e) {
      throw new SonarException("StyleCop analyzer settings file not found", e);
    }
    return result;
  }

  private static void writeNode(StringBuilder builder, Node node) {
    switch (node.getNodeType()) {
      case Node.ELEMENT_NODE:
        writeElement(builder, (Element) node);
        break;
      case Node.TEXT_NODE:
      case Node.CDATA_SECTION_NODE:
        builder.append(((CharacterData) node).getData());
        break;
      default:
        LOG.debug("node ignored {}", node);
        break;
    }
  }

  private static void writeElement(StringBuilder builder, Element elt) {
    builder.append("<").append(elt.getTagName());
    NamedNodeMap nm = elt.getAttributes();
    for (int i = 0; i < nm.getLength(); i++) {
      Attr attr = (Attr) nm.item(i);
      builder.append(" ").append(attr.getName()).append("=\"").append(attr.getValue()).append("\"");
    }
    builder.append(">");
    NodeList list = elt.getChildNodes();
    for (int i = 0; i < list.getLength(); i++) {
      writeNode(builder, list.item(i));
    }
    builder.append("</").append(elt.getTagName()).append(">");
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
    // Note that TreeMap is used to guarantee stable order, and thus to pass tests under both JDK 1.5 and JDK 1.6
    Map<String, List<StyleCopRule>> rulesByAnalyzer = Maps.newTreeMap();
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

  private void printRuleFile(Writer writer, Map<String, List<StyleCopRule>> rulesByAnalyzer, String analyzerId, String analyzerSettings) throws IOException {
    writer.append("        <Analyzer AnalyzerId=\"");
    StringEscapeUtils.escapeXml(writer, analyzerId);
    writer.append("\">\n");
    writer.append("            <Rules>\n");
    for (StyleCopRule styleCopRule : rulesByAnalyzer.get(analyzerId)) {
      printRule(writer, styleCopRule);
    }
    writer.append("            </Rules>\n");
    if (StringUtils.isNotEmpty(analyzerSettings)) {
      writer.append("            ").append(analyzerSettings).append("\n");
    }
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
