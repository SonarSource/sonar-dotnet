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

/*
 * Created on May 19, 2009
 */
package org.sonar.plugins.csharp.stylecop.profiles.utils;

import java.io.Reader;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;

import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpression;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;

/**
 * Parses StyleCop rules configuration file.
 * 
 * @author Jose CHILLAN May 19, 2009
 */
public final class StyleCopRuleParser {

  private static final Logger LOG = LoggerFactory.getLogger(StyleCopRuleParser.class);

  private StyleCopRuleParser() {
  }

  /**
   * Parses the context of StyleCop rules.
   * 
   * @param xml
   * @return
   */
  public static List<StyleCopRule> parse(String xml) {
    StringReader stringReader = new StringReader(xml);
    return parse(stringReader);
  }

  /**
   * @param reader
   * @return
   */
  public static List<StyleCopRule> parse(Reader reader) {
    InputSource source = new InputSource(reader);
    XPathFactory factory = XPathFactory.newInstance();
    XPath xpath = factory.newXPath();
    List<StyleCopRule> result = new ArrayList<StyleCopRule>();

    try {
      XPathExpression expression = xpath.compile("//Rule");
      NodeList nodes = (NodeList) expression.evaluate(source, XPathConstants.NODESET);
      int count = nodes.getLength();
      for (int idxRule = 0; idxRule < count; idxRule++) {
        Element ruleElement = (Element) nodes.item(idxRule);
        Element analyzerElement = (Element) ruleElement.getParentNode().getParentNode();
        String ruleName = ruleElement.getAttribute("Name");
        String priority = ruleElement.getAttribute("SonarPriority");

        StyleCopRule rule = new StyleCopRule();
        NodeList elements = ruleElement.getElementsByTagName("BooleanProperty");
        boolean active = true;
        int countBoolean = elements.getLength();
        for (int idxElement = 0; idxElement < countBoolean; idxElement++) {
          Element booleanElement = (Element) elements.item(idxElement);
          String booleanName = booleanElement.getAttribute("Name");
          if ("Enabled".equals(booleanName)) {
            String activeStr = booleanElement.getTextContent();
            active = !activeStr.toLowerCase().contains("false");
          }
        }
        String analyzerId = analyzerElement.getAttribute("AnalyzerId");
        String category = StringUtils.removeEnd(StringUtils.substringAfterLast(analyzerId, "."), "Rules");
        rule.setAnalyzerId(analyzerId);
        rule.setName(ruleName);
        rule.setPriority(priority);
        rule.setEnabled(active);
        rule.setCategory(category);
        result.add(rule);
      }
    } catch (XPathExpressionException e) {
      LOG.debug("Xpath error un stylecop report", e);
    }

    return result;
  }

}
