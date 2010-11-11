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

package org.apache.maven.dotnet;

import java.io.File;
import java.io.FileOutputStream;
import java.io.FileWriter;
import java.io.FilenameFilter;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.io.UnsupportedEncodingException;
import java.io.Writer;
import java.util.Arrays;
import java.util.Collection;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.ResourceBundle;
import java.util.Set;

import net.sourceforge.pmd.cpd.CPD;
import net.sourceforge.pmd.cpd.CSVRenderer;
import net.sourceforge.pmd.cpd.CsLanguage;
import net.sourceforge.pmd.cpd.Renderer;
import net.sourceforge.pmd.cpd.XMLRenderer;

import org.apache.maven.dotnet.commons.project.SourceFile;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.plugin.MojoFailureException;
import org.apache.maven.plugin.logging.Log;
import org.apache.maven.plugin.pmd.CpdReport;
import org.apache.maven.project.MavenProject;
import org.codehaus.plexus.util.StringUtils;

/**
 * Maven plugin for dotnet cpd analysis. Contains copy/paste code from class
 * CpdReport (it was impossible to inherit from AbstractDotNetMojo and CpdReport
 * at the same time)
 * 
 * @goal cpd
 * @phase site
 * @description generates a report on copy/paste or duplicated code in a C#
 *              project or solution
 * 
 * 
 * @author avictoor101408
 * 
 */
public class CpdMojo extends AbstractDotNetMojo {

  /**
   * The minimum number of tokens that need to be duplicated before it causes a
   * violation.
   * 
   * @parameter expression="${minimumTokens}" default-value="50"
   */
  private int minimumTokens;

  /**
   * Skip the CPD report generation. Most useful on the command line via
   * "-Dcpd.skip=true".
   * 
   * @parameter expression="${cpd.skip}" default-value="false"
   * @since 2.1
   */
  private boolean skip;

  /**
   * The file encoding to use when reading the Java sources.
   * 
   * @parameter expression="${encoding}"
   *            default-value="${project.build.sourceEncoding}"
   * @since 2.3
   */
  private String sourceEncoding;

  /**
   * The output directory for the final HTML report. Note that this parameter is
   * only evaluated if the goal is run directly from the command line or during
   * the default lifecycle. If the goal is run indirectly as part of a site
   * generation, the output directory configured in the Maven Site Plugin is
   * used instead.
   * 
   * @parameter expression="${project.reporting.outputDirectory}"
   * @required
   */
  protected File outputReportingDirectory;

  /**
   * Set the output format type, in addition to the HTML report. Must be one of:
   * "none", "csv", "xml", "txt" or the full class name of the PMD renderer to
   * use. See the net.sourceforge.pmd.renderers package javadoc for available
   * renderers. XML is required if the pmd:check goal is being used.
   * 
   * @parameter expression="${format}" default-value="xml"
   */
  protected String format = "xml";

  /**
   * Run PMD on the tests.
   * 
   * @parameter default-value="false"
   * @since 2.2
   */
  protected boolean includeTests;

  /**
   * Whether to build an aggregated report at the root, or build individual
   * reports.
   * 
   * @parameter expression="${aggregate}" default-value="false"
   * @since 2.2
   */
  protected boolean aggregate;
  
  /**
   * List of the excluded projects, using ',' as delimiter. C# files
   * of these projects will be ignored.
   * 
   * @parameter expression="${skippedProjects}"
   */
  private String skippedProjects;

  /**
   * @see org.apache.maven.reporting.AbstractMavenReport#getProject()
   */
  protected MavenProject getProject() {

    return project;
  }

  /**
   * Convenience method to get the list of files where the CPD tool will be
   * executed
   * 
   * @return a List of the files where the PMD tool will be executed
   * @throws java.io.IOException
   * @throws MojoExecutionException
   */
  protected Map<File, SourceFile> getFilesToProcess()
      throws MojoExecutionException {
    Map<File, SourceFile> fileMap = new HashMap<File, SourceFile>();
    FilenameFilter filter = new CsLanguage().getFileFilter();
    
    Set<String> skippedProjectSet = new HashSet<String>();
    if (skippedProjects!=null) {
      skippedProjectSet.addAll(Arrays.asList(StringUtils.split(skippedProjects,",")));
    }
    
    List<VisualStudioProject> projects = getVisualSolution().getProjects();
    for (VisualStudioProject visualStudioProject : projects) {
      if (visualStudioProject.isTest() && !includeTests) {
        getLog()
            .debug("skipping test project " + visualStudioProject.getName());
        
      } else if (skippedProjectSet.contains(visualStudioProject.getName())) {
        getLog().info("Skipping project " + visualStudioProject.getName());
        
      } else {
        Collection<SourceFile> sources = visualStudioProject.getSourceFiles();
        for (SourceFile sourceFile : sources) {
          if (filter.accept(sourceFile.getFile().getParentFile(),
              sourceFile.getName())) {
            fileMap.put(sourceFile.getFile(), sourceFile);
          }
        }
      }
    }
    return fileMap;
  }

