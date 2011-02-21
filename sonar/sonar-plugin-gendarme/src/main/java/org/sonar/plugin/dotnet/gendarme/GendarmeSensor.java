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

package org.sonar.plugin.dotnet.gendarme;

import static org.sonar.plugin.dotnet.gendarme.Constants.*;

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

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.maven.MavenPluginHandler;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.rules.RulesManager;
import org.sonar.plugin.dotnet.core.AbstractDotnetSensor;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;
import org.sonar.plugin.dotnet.core.resource.InvalidResourceException;
import org.xml.sax.InputSource;

public class GendarmeSensor extends AbstractDotnetSensor {
  
  private final static Logger log = LoggerFactory.getLogger(GendarmeSensor.class);
  

  private final RulesManager rulesManager;
  private final RulesProfile profile;
  private final GendarmePluginHandler pluginHandler;
  private final CSharpFileLocator fileLocator;

  /**
   * Constructs a @link{GendarmeSensor}.
   * 
   * @param rulesManager
   */
  public GendarmeSensor(RulesProfile profile, RulesManager rulesManager,
      GendarmePluginHandler pluginHandler, CSharpFileLocator fileLocator) {
    super();
    this.rulesManager = rulesManager;
    this.profile = profile;
    this.pluginHandler = pluginHandler;
    this.fileLocator = fileLocator;
  }

  /**
   * Launches the project analysis/
   * 
   * @param project
   * @param context
   */
  @Override
  public void analyse(Project project, SensorContext context) {
    final String reportFileName;
    if (GENDARME_REUSE_MODE.equals(getGendarmeMode(project))) {
      reportFileName = project.getConfiguration().getString(GENDARME_REPORT_KEY);
      log.warn("Using reuse report mode for Mono Gendarme");
      log.warn("Mono Gendarme profile settings may not have been taken in account");
    } else {
      reportFileName = GENDARME_REPORT_XML;
    }
    
    File dir = getReportsDirectory(project);
    File report = new File(dir, reportFileName);

    // We generate the transformer
    File transformedReport = transformReport(report, dir);
    if (transformedReport == null) {
      return;
    }
    GendarmeResultParser parser 
      = new GendarmeResultParser(project, context, rulesManager, profile, fileLocator);
    try {
      parser.parse(transformedReport);
    } catch (InvalidResourceException ex) {
      log.warn("C# file not referenced in the solution", ex);
    }
  }

  /**
   * Transforms the report to a usable format.
   * 
   * @param report
   * @param dir
   * @return
   */
  private File transformReport(File report, File dir) {
    try {
      ClassLoader contextClassLoader = Thread.currentThread()
        .getContextClassLoader();
      InputStream stream = contextClassLoader
        .getResourceAsStream(GENDARME_TRANSFO_XSL);
      if (stream==null) {
        // happens with sonar2.3 classloader mechanism
        stream = getClass().getClassLoader()
          .getResourceAsStream(GENDARME_TRANSFO_XSL);
      }
      Source xslSource = new SAXSource(new InputSource(stream));
      Templates templates = TransformerFactory.newInstance().newTemplates(
          xslSource);
      Transformer transformer = templates.newTransformer();

      // We open the report to be processed
      Source xmlSource = new StreamSource(report);

      File processedReport = new File(dir, GENDARME_PROCESSED_REPORT_XML);
      processedReport.delete();
      Result result = new StreamResult(new FileOutputStream(processedReport));
      transformer.transform(xmlSource, result);
      return processedReport;
    } catch (Exception exc) {
      log.warn("Error during the processing of the Gendarme report for Sonar",
          exc);
    }
    return null;
  }

  /**
   * @param project
   * @return
   */
  @Override
  public MavenPluginHandler getMavenPluginHandler(Project project) {
    String mode = getGendarmeMode(project);
    final MavenPluginHandler pluginHandlerReturned;
    if (GENDARME_DEFAULT_MODE.equalsIgnoreCase(mode)) {
      pluginHandlerReturned = pluginHandler;
    } else {
      pluginHandlerReturned = null;
    }
    return pluginHandlerReturned;
  }
  
  @Override
  public boolean shouldExecuteOnProject(Project project) {
    String mode = getGendarmeMode(project);
    return super.shouldExecuteOnProject(project) && !GENDARME_SKIP_MODE.equalsIgnoreCase(mode);
  }

  private String getGendarmeMode(Project project) {
    String mode = project.getConfiguration().getString(GENDARME_MODE_KEY, GENDARME_DEFAULT_MODE);
    return mode;
  }

}
