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
 * Created on May 19, 2009
 */
package org.sonar.plugin.dotnet.stylecop;

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
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;

/**
 * Parses StyleCop rules configuration file.
 * @author Jose CHILLAN May 19, 2009
 */
public class StyleCopRuleParser
{

  /**
   * Parses the context of FXCop rules.
   * @param xml
   * @return
   */
  public static List<StyleCopRule> parse(String xml)
  {
    StringReader stringReader = new StringReader(xml);
    List<StyleCopRule> result = parse(stringReader);      
    return result;
  }
  
  /**
   * @param reader
   * @return
   */
  public static List<StyleCopRule> parse(Reader reader)
  {
    InputSource source = new InputSource(reader);
    XPathFactory factory = XPathFactory.newInstance();
    XPath xpath = factory.newXPath();
    List<StyleCopRule> result = new ArrayList<StyleCopRule>();

    try
    {
      XPathExpression expression = xpath.compile("//Rule");
      NodeList nodes = (NodeList) expression.evaluate(source, XPathConstants.NODESET);
      int count = nodes.getLength();
      for (int idxRule = 0; idxRule < count; idxRule++)
      {
        Element ruleElement = (Element) nodes.item(idxRule);
        Element analyzerElement = (Element) ruleElement.getParentNode().getParentNode();
        String ruleName = ruleElement.getAttribute("Name");
        String priority = ruleElement.getAttribute("SonarPriotiry");

        StyleCopRule rule = new StyleCopRule();
        NodeList elements = ruleElement.getElementsByTagName("BooleanProperty");
        boolean active = true;
        int countBoolean = elements.getLength();
        for (int idxElement = 0; idxElement < countBoolean; idxElement++)
        {
          Element booleanElement = (Element) elements.item(idxElement);
          String booleanName = booleanElement.getAttribute("Name");
          if ("Enabled".equals(booleanName))
          {
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
    }
    catch (XPathExpressionException e)
    {
      // Nothing
    }

    return result;
  }

}
