/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2017 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonarsource.dotnet.shared.plugins;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.Reader;
import java.nio.charset.StandardCharsets;
import java.util.LinkedHashMap;
import java.util.Map;
import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamException;
import org.codehaus.staxmate.SMInputFactory;
import org.codehaus.staxmate.in.SMHierarchicCursor;
import org.codehaus.staxmate.in.SMInputCursor;
import org.sonar.api.server.debt.DebtRemediationFunction;
import org.sonar.api.server.rule.RulesDefinition.DebtRemediationFunctions;
import org.sonar.api.server.rule.RulesDefinition.NewRepository;
import org.sonar.api.server.rule.RulesDefinition.NewRule;

/**
 * Copied from sslr-squid-bridge to cut dependency and remove Guava dependency
 */
public class SqaleXmlLoader {

  private NewRepository repository;

  public SqaleXmlLoader(NewRepository repository) {
    this.repository = repository;
  }

  public static void load(NewRepository repository, String xmlResourcePath) {
    new SqaleXmlLoader(repository).loadXmlResource(xmlResourcePath);
  }

  public void loadXmlResource(String resourcePath) {
    try (InputStream is = SqaleXmlLoader.class.getResourceAsStream(resourcePath); Reader reader = new BufferedReader(new InputStreamReader(is, StandardCharsets.UTF_8))) {
      XMLInputFactory xmlFactory = XMLInputFactory.newInstance();
      xmlFactory.setProperty(XMLInputFactory.IS_COALESCING, Boolean.TRUE);
      xmlFactory.setProperty(XMLInputFactory.IS_NAMESPACE_AWARE, Boolean.FALSE);
      // just so it won't try to load DTD in if there's DOCTYPE
      xmlFactory.setProperty(XMLInputFactory.SUPPORT_DTD, Boolean.FALSE);
      xmlFactory.setProperty(XMLInputFactory.IS_VALIDATING, Boolean.FALSE);
      SMInputFactory inputFactory = new SMInputFactory(xmlFactory);
      processRoot(reader, inputFactory);
    } catch (IOException | XMLStreamException e) {
      throw new IllegalArgumentException("Could not read SQALE XML File at " + resourcePath, e);
    }
  }

  private void processRoot(Reader reader, SMInputFactory inputFactory) throws XMLStreamException {
    SMHierarchicCursor rootCursor = inputFactory.rootElementCursor(reader);
    rootCursor.advance();
    SMInputCursor charCursor = rootCursor.childElementCursor("chc");
    while (charCursor.getNext() != null) {
      SMInputCursor subCharCursor = charCursor.childElementCursor("chc");
      while (subCharCursor.getNext() != null) {
        processSubChar(subCharCursor);
      }
    }
  }

  private void processSubChar(SMInputCursor subCharCursor) throws XMLStreamException {
    String subCharName = null;
    SMInputCursor subCharChildCursor = subCharCursor.childElementCursor();
    while (subCharChildCursor.getNext() != null) {
      String childName = subCharChildCursor.getLocalName();
      if ("key".equals(childName)) {
        subCharName = subCharChildCursor.getElemStringValue();
      }
      if ("chc".equals(childName)) {
        processRule(subCharName, subCharChildCursor);
      }
    }
  }

  private void processRule(String subCharName, SMInputCursor ruleCursor) throws XMLStreamException {
    SMInputCursor ruleChildCursor = ruleCursor.childElementCursor();
    String ruleKey = null;
    String remediationFunction = null;
    String remediationFactor = null;
    String offset = null;
    while (ruleChildCursor.getNext() != null) {
      String childName = ruleChildCursor.getLocalName();
      if ("rule-key".equals(childName)) {
        ruleKey = ruleChildCursor.getElemStringValue();
      }
      if ("prop".equals(childName)) {
        Map<String, String> propMap = childrenMap(ruleChildCursor.childElementCursor());
        String key = propMap.get("key");
        if ("remediationFunction".equals(key)) {
          remediationFunction = propMap.get("txt");
        }
        if ("offset".equals(key)) {
          offset = timeValue(propMap);
        }
        if ("remediationFactor".equals(key)) {
          remediationFactor = timeValue(propMap);
        }
      }
    }
    NewRule rule = repository.rule(ruleKey);
    if (rule != null) {
      rule.setDebtSubCharacteristic(subCharName);
      rule.setDebtRemediationFunction(remediationFunction(
        rule.debtRemediationFunctions(), remediationFunction, offset, remediationFactor));
    }
  }

  private static String timeValue(Map<String, String> propMap) {
    String timeUnit = propMap.get("txt");
    if ("mn".equals(timeUnit)) {
      timeUnit = "min";
    }
    String value = propMap.get("val");
    if (value.indexOf('.') > -1) {
      value = value.substring(0, value.indexOf('.'));
    }
    return value + timeUnit;
  }

  private static DebtRemediationFunction remediationFunction(DebtRemediationFunctions functions,
    String remediationFunction, String offset, String remediationFactor) {
    if ("CONSTANT_ISSUE".equalsIgnoreCase(remediationFunction)) {
      return functions.constantPerIssue(offset);
    } else if ("LINEAR".equalsIgnoreCase(remediationFunction)) {
      return functions.linear(remediationFactor);
    } else if ("LINEAR_OFFSET".equalsIgnoreCase(remediationFunction)) {
      return functions.linearWithOffset(remediationFactor, offset);
    }
    return null;
  }

  private static Map<String, String> childrenMap(SMInputCursor cursor) throws XMLStreamException {
    Map<String, String> map = new LinkedHashMap<>();
    while (cursor.getNext() != null) {
      map.put(cursor.getLocalName(), cursor.getElemStringValue());
    }
    return map;
  }

}
