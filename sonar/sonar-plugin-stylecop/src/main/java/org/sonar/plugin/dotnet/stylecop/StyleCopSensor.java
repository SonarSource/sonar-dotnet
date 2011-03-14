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
 * Created on May 19, 2009
 *
 */
package org.sonar.plugin.dotnet.stylecop;

import static org.sonar.plugin.dotnet.stylecop.Constants.*;

import java.io.File;

import java.text.ParseException;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.maven.MavenPluginHandler;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulesManager;
import org.sonar.api.rules.Violation;
import org.sonar.api.utils.ParsingUtils;
import org.sonar.plugin.dotnet.core.AbstractDotnetSensor;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;
import org.sonar.plugin.dotnet.stylecop.stax.StyleCopResultStaxParser;

/**
 * Extracts the data of a StyleCop report and store them in Sonar.
 * 
 * @author Jose CHILLAN Apr 6, 2010
 */
public class StyleCopSensor extends AbstractDotnetSensor {

  private final static Logger log = LoggerFactory.getLogger(StyleCopSensor.class);


  private final RulesManager rulesManager;
  private final StyleCopResultStaxParser styleCopResultStaxParser;
  private final RulesProfile profile;
  private final StyleCopPluginHandler pluginHandler;
  private final CSharpFileLocator fileLocator;

  /**
   * Constructs a @link{StyleCopSensor}.
   * 
   * @param rulesManager
   */
  public StyleCopSensor(RulesManager rulesManager, StyleCopResultStaxParser styleCopResultStaxParser,
      StyleCopPluginHandler pluginHandler, RulesProfile profile, CSharpFileLocator fileLocator) {
    super();
    this.rulesManager = rulesManager;
    this.styleCopResultStaxParser = styleCopResultStaxParser;
    this.pluginHandler = pluginHandler;
    this.profile = profile;
    this.fileLocator = fileLocator;
  }

  /**
   * @param project
   * @param context
   */
  @Override
  public void analyse(Project project, SensorContext context) {
    final String reportFileName;
    if (STYLECOP_REUSE_MODE.equals(getStyleCopMode(project))) {
      reportFileName = project.getConfiguration().getString(STYLECOP_REPORT_KEY);
      log.warn("Using reuse report mode for StyleCop");
      log.warn("WARNING : StyleCop rules profile settings may not have been taken into account");
    } else {
      reportFileName = STYLECOP_REPORT_NAME;
    }

    File dir = getReportsDirectory(project);
    File report = new File(dir, reportFileName);

    List<StyleCopViolation> violations = styleCopResultStaxParser.parse(report);
    for (StyleCopViolation styleCopViolation : violations) {

      final Resource<?> resource;

      String lineNumber = styleCopViolation.getLineNumber();
      String filePath = styleCopViolation.getFilePath();
      String key = styleCopViolation.getKey();
      String message = styleCopViolation.getMessage();

      if (StringUtils.isEmpty(filePath)) {
        log.debug("violation without file path");
        resource = null;
      }
      else {
        resource = fileLocator.getResource(project, filePath);
        if (resource == null) {
          log.info("violation on an excluded file '{}'", filePath);
          continue;
        }
      }

      Rule rule = rulesManager.getPluginRule(StyleCopPlugin.KEY, key);
      if (rule == null) {
        // Skips the non registered rules
        log.info("violation found for an unknown '{}' rule", key);
        continue;
      }

      log.debug("Retrieving active rule corresponding to the violation key {}", key);
      ActiveRule activeRule = profile.getActiveRule(StyleCopPlugin.KEY, key);
      Violation violation = new Violation(rule, resource);
      Integer line = getIntValue(lineNumber);

      violation.setLineId(line);
      violation.setMessage(message);
      if (activeRule != null) {
        // We copy the priority
        violation.setPriority(activeRule.getPriority());
      }
      // We save the violation
      context.saveViolation(violation);
    }

  }

  /**
   * @param project
   * @return
   */
  @Override
  public MavenPluginHandler getMavenPluginHandler(Project project) {
    String mode = getStyleCopMode(project);
    final MavenPluginHandler pluginHandlerReturned;
    if (STYLECOP_DEFAULT_MODE.equalsIgnoreCase(mode)) {
      pluginHandlerReturned = pluginHandler;
    } else {
      pluginHandlerReturned = null;
    }
    return pluginHandlerReturned;
  }

  @Override
  public boolean shouldExecuteOnProject(Project project) {
    String mode = getStyleCopMode(project);
    return super.shouldExecuteOnProject(project) && !STYLECOP_SKIP_MODE.equalsIgnoreCase(mode);
  }

  private String getStyleCopMode(Project project) {
    return project.getConfiguration().getString(STYLECOP_MODE_KEY, STYLECOP_DEFAULT_MODE);
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
