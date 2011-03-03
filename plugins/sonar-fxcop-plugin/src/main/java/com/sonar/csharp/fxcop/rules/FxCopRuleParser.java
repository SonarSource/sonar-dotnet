/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop.rules;

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
      NodeList nodes = (NodeList) expression.evaluate(source, XPathConstants.NODESET);
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
        String priority = ruleElement.getAttribute("SonarPriority");
        rule.setName(ruleName);
        rule.setEnabled(active.toLowerCase().contains("true"));
        rule.setCategory(category);
        rule.setFileName(scopeName);
        rule.setPriority(priority);
        result.add(rule);
      }
    } catch (XPathExpressionException e) {
      // should not occur
      log.error("xpath exception while parsing fxcop config file", e);
    }
    return result;
  }

}
