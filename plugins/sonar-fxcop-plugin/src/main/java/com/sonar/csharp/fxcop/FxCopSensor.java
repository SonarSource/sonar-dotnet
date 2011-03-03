/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop;

import static com.sonar.csharp.fxcop.Constants.FXCOP_PROCESSED_REPORT_SUFFIX;
import static com.sonar.csharp.fxcop.Constants.FXCOP_REPORT_XML;
import static com.sonar.csharp.fxcop.Constants.FXCOP_TRANSFO_XSL;
import static com.sonar.csharp.fxcop.Constants.SL_FXCOP_REPORT_XML;

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
import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.maven.DependsUponMavenPlugin;
import org.sonar.api.batch.maven.MavenPluginHandler;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.rules.RulesManager;
import org.xml.sax.InputSource;

import com.sonar.csharp.fxcop.maven.FxCopPluginHandler;
import com.sonar.csharp.fxcop.utils.FxCopResultParser;

/**
 * Collects the FXCop reporting into sonar.
 */
public class FxCopSensor implements Sensor, DependsUponMavenPlugin {

  private final static Logger log = LoggerFactory.getLogger(FxCopSensor.class);
  private RulesManager rulesManager;
  private RulesProfile profile;
  private FxCopPluginHandler pluginHandler;

  public FxCopSensor(RulesProfile profile, RulesManager rulesManager, FxCopPluginHandler pluginHandler) {
    this.rulesManager = rulesManager;
    this.profile = profile;
    this.pluginHandler = pluginHandler;
  }

  public boolean shouldExecuteOnProject(Project project) {
    return project.getLanguageKey().equals("cs") && "sln".equals(project.getPackaging());
  }

  public MavenPluginHandler getMavenPluginHandler(Project project) {
    // TODO: must handle the activation or not through the properties
    return pluginHandler;
  }

  /**
   * {@inheritDoc}
   */
  public void analyse(Project project, SensorContext context) {
    log.info("----------> Executin FxCop...");
    // Resource<?> file = CSharpResourcesBridge.getInstance().getFromTypeName("Example.Core", "Money");

    final String[] reportFileNames;
    // if (FXCOP_REUSE_MODE.equals(getFxCopMode(project))) {
    // reportFileNames = StringUtils.split(project.getConfiguration().getString(FXCOP_REPORT_KEY),';');
    // log.warn("Using reuse report mode for FxCop");
    // log.warn("FxCop profile settings may not have been taken in account");
    // } else {
    reportFileNames = new String[] { FXCOP_REPORT_XML, SL_FXCOP_REPORT_XML };
    // }

    File dir = project.getFileSystem().getBuildDir();

    for (String reportFileName : reportFileNames) {
      File report = new File(dir, reportFileName);
      if (report.exists()) {
        log.info("FxCop report found at location {}", report);
        FxCopResultParser parser = new FxCopResultParser(project, context, rulesManager, profile);

        parseReport(report, parser, dir);
      } else {
        log.info("No FxCop report found for path {}", report);
      }
    }
  }

  private void parseReport(File report, FxCopResultParser parser, File workDirectory) {
    // We generate the transformer
    File transformedReport = transformReport(report, workDirectory, report.getName() + FXCOP_PROCESSED_REPORT_SUFFIX);
    if (transformedReport == null) {
      return;
    }
    parser.parse(transformedReport);

  }

  private File transformReport(File report, File dir, String targetFileName) {
    try {
      ClassLoader contextClassLoader = Thread.currentThread().getContextClassLoader();
      InputStream stream = contextClassLoader.getResourceAsStream(FXCOP_TRANSFO_XSL);

      if (stream == null) {
        // happens with sonar2.3 classloader mechanism
        stream = getClass().getClassLoader().getResourceAsStream(FXCOP_TRANSFO_XSL);
      }

      Source xslSource = new SAXSource(new InputSource(stream));
      Templates templates = TransformerFactory.newInstance().newTemplates(xslSource);
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

}