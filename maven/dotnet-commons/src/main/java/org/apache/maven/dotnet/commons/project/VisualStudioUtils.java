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
 * Created on Apr 16, 2009
 */
package org.apache.maven.dotnet.commons.project;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FilenameFilter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import javax.xml.XMLConstants;
import javax.xml.namespace.NamespaceContext;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpression;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;

import org.apache.maven.project.MavenProject;
import org.codehaus.plexus.util.FileUtils;
import org.codehaus.plexus.util.SelectorUtils;
import org.codehaus.plexus.util.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;

/**
 * Utility classes for the parsing of a Visual Studio project
 * 
 * @author Jose CHILLAN Aug 14, 2009
 */
public class VisualStudioUtils {
  
  private final static Logger log = LoggerFactory.getLogger(VisualStudioUtils.class);
  
  public final static String TEST_PROJECT_PATTERN_PROPERTY = "visual.test.project.pattern";
  public final static String VISUAL_SOLUTION_NAME_PROPERTY = "visual.studio.solution";
  public final static String VISUAL_PROJECT_NAME_PROPERTY = "visual.studio.project";
  public final static String SOLUTION_PACKAGING = "sln";

  /**
   * Checks, whether the child directory is a subdirectory of the base
   * directory.
   * 
   * @param base
   *          the base directory.
   * @param child
   *          the suspected child directory.
   * @return true, if the child is a subdirectory of the base directory.
   * @throws IOException
   *           if an IOError occured during the test.
   */
  public static boolean isSubDirectory(File base, File child) {
    try {
      base = base.getCanonicalFile();
      child = child.getCanonicalFile();

      File parentFile = child;

      // Checks recursively if "base" is one of the parent of "child"
      while (parentFile != null) {
        if (base.equals(parentFile)) {
          return true;
        }
        parentFile = parentFile.getParentFile();
      }
    } catch (IOException ex) {
      // This is false
      if (log.isDebugEnabled()) {
        log.debug(child + " is not in " + base, ex);
      }
    }
    return false;
  }

  public static VisualStudioSolution getVisualSolution(MavenProject project,
      String proposedSolutionName) throws DotNetProjectException {
    if (!SOLUTION_PACKAGING.equals(project.getPackaging())) {
      return null;
    }
    String solutionName = proposedSolutionName;
    if (solutionName == null || StringUtils.isWhitespace(solutionName)) {
      // No proposed name
      solutionName = project.getProperties().getProperty(
          VISUAL_SOLUTION_NAME_PROPERTY);
    }
    if (solutionName == null) {
      // This is not a solution
      return null;
    }

    File solutionFile;
    try {
      solutionFile = VisualStudioUtils.getVisualFile(project, solutionName,
          ".sln", "solution");
    } catch (DotNetProjectException except) {
      throw new DotNetProjectException(
          "Could not create a VisualStudio solution", except);
    }

    // No solution defined
    if (solutionFile == null) {
      return null;
    }
    return getVisualSolution(project, solutionFile);
  }

  /**
   * @param project
   * @param solutionFile
   * @return
   * @throws DotNetProjectException
   */
  public static VisualStudioSolution getVisualSolution(MavenProject project,
      File solutionFile) throws DotNetProjectException {
    // We try to build the solution
    VisualStudioSolution solution;
    try {
      solution = VisualStudioUtils.getSolution(solutionFile);
    } catch (Exception e) {
      throw new DotNetProjectException(
          "Could not extract the solution information for " + solutionFile, e);
    }
    List<VisualStudioProject> projects = solution.getProjects();
    String testProjectPattern = project.getProperties().getProperty(
        TEST_PROJECT_PATTERN_PROPERTY, "*.Tests");
    // We define for each project if it is a test project
    for (VisualStudioProject visualStudioProject : projects) {
      assessTestProject(visualStudioProject, testProjectPattern);
    }
    return solution;
  }

  /**
   * @param visualStudioProject
   */
  private static void assessTestProject(
      VisualStudioProject visualStudioProject, String testProjectPattern) {
    String assemblyName = visualStudioProject.getAssemblyName();
    if (SelectorUtils.match(testProjectPattern, assemblyName)) {
      visualStudioProject.setTest(true);
    }
  }

  /**
   * Gets the solution from its folder and name.
   * 
   * @param baseDirectory
   *          the directory containing the solution
   * @param solutionName
   *          the solution name
   * @return the generated solution
   * @throws IOException
   * @throws XPathExpressionException
   */
  public static VisualStudioSolution getSolution(File baseDirectory,
      String solutionName) throws IOException, DotNetProjectException {
    File solutionFile = new File(baseDirectory, solutionName);
    return getSolution(solutionFile);
  }

