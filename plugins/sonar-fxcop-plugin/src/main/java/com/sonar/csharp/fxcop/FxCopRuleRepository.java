/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import org.sonar.api.platform.ServerFileSystem;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleRepository;
import org.sonar.api.rules.XMLRuleParser;

/**
 * Loads and generated the FXCop rules configuration file.
 */
public class FxCopRuleRepository extends RuleRepository {

  // for user extensions
  private ServerFileSystem fileSystem;
  private XMLRuleParser xmlRuleParser;

  public FxCopRuleRepository(ServerFileSystem fileSystem, XMLRuleParser xmlRuleParser) {
    super(Constants.REPOSITORY_KEY, Constants.LANGUAGE_KEY);
    setName(Constants.REPOSITORY_NAME);
    this.fileSystem = fileSystem;
    this.xmlRuleParser = xmlRuleParser;
  }

  @Override
  public List<Rule> createRules() {
    List<Rule> rules = new ArrayList<Rule>();
    rules.addAll(xmlRuleParser.parse(getClass().getResourceAsStream("/com/sonar/csharp/fxcop/rules/rules.xml")));
    for (File userExtensionXml : fileSystem.getExtensions(Constants.REPOSITORY_KEY, "xml")) {
      rules.addAll(xmlRuleParser.parse(userExtensionXml));
    }
    return rules;
  }
}