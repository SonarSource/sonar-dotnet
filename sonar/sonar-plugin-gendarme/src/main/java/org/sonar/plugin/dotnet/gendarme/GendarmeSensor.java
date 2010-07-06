package org.sonar.plugin.dotnet.gendarme;

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
import org.sonar.plugin.dotnet.core.resource.InvalidResourceException;
import org.xml.sax.InputSource;

public class GendarmeSensor extends AbstractDotnetSensor
{
  private final static Logger log                        = LoggerFactory.getLogger(GendarmeSensor.class);

  private static final String GENDARME_REPORT_XML           = "gendarme-report.xml";
  private static final String GENDARME_TRANSFO_XSL          = "gendarme-transformation.xsl";
  private static final String GENDARME_PROCESSED_REPORT_XML = "gendarme-report-processed.xml";

  private RulesManager        rulesManager;
  private RulesProfile        profile;
  private GendarmePluginHandler  pluginHandler;

  /**
   * Constructs a @link{GendarmeSensor}.
   * 
   * @param rulesManager
   */
  public GendarmeSensor(RulesProfile profile, RulesManager rulesManager, GendarmePluginHandler pluginHandler)
  {
    super();
    this.rulesManager = rulesManager;
    this.profile = profile;
    this.pluginHandler = pluginHandler;
  }

  /**
   * Launches the project analysis/
   * @param project
   * @param context
   */
  @Override
  public void analyse(Project project, SensorContext context)
  {
    File report = findReport(project, GENDARME_REPORT_XML);
    File dir = getReportsDirectory(project);

    // We generate the transformer
    File transformedReport = transformReport(report, dir);
    if (transformedReport == null)
    {
      return;
    }
    GendarmeResultParser parser = new GendarmeResultParser(project, context, rulesManager, profile);
    try
    {
      URL fileURL = transformedReport.toURI().toURL();
      parser.parse(fileURL);
    }
    catch (MalformedURLException e)
    {
      log.debug("Error while loading the file: {}\n{}", report, e);
    }
    catch (InvalidResourceException ex) {
  	  log.warn("C# file not referenced in the solution", ex);
    }
  }

  /**
   * Transforms the report to a usable format.
   * @param report
   * @param dir
   * @return
   */
  private File transformReport(File report, File dir)
  {
    try
    {
      ClassLoader contextClassLoader = Thread.currentThread().getContextClassLoader();
      InputStream stream = contextClassLoader.getResourceAsStream(GENDARME_TRANSFO_XSL);
      Source xslSource = new SAXSource(new InputSource(stream));
      Templates templates = TransformerFactory.newInstance().newTemplates(xslSource);
      Transformer transformer = templates.newTransformer();

      // We open the report to be processed
      Source xmlSource = new StreamSource(report);

      File processedReport = new File(dir, GENDARME_PROCESSED_REPORT_XML);
      processedReport.delete();
      Result result = new StreamResult(processedReport);
      transformer.transform(xmlSource, result);
      return processedReport;
    }
    catch (Exception exc)
    {
      log.warn("Error during the processing of the Gendarme report for Sonar", exc);
    }
    return null;
  }

  /**
   * @param project
   * @return
   */
  @Override
  public MavenPluginHandler getMavenPluginHandler(Project project)
  {
    return pluginHandler;
  }

}
