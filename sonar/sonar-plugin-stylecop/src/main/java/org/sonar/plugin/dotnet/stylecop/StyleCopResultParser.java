/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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
 * Created on Sep 24, 2009
 */
package org.sonar.plugin.dotnet.stylecop;

import java.io.File;
import java.text.ParseException;
import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import org.apache.commons.lang.StringUtils;
import org.apache.maven.dotnet.commons.GeneratedCodeFilter;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulesManager;
import org.sonar.api.rules.Violation;
import org.sonar.api.utils.ParsingUtils;
import org.sonar.plugin.dotnet.core.AbstractXmlParser;
import org.sonar.plugin.dotnet.core.resource.CSharpFile;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;
import org.sonar.plugin.dotnet.core.resource.InvalidResourceException;
import org.w3c.dom.Element;

/**
 * Parses the Style cop violations reports.
 * 
 * @author Jose CHILLAN Sep 24, 2009
 */
public class StyleCopResultParser extends AbstractXmlParser {
  private final static Logger log = LoggerFactory
      .getLogger(StyleCopResultParser.class);

  private final Project project;
  private final SensorContext context;
  private final RulesManager rulesManager;
  private final RulesProfile profile;
  private final CSharpFileLocator fileLocator; 

  /**
   * Constructs a @link{FxCopResultParser}.
   * 
   * @param project
   * @param context
   * @param rulesManager
   * @param profile
   */
  public StyleCopResultParser(Project project, SensorContext context,
      RulesManager rulesManager, RulesProfile profile, CSharpFileLocator fileLocator) {
    super();
    this.project = project;
    this.context = context;
    this.rulesManager = rulesManager;
    this.profile = profile;
    this.fileLocator = fileLocator;
  }

  /**
   * Parses a processed violation file.
   * 
   * @param stream
   */
  public void parse(File file) {
    List<Element> issues = extractElements(file, "//issue");
    // We add each issue
    for (Element issueElement : issues) {
      String filePath = getNodeContent(issueElement, "file");
      String key = getNodeContent(issueElement, "key");
      String message = getNodeContent(issueElement, "message");
      String lineNumber = getNodeContent(issueElement, "line");
      
      final Resource<?> resource;
      if (StringUtils.isEmpty(filePath)) {
        log.debug("violation without file path");
        resource = null;
      } else {
        resource = fileLocator.getResource(project, filePath);
        if (resource == null) {
          log.debug("violation on an excluded file {}", file);
          continue;
        }
      }
      
      Integer line = getIntValue(lineNumber);
      Rule rule = rulesManager.getPluginRule(StyleCopPlugin.KEY, key);
      if (rule == null) {
        // Skips the non registered rules
        log.debug("violation found for an unknown '{}' rule", key);
        continue;
      }
      
      ActiveRule activeRule = profile.getActiveRule(StyleCopPlugin.KEY, key);
      Violation violation = new Violation(rule, resource);
      violation.setLineId(line);
      violation.setMessage(message);
      if (activeRule != null) {
        // We copy the priority
        violation.setPriority(activeRule.getPriority());
      }
      // We store the violation
      context.saveViolation(violation);
    }
  }

  /**
   * Extracts the line number.
   * 
   * @param lineStr
   * @return
   */
  protected Integer getIntValue(String lineStr) {
    if (StringUtils.isBlank(lineStr)) {
      return null;
    }
    try {
      return (int) ParsingUtils.parseNumber(lineStr);
    } catch (ParseException ignore) {
      if (log.isDebugEnabled()) {
        log.debug("int parsing error" + lineStr, ignore);
      }
      return null;
    }
  }
}
