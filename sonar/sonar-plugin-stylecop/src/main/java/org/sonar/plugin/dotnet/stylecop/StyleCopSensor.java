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
 *
 */
package org.sonar.plugin.dotnet.stylecop;

import static org.sonar.plugin.dotnet.stylecop.Constants.*;

import java.io.File;
import java.io.InputStream;
import java.net.MalformedURLException;
import java.net.URL;

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
import org.xml.sax.InputSource;

/**
 * Extracts the data of a StyleCop report and store them in Sonar.
 * 
 * @author Jose CHILLAN Apr 6, 2010
 */
public class StyleCopSensor extends AbstractDotnetSensor {
  private final static Logger log = LoggerFactory
      .getLogger(StyleCopSensor.class);
  

  private RulesManager rulesManager;
  private RulesProfile profile;
  private StyleCopPluginHandler pluginHandler;

  /**
   * Constructs a @link{FxCopCollector}.
   * 
   * @param rulesManager
   */
  public StyleCopSensor(RulesManager rulesManager,
      StyleCopPluginHandler pluginHandler, RulesProfile profile) {
    super();
    this.rulesManager = rulesManager;
    this.pluginHandler = pluginHandler;
    this.profile = profile;
  }

  /**
   * @param project
   * @param context
   */
  @Override
  public void analyse(Project project, SensorContext context) {
    File report = findReport(project, STYLECOP_REPORT_NAME);
    File dir = getReportsDirectory(project);

    // We generate the transformer
    File transformedReport = transformReport(report, dir);
    if (transformedReport == null) {
      return;
    }
    StyleCopResultParser parser = new StyleCopResultParser(project, context,
        rulesManager, profile);
    try {
      URL fileURL = transformedReport.toURI().toURL();
      parser.parse(fileURL);
    } catch (MalformedURLException e) {
      log.debug("Error while parsing the file: {}\n{}", report, e);
    }
  }

  /**
   * @param project
   * @return
   */
  @Override
  public MavenPluginHandler getMavenPluginHandler(Project project) {
    return pluginHandler;
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
      // ClassLoader contextClassLoader = this.getClass().getClassLoader();
      // TODO MIGRATION23
      ClassLoader contextClassLoader = Thread.currentThread()
          .getContextClassLoader();
      InputStream stream = contextClassLoader
          .getResourceAsStream(STYLECOP_TRANSFO_XSL);
      Source xslSource = new SAXSource(new InputSource(stream));
      Templates templates = TransformerFactory.newInstance().newTemplates(
          xslSource);
      Transformer transformer = templates.newTransformer();

      // We open the report to be processed
      Source xmlSource = new StreamSource(report);

      File processedReport = new File(dir, STYLECOP_PROCESSED_REPORT_XML);
      processedReport.delete();
      Result result = new StreamResult(processedReport);
      transformer.transform(xmlSource, result);
      return processedReport;
    } catch (Exception exc) {
      log.warn(
          "Error during the transformation of the StyleCop report for Sonar",
          exc);
    }
    return null;
  }

}
