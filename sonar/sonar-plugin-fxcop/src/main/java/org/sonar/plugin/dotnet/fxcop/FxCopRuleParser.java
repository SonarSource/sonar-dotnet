/**
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

/*
 * Created on May 7, 2009
 *
 */
package org.sonar.plugin.dotnet.fxcop;

import java.io.Reader;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

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
public class FxCopRuleParser {
	
	private final static Logger log = LoggerFactory.getLogger(FxCopRuleParser.class);
	
  /**
   * Parses the context of FXCop rules.
   * 
   * @param xml
   * @return
   */
  public static List<FxCopRule> parse(String xml) {
    StringReader stringReader = new StringReader(xml);
    List<FxCopRule> result = parse(stringReader);
    return result;
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
    Pattern pattern = Pattern.compile("(\\w+)Rules");

    try {
      XPathExpression expression = xpath.compile("//Rule");
      NodeList nodes = (NodeList) expression.evaluate(source,
          XPathConstants.NODESET);
      int count = nodes.getLength();
      // For each rule we extract the elements
      for (int idxRule = 0; idxRule < count; idxRule++) {
        Element ruleElement = (Element) nodes.item(idxRule);
        Element parent = (Element) ruleElement.getParentNode();
        String scopeName = parent.getAttribute("Name");
        Matcher matcher = pattern.matcher(scopeName);
        String category = "Default";
        if (matcher.find()) {
          category = matcher.group(1);
        }

        FxCopRule rule = new FxCopRule();
        String ruleName = ruleElement.getAttribute("Name");
        String active = ruleElement.getAttribute("Enabled");
        rule.setName(ruleName);
        rule.setEnabled(active.toLowerCase().contains("true"));
        rule.setCategory(category);
        rule.setFileName(scopeName);
        result.add(rule);
      }
    } catch (XPathExpressionException e) {
      // should not occur
    	log.error("xpath exception while parsing fxcop config file", e);
    }
    return result;
  }

}