  /**
   * @param solutionFile
   *          the solution file
   * @return a new visual studio solution
   * @throws IOException
   * @throws XPathExpressionException
   */
  public static VisualStudioSolution getSolution(File solutionFile)
      throws IOException, DotNetProjectException {
    List<VisualStudioProject> projects = getProjects(solutionFile);
    VisualStudioSolution solution = new VisualStudioSolution(solutionFile,
        projects);
    solution.setName(solutionFile.getName());
    return solution;
  }

  /**
   * Gets all the projects in a solution.
   * 
   * @param solutionFile
   *          the solution file
   * @return a list of projects
   * @throws IOException
   * @throws XPathExpressionException
   */
  protected static List<VisualStudioProject> getProjects(File solutionFile)
      throws IOException, DotNetProjectException {
    File baseDirectory = solutionFile.getParentFile();

    // A pattern to extract the projects from a visual studion solution
    String projectExtractExp = "(Project.*?^EndProject$)";
    Pattern projectExtractPattern = Pattern.compile(projectExtractExp,
        Pattern.MULTILINE + Pattern.DOTALL);
    List<String> projectDefinitions = new ArrayList<String>();
    String solutionContent = FileUtils.fileRead(solutionFile);
    // Extracts all the projects from the solution
    Matcher globalMatcher = projectExtractPattern.matcher(solutionContent);
    while (globalMatcher.find()) {
      String projectDefinition = globalMatcher.group(1);
      projectDefinitions.add(projectDefinition);
    }

    // This pattern extracts the projects from a Visual Studio solution
    String normalProjectExp = "\\s*Project\\([^\\)]*\\)\\s*=\\s*\"([^\"]*)\"\\s*,\\s*\"([^\"]*?\\.csproj)\"";
    String webProjectExp = "\\s*Project\\([^\\)]*\\)\\s*=\\s*\"([^\"]*).*?ProjectSection\\(WebsiteProperties\\).*?"
        + "Debug\\.AspNetCompiler\\.PhysicalPath\\s*=\\s*\"([^\"]*)";
    Pattern projectPattern = Pattern.compile(normalProjectExp);
    Pattern webPattern = Pattern.compile(webProjectExp, Pattern.MULTILINE
        + Pattern.DOTALL);

    List<VisualStudioProject> result = new ArrayList<VisualStudioProject>();
    for (String projectDefinition : projectDefinitions) {
      // Looks for project files
      Matcher matcher = projectPattern.matcher(projectDefinition);
      if (matcher.find()) {
        String projectName = matcher.group(1);
        String projectPath = matcher.group(2);
        File projectFile = new File(baseDirectory, projectPath);
        if (!projectFile.exists()) {
          throw new FileNotFoundException("Could not find the project file: "
              + projectFile);
        }
        VisualStudioProject project = getProject(projectFile, projectName);
        result.add(project);
      } else {
        // Searches the web project
        Matcher webMatcher = webPattern.matcher(projectDefinition);

        if (webMatcher.find()) {
          String projectName = webMatcher.group(1);
          String projectPath = webMatcher.group(2);
          File projectRoot = new File(baseDirectory, projectPath);
          VisualStudioProject project = getWebProject(baseDirectory,
              projectRoot, projectName, projectDefinition);
          result.add(project);
        }
      }
    }
    return result;
  }

  /**
   * Creates a project from its file
   * 
   * @param projectFile
   *          the project file
   * @return the visual project if possible to define
   * @throws XPathExpressionException
   *           if the project is invalid
   * @throws FileNotFoundException
   */
  public static VisualStudioProject getProject(File projectFile)
      throws DotNetProjectException, FileNotFoundException {
    String projectName = projectFile.getName();
    return getProject(projectFile, projectName);
  }

  /**
   * Generates a list of projects from the path of the visual studio projects
   * files (.csproj)
   * 
   * @param projectFile
   *          the project file
   * @param projectName
   *          the name of the project
   * @throws XPathExpressionException
   *           if the project .csproj is invalid
   * @throws FileNotFoundException
   *           if the file was not found
   */
  public static VisualStudioProject getProject(File projectFile,
      String projectName) throws DotNetProjectException, FileNotFoundException {
    XPathFactory factory = XPathFactory.newInstance();
    XPath xpath = factory.newXPath();

    // This is a workaround to avoid Xerces class-loading issues
    ClassLoader savedClassloader = Thread.currentThread()
        .getContextClassLoader();
    Thread.currentThread().setContextClassLoader(
        xpath.getClass().getClassLoader());
    try {
      // We define the namespace prefix for Visual Studio
      xpath.setNamespaceContext(new VisualStudioNamespaceContext());
      XPathExpression projectTypeExpression = xpath
          .compile("/vst:Project/vst:PropertyGroup/vst:OutputType");
      XPathExpression assemblyNameExpression = xpath
          .compile("/vst:Project/vst:PropertyGroup/vst:AssemblyName");
      XPathExpression rootNamespaceExpression = xpath
          .compile("/vst:Project/vst:PropertyGroup/vst:RootNamespace");
      XPathExpression debugOutputExpression = xpath
          .compile("/vst:Project/vst:PropertyGroup[contains(@Condition,'Debug')]/vst:OutputPath");
      XPathExpression releaseOutputExpression = xpath
          .compile("/vst:Project/vst:PropertyGroup[contains(@Condition,'Release')]/vst:OutputPath");
      VisualStudioProject project = new VisualStudioProject();
      project.setProjectFile(projectFile);
      project.setName(projectName);
      File projectDir = projectFile.getParentFile();

      // Extracts the properties of a Visual Studio Project
      String typeStr = extractProjectProperty(projectTypeExpression,
          projectFile);
      String assemblyName = extractProjectProperty(assemblyNameExpression,
          projectFile);
      String rootNamespace = extractProjectProperty(rootNamespaceExpression,
          projectFile);
      String debugOutput = extractProjectProperty(debugOutputExpression,
          projectFile);
      String releaseOutput = extractProjectProperty(releaseOutputExpression,
          projectFile);

      // Assess if the artifact is a library or an executable
      ArtifactType type = ArtifactType.LIBRARY;
      if (typeStr.toLowerCase().contains("exe")) {
        type = ArtifactType.EXECUTABLE;
      }
      // The project is populated
      project.setProjectFile(projectFile);
      project.setType(type);
      project.setDirectory(projectDir);
      project.setAssemblyName(assemblyName);
      project.setRootNamespace(rootNamespace);
      project.setDebugOutputDir(new File(projectDir, debugOutput));
      project.setReleaseOutputDir(new File(projectDir, releaseOutput));
      return project;
    } catch (XPathExpressionException xpee) {
      throw new DotNetProjectException("Error while processing the project "
          + projectFile, xpee);
    } finally {
      // Replaces the class loader after usage
      Thread.currentThread().setContextClassLoader(savedClassloader);
    }
  }

  public static VisualStudioProject getWebProject(File solutionRoot,
      File projectRoot, String projectName, String definition)
      throws DotNetProjectException, FileNotFoundException {

    // We define the namespace prefix for Visual Studio
    VisualStudioProject project = new VisualStudioProject();
    project.setName(projectName);

    // Extracts the properties of a Visual Studio Project
    String assemblyName = projectName;
    String rootNamespace = "";
    String debugOutput = extractSolutionProperty(
        "Debug.AspNetCompiler.TargetPath", definition);
    String releaseOutput = extractSolutionProperty(
        "Release.AspNetCompiler.TargetPath", definition);

    // Assess if the artifact is a library or an executable
    ArtifactType type = ArtifactType.WEB;

    // The project is populated
    project.setProjectFile(null); // No projet file
    project.setType(type);
    project.setDirectory(projectRoot);
    project.setAssemblyName(assemblyName);
    project.setRootNamespace(rootNamespace);
    project.setDebugOutputDir(new File(solutionRoot, debugOutput));
    project.setReleaseOutputDir(new File(solutionRoot, releaseOutput));
    return project;
  }

  /**
   * Reads a property from a project
   * 
   * @param string
   * @param definition
   * @return
   */
  public static String extractSolutionProperty(String name, String definition) {
    String regexp = name + "\\s*=\\s*\"([^\"]*)";
    Pattern pattern = Pattern.compile(regexp);
    Matcher matcher = pattern.matcher(definition);
    if (matcher.find()) {
      return matcher.group(1);
    }
    return null;
  }

