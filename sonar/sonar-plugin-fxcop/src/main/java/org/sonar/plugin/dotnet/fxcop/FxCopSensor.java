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
 */
package org.sonar.plugin.dotnet.fxcop;


import static org.sonar.plugin.dotnet.fxcop.Constants.*;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;

import javax.xml.transform.Result;
import javax.xml.transform.Source;
import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.maven.MavenPluginHandler;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.rules.RulesManager;
import org.sonar.plugin.dotnet.core.AbstractDotnetSensor;
import org.sonar.plugin.dotnet.core.resource.InvalidResourceException;
import org.xml.sax.InputSource;

/**
 * Collects the FXCop reporting into sonar.
 * 
 * @author Jose CHILLAN Feb 16, 2010
 */
public class FxCopSensor extends AbstractDotnetSensor {
  private final static Logger log = LoggerFactory.getLogger(FxCopSensor.class);

  private RulesManager rulesManager;
  private RulesProfile profile;
  private FxCopPluginHandler pluginHandler;

  /**
   * Constructs a @link{FxCopCollector}.
   * 
   * @param rulesManager
   */
  public FxCopSensor(RulesProfile profile, RulesManager rulesManager,
      FxCopPluginHandler pluginHandler) {
    super();
    this.rulesManager = rulesManager;
    this.profile = profile;
    this.pluginHandler = pluginHandler;
  }

  /**
   * Launches the project analysis/
   * 
   * @param project
   * @param context
   */
  @Override
  public void analyse(Project project, SensorContext context) {
    
    final String[] reportFileNames;
    if (FXCOP_REUSE_MODE.equals(getFxCopMode(project))) {
      reportFileNames = StringUtils.split(project.getConfiguration().getString(FXCOP_REPORT_KEY),';');
      log.warn("Using reuse report mode for FxCop");
      log.warn("FxCop profile settings may not have been taken in account");
    } else {
      reportFileNames = new String[]{ FXCOP_REPORT_XML, SL_FXCOP_REPORT_XML};
    }
    
    
    File dir = getReportsDirectory(project);
    
    for (String reportFileName : reportFileNames) {
      File report = new File(dir, reportFileName);
      if (report.exists()) {
        log.info("FxCop report found at location {}", report);
        FxCopResultParser parser 
          = new FxCopResultParser(project, context, rulesManager, profile);
        
        parseReport(report, parser, dir);
      } else {
        log.info("No FxCop report found for path {}", report);
      }
    }
  }

  private void parseReport(File report, FxCopResultParser parser, File workDirectory) { 
    // We generate the transformer
    File transformedReport 
      = transformReport(report, workDirectory, report.getName()+FXCOP_PROCESSED_REPORT_SUFFIX);
    if (transformedReport == null) {
      return;
    }
    try {
      parser.parse(transformedReport);
    } catch (InvalidResourceException ex) {
      log.error("C# file not referenced in the solution", ex);
    }
    
  }

  /**
   * Transforms the report to a usable format.
   * 
   * @param report
   * @param dir
   * @return
   */
  private File transformReport(File report, File dir, String targetFileName) {
    try {
      ClassLoader contextClassLoader = Thread.currentThread()
        .getContextClassLoader();
      InputStream stream = contextClassLoader
        .getResourceAsStream(FXCOP_TRANSFO_XSL);
  
      if (stream==null) {
        // happens with sonar2.3 classloader mechanism
        stream = getClass().getClassLoader()
          .getResourceAsStream(FXCOP_TRANSFO_XSL);
      }
    
      Source xslSource = new SAXSource(new InputSource(stream));
      Templates templates = TransformerFactory.newInstance().newTemplates(
          xslSource);
      Transformer transformer = templates.newTransformer();

      // We open the report to be processed
      Source xmlSource = new StreamSource(report);

      File processedReport = new File(dir, targetFileName);
      processedReport.delete();
      Result result = new StreamResult(new FileOutputStream(processedReport));
      transformer.transform(xmlSource, result);
      return processedReport;
    } catch (Exception exc) {
      log.error("Error during the processing of the FxCop report for Sonar", exc);
    }
    return null;
  }

  /**
   * @param project
   * @return
   */
  @Override
  public MavenPluginHandler getMavenPluginHandler(Project project) {
    String mode = getFxCopMode(project);
    final MavenPluginHandler pluginHandlerReturned;
    if (FXCOP_DEFAULT_MODE.equalsIgnoreCase(mode)) {
      pluginHandlerReturned = pluginHandler;
    } else {
      pluginHandlerReturned = null;
    }
    return pluginHandlerReturned;
  }
  
  @Override
  public boolean shouldExecuteOnProject(Project project) {
    String mode = getFxCopMode(project);
    return super.shouldExecuteOnProject(project) && !FXCOP_SKIP_MODE.equalsIgnoreCase(mode);
  }

  private String getFxCopMode(Project project) {
    String mode = project.getConfiguration().getString(FXCOP_MODE_KEY, FXCOP_DEFAULT_MODE);
    return mode;
  }

}