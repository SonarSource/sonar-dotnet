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

package org.sonar.plugins.csharp.fxcop.profiles.utils;

import java.io.Reader;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;

import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpression;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;

/**
 * Parser the FXCop rules to populate sonar.
 * 
 * @author Jose CHILLAN May 7, 2009
 */
public final class FxCopRuleParser {

  private static final Logger LOG = LoggerFactory.getLogger(FxCopRuleParser.class);

  private FxCopRuleParser() {
  }

  /**
   * Parses the context of FXCop rules.
   * 
   * @param xml
   * @return
   */
  public static List<FxCopRule> parse(String xml) {
    StringReader stringReader = new StringReader(xml);
    return parse(stringReader);
  }

  /**
   * Parses a FxCop rule file
   * 
   * @param reader
   * @return
   */
  public static List<FxCopRule> parse(Reader reader) {
    InputSource source = new InputSource(reader);
    XPathFactory factory = XPathFactory.newInstance();
    XPath xpath = factory.newXPath();
    List<FxCopRule> result = new ArrayList<FxCopRule>();

    try {
      XPathExpression expression = xpath.compile("//Rule");
      NodeList nodes = (NodeList) expression.evaluate(source, XPathConstants.NODESET);
      int count = nodes.getLength();
      // For each rule we extract the elements
      for (int idxRule = 0; idxRule < count; idxRule++) {
        Element ruleElement = (Element) nodes.item(idxRule);
        Element parent = (Element) ruleElement.getParentNode();
        String scopeName = parent.getAttribute("Name");

        FxCopRule rule = new FxCopRule();
        String ruleName = ruleElement.getAttribute("Name");
        String active = ruleElement.getAttribute("Enabled");
        String priority = ruleElement.getAttribute("SonarPriority");
        rule.setName(ruleName);
        rule.setEnabled(active.toLowerCase().contains("true"));
        rule.setFileName(scopeName);
        rule.setPriority(priority);
        result.add(rule);
      }
    } catch (XPathExpressionException e) {
      // should not occur
      LOG.error("xpath exception while parsing fxcop config file", e);
    }
    return result;
  }

}