  /**
   * @see org.apache.maven.reporting.AbstractMavenReport#canGenerateReport()
   */
  public boolean canGenerateReport() {

    if (aggregate && !project.isExecutionRoot()) {
      return false;
    }

    // if format is XML, we need to output it even if the file list is empty
    // so the "check" goals can check for failures
    if ("xml".equals(format)) {
      return true;
    }
    try {
      Map filesToProcess = getFilesToProcess();
      if (filesToProcess.isEmpty()) {
        return false;
      }
    } catch (MojoExecutionException e) {
      getLog().error(e);
    }
    return true;
  }

  /**
   * @see org.apache.maven.reporting.AbstractMavenReport#getOutputDirectory()
   */
  protected String getOutputReportingDirectory() {

    return outputReportingDirectory.getAbsolutePath();
  }

  /**
   * @see org.apache.maven.reporting.MavenReport#getName(java.util.Locale)
   */
  public String getName(Locale locale) {

    return getBundle(locale).getString("report.cpd.name");
  }

  /**
   * @see org.apache.maven.reporting.MavenReport#getDescription(java.util.Locale)
   */
  public String getDescription(Locale locale) {

    return getBundle(locale).getString("report.cpd.description");
  }

  /**
   * @throws MojoExecutionException
   * @see org.apache.maven.reporting.AbstractMavenReport#executeReport(java.util.Locale)
   */
  public void executeReport() throws MojoExecutionException {

    if (!skip && canGenerateReport()) {
      ClassLoader origLoader = Thread.currentThread().getContextClassLoader();
      try {
        Thread.currentThread().setContextClassLoader(
            this.getClass().getClassLoader());

        CPD cpd = new CPD(minimumTokens, new CsLanguage());
        try {
          Map<File, SourceFile> files = getFilesToProcess();

          if (StringUtils.isNotEmpty(sourceEncoding)) {
            cpd.setEncoding(sourceEncoding);
          } else if (!files.isEmpty()) {
            getLog()
                .warn(
                    "File encoding has not been set, using platform encoding , i.e. build is platform dependent!");
          }

          Set<File> fileSet = files.keySet();
          for (File file : fileSet) {
            cpd.add(file);
          }

        } catch (UnsupportedEncodingException e) {
          throw new MojoExecutionException("Encoding '" + sourceEncoding
              + "' is not supported.", e);
        } catch (IOException e) {
          throw new MojoExecutionException(e.getMessage(), e);
        }
        cpd.go();

        writeNonHtml(cpd);

      } finally {
        Thread.currentThread().setContextClassLoader(origLoader);
      }

    }
  }

  void writeNonHtml(CPD cpd) throws MojoExecutionException {

    Renderer r = createRenderer();
    String buffer = r.render(cpd.getMatches());
    try {
      outputDirectory.mkdirs();
      FileOutputStream tStream = new FileOutputStream(new File(outputDirectory,
          "cpd." + format));
      Writer writer = new OutputStreamWriter(tStream, "UTF-8");
      writer.write(buffer, 0, buffer.length());
      writer.close();

      File siteDir = new File(outputDirectory, "site");
      siteDir.mkdirs();
      writer = new FileWriter(new File(siteDir, "cpd." + format));
      writer.write(buffer, 0, buffer.length());
      writer.close();

    } catch (IOException ioe) {
      throw new MojoExecutionException(ioe.getMessage(), ioe);
    }
  }

  /**
   * @see org.apache.maven.reporting.MavenReport#getOutputName()
   */
  public String getOutputName() {

    return "cpd";
  }

  private static ResourceBundle getBundle(Locale locale) {

    return ResourceBundle.getBundle("cpd-report", locale,
        CpdReport.class.getClassLoader());
  }

  /**
   * Create and return the correct renderer for the output type.
   * 
   * @return the renderer based on the configured output
   * @throws org.apache.maven.reporting.MavenReportException
   *           if no renderer found for the output type
   */
  public Renderer createRenderer() throws MojoExecutionException {

    Renderer renderer = null;
    if ("xml".equals(format)) {
      renderer = new XMLRenderer("UTF-8");
    } else if ("csv".equals(format)) {
      renderer = new CSVRenderer();
    } else if (!"".equals(format) && !"none".equals(format)) {
      try {
        renderer = (Renderer) Class.forName(format).newInstance();
      } catch (Exception e) {
        throw new MojoExecutionException("Can't find the custom format "
            + format + ": " + e.getClass().getName());
      }
    }

    if (renderer == null) {
      throw new MojoExecutionException("Can't create report with format of "
          + format);
    }

    return renderer;
  }

  @Override
  protected void executeProject(VisualStudioProject visualProject)
      throws MojoExecutionException, MojoFailureException {

    Log log = getLog();
    log.info("executeProject " + visualProject);

  }

  @Override
  protected void executeSolution(VisualStudioSolution visualSolution)
      throws MojoExecutionException, MojoFailureException {

    Log log = getLog();
    log.info("executeSolution " + visualSolution);
    executeReport();
  }
}