  /**
   * Gets the relative paths of all the files in a project, as they are defined
   * in the .csproj file.
   * 
   * @param project
   *          the project file
   * @return a list of the project files
   */
  public static List<String> getFilesPath(File project) {
    List<String> result = new ArrayList<String>();

    XPathFactory factory = XPathFactory.newInstance();
    XPath xpath = factory.newXPath();
    // We define the namespace prefix for Visual Studio
    xpath.setNamespaceContext(new VisualStudioNamespaceContext());
    try {
      XPathExpression filesExpression = xpath
          .compile("/vst:Project/vst:ItemGroup/vst:Compile");
      InputSource inputSource = new InputSource(new FileInputStream(project));
      NodeList nodes = (NodeList) filesExpression.evaluate(inputSource,
          XPathConstants.NODESET);
      int countNodes = nodes.getLength();
      for (int idxNode = 0; idxNode < countNodes; idxNode++) {
        Element compileElement = (Element) nodes.item(idxNode);
        // We filter the files
        String filePath = compileElement.getAttribute("Include");
        if ((filePath != null) && filePath.endsWith(".cs")) {
          result.add(filePath);
        }
      }

    } catch (XPathExpressionException exception) {
      // Should not happen
      log.debug("xpath error", exception);
    } catch (FileNotFoundException exception) {
      // Should not happen
      log.debug("project file not found", exception);
    }
    return result;
  }

  /**
   * Extracts a string project data.
   * 
   * @param expression
   * @param projectFile
   * @return
   * @throws XPathExpressionException
   * @throws FileNotFoundException
   */
  private static String extractProjectProperty(XPathExpression expression,
      File projectFile) throws DotNetProjectException {
    try {
      FileInputStream file = new FileInputStream(projectFile);
      InputSource source = new InputSource(file);
      return expression.evaluate(source);
    } catch (Exception e) {
      throw new DotNetProjectException("Could not evaluate the expression "
          + expression + " on project " + projectFile, e);
    }
  }

  /**
   * A Namespace context specialized for the handling of csproj files
   * 
   * @author Jose CHILLAN Sep 1, 2009
   */
  private static class VisualStudioNamespaceContext implements NamespaceContext {

    /**
     * Gets the namespace URI.
     * 
     * @param prefix
     * @return
     */
    public String getNamespaceURI(String prefix) {
      if (prefix == null) {
        throw new RuntimeException("Null prefix");
      }
      
      final String result;    
      if ("vst".equals(prefix)) {
        result = "http://schemas.microsoft.com/developer/msbuild/2003"; 
      } else if ("xml".equals(prefix)) {
        result = XMLConstants.XML_NS_URI;
      } else {
        result = XMLConstants.NULL_NS_URI;
      }
      return result;
    }

    // This method isn't necessary for XPath processing.
    public String getPrefix(String uri) {
      throw new UnsupportedOperationException();
    }

    // This method isn't necessary for XPath processing either.
    public Iterator<?> getPrefixes(String uri) {
      throw new UnsupportedOperationException();
    }

  }

  /**
   * Gets a visual file from a project
   * 
   * @param localProject
   * @param configuredName
   * @param extension
   * @param fileType
   * @return the visual file
   * @throws DotNetProjectException
   */
  public static File getVisualFile(MavenProject localProject,
      String configuredName, final String extension, String fileType)
      throws DotNetProjectException {
    File basedir = localProject.getBasedir();
    File visualFile = null;
    // We look for a defined name
    if (configuredName != null) {
      visualFile = checkFileExistence(basedir, configuredName);
      if (visualFile == null) {
        throw new DotNetProjectException("Could not find the configured "
            + fileType + " file " + configuredName);
      }
    }
    String fileName;
    // Second : we try a solution having the same name as the artifact id
    if (visualFile == null) {
      String artifactId = localProject.getArtifactId();
      fileName = artifactId + extension;
      visualFile = checkFileExistence(basedir, fileName);
    }
    // We have found it
    if (visualFile != null) {
      return visualFile;
    }

    // Last chance : we scan the folder for a solution
    File[] matchingFiles = basedir.listFiles(new FilenameFilter() {
      @Override
      public boolean accept(File dir, String name) {
        return name.endsWith(extension);
      }
    });
    if (matchingFiles.length >= 2) {
      StringBuilder builder = new StringBuilder("Found multiple " + fileType
          + " in the same folder." + " Please define the 'visual." + fileType
          + ".name' property to select from: ");
      for (File file : matchingFiles) {
        builder.append(file.getName()).append(" ");
      }
      throw new DotNetProjectException(builder.toString());
    }
    if (matchingFiles.length == 1) {
      visualFile = matchingFiles[0];
    }
    return visualFile;
  }

  /**
   * Checks a file existence in a directory.
   * 
   * @param basedir
   *          the directory containing the file
   * @param fileName
   *          the file name
   * @return <code>null</code> if the file doesn't exist, the file if it is
   *         found
   */
  public static File checkFileExistence(File basedir, String fileName) {
    File checkedFile = new File(basedir, fileName);
    if (checkedFile.exists()) {
      return checkedFile;
    }
    return null;
  }
}
