/*
 * .NET tools :: Commons
 * Copyright (C) 2011 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * Created on Apr 16, 2009
 */
package org.sonar.plugins.csharp.api.visualstudio;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import javax.xml.XMLConstants;
import javax.xml.namespace.NamespaceContext;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpression;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;

import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.WildcardPattern;
import org.sonar.plugins.csharp.api.DotNetToolsException;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;

/**
 * Utility classes for the parsing of a Visual Studio project
 * 
 * @author Fabrice BELLINGARD
 * @author Jose CHILLAN Aug 14, 2009
 */
public final class ModelFactory {

  private static final Logger LOG = LoggerFactory.getLogger(ModelFactory.class);
  private static final String VERSION_KEY = ", Version=";

  /*
   * Pattern used to define if a project is a test project or not
   */
  private static String testProjectNamePattern = "*.Tests";

  private ModelFactory() {
  }

  /**
   * Sets the pattern used to define if a project is a test project or not
   * 
   * @param testProjectNamePattern
   *          the pattern
   */
  public static void setTestProjectNamePattern(String testProjectNamePattern) {
    ModelFactory.testProjectNamePattern = testProjectNamePattern;
  }

  /**
   * Checks, whether the child directory is a subdirectory of the base directory.
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
      File baseFile = base.getCanonicalFile();
      File childFile = child.getCanonicalFile();
      File parentFile = childFile;

      // Checks recursively if "base" is one of the parent of "child"
      while (parentFile != null) {
        if (baseFile.equals(parentFile)) {
          return true;
        }
        parentFile = parentFile.getParentFile();
      }
    } catch (IOException ex) {
      // This is false
      if (LOG.isDebugEnabled()) {
        LOG.debug(child + " is not in " + base, ex);
      }
    }
    return false;
  }

  /**
   * @param visualStudioProject
   */
  protected static void assessTestProject(VisualStudioProject visualStudioProject, String testProjectPatterns) {

    String assemblyName = visualStudioProject.getAssemblyName();

    String[] patterns = StringUtils.split(testProjectPatterns, ";");
    boolean testFlag = false;

    for (int i = 0; i < patterns.length; i++) {
      if (WildcardPattern.create(patterns[i]).match(assemblyName)) {
        testFlag = true;
        break;
      }
    }

    if (testFlag) {
      LOG.info("The project '{}' has been qualified as a test project.", visualStudioProject.getName());
    }

    visualStudioProject.setTest(testFlag);
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
   * @throws DotNetToolsException
   */
  public static VisualStudioSolution getSolution(File baseDirectory, String solutionName) throws IOException, DotNetToolsException {
    File solutionFile = new File(baseDirectory, solutionName);
    return getSolution(solutionFile);
  }

  /**
   * @param solutionFile
   *          the solution file
   * @return a new visual studio solution
   * @throws IOException
   * @throws DotNetToolsException
   */
  public static VisualStudioSolution getSolution(File solutionFile) throws IOException, DotNetToolsException {

    String solutionContent = FileUtils.readFileToString(solutionFile);
    List<String> buildConfigurations = getBuildConfigurations(solutionContent);

    List<VisualStudioProject> projects = getProjects(solutionFile, solutionContent, buildConfigurations);
    VisualStudioSolution solution = new VisualStudioSolution(solutionFile, projects);
    solution.setBuildConfigurations(buildConfigurations);
    solution.setName(solutionFile.getName());
    return solution;
  }

  private static List<String> getBuildConfigurations(String solutionContent) {
    // A pattern to extract the build configurations from a visual studio solution
    String confExtractExp = "(\tGlobalSection\\(SolutionConfigurationPlatforms\\).*?^\tEndGlobalSection$)";
    Pattern confExtractPattern = Pattern.compile(confExtractExp, Pattern.MULTILINE + Pattern.DOTALL);
    List<String> buildConfigurations = new ArrayList<String>();
    // Extracts all the projects from the solution
    Matcher blockMatcher = confExtractPattern.matcher(solutionContent);
    if (blockMatcher.find()) {
      String buildConfigurationBlock = blockMatcher.group(1);
      String buildConfExtractExp = " = (.*)\\|";
      Pattern buildConfExtractPattern = Pattern.compile(buildConfExtractExp);
      Matcher buildConfMatcher = buildConfExtractPattern.matcher(buildConfigurationBlock);
      while (buildConfMatcher.find()) {
        String buildConfiguration = buildConfMatcher.group(1);
        buildConfigurations.add(buildConfiguration);
      }
    }
    return buildConfigurations;
  }

  /**
   * Gets all the projects in a solution.
   * 
   * @param solutionFile
   *          the solution file
   * @param solutionContent
   *          the text content of the solution file
   * @return a list of projects
   * @throws IOException
   * @throws DotNetToolsException
   */
  private static List<VisualStudioProject> getProjects(File solutionFile, String solutionContent, List<String> buildConfigurations)
      throws IOException, DotNetToolsException {

    File baseDirectory = solutionFile.getParentFile();

    // A pattern to extract the projects from a visual studion solution
    String projectExtractExp = "(Project.*?^EndProject$)";
    Pattern projectExtractPattern = Pattern.compile(projectExtractExp, Pattern.MULTILINE + Pattern.DOTALL);
    List<String> projectDefinitions = new ArrayList<String>();
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
    Pattern webPattern = Pattern.compile(webProjectExp, Pattern.MULTILINE + Pattern.DOTALL);

    List<VisualStudioProject> result = new ArrayList<VisualStudioProject>();
    for (String projectDefinition : projectDefinitions) {
      // Looks for project files
      Matcher matcher = projectPattern.matcher(projectDefinition);
      if (matcher.find()) {
        String projectName = matcher.group(1);
        String projectPath = StringUtils.replace(matcher.group(2), "\\", File.separatorChar + "");

        File projectFile = new File(baseDirectory, projectPath);
        if ( !projectFile.exists()) {
          throw new FileNotFoundException("Could not find the project file: " + projectFile);
        }
        VisualStudioProject project = getProject(projectFile, projectName, buildConfigurations);
        result.add(project);
      } else {
        // Searches the web project
        Matcher webMatcher = webPattern.matcher(projectDefinition);

        if (webMatcher.find()) {
          String projectName = webMatcher.group(1);
          String projectPath = webMatcher.group(2);
          File projectRoot = new File(baseDirectory, projectPath);
          VisualStudioProject project = getWebProject(baseDirectory, projectRoot, projectName, projectDefinition);
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
   * @throws DotNetToolsException
   * @throws FileNotFoundException
   */
  public static VisualStudioProject getProject(File projectFile) throws FileNotFoundException, DotNetToolsException {
    String projectName = projectFile.getName();
    return getProject(projectFile, projectName, null);
  }

  /**
   * Generates a list of projects from the path of the visual studio projects files (.csproj)
   * 
   * @param projectFile
   *          the project file
   * @param projectName
   *          the name of the project
   * @throws DotNetToolsException
   * @throws FileNotFoundException
   *           if the file was not found
   */
  public static VisualStudioProject getProject(File projectFile, String projectName, List<String> buildConfigurations)
      throws FileNotFoundException, DotNetToolsException {

    VisualStudioProject project = new VisualStudioProject();
    project.setProjectFile(projectFile);
    project.setName(projectName);
    File projectDir = projectFile.getParentFile();

    XPathFactory factory = XPathFactory.newInstance();
    XPath xpath = factory.newXPath();

    // This is a workaround to avoid Xerces class-loading issues
    ClassLoader savedClassloader = Thread.currentThread().getContextClassLoader();
    Thread.currentThread().setContextClassLoader(xpath.getClass().getClassLoader());
    try {
      // We define the namespace prefix for Visual Studio
      xpath.setNamespaceContext(new VisualStudioNamespaceContext());

      if (buildConfigurations != null) {
        Map<String, File> buildConfOutputDirMap = new HashMap<String, File>();
        for (String config : buildConfigurations) {
          XPathExpression configOutputExpression = xpath.compile("/vst:Project/vst:PropertyGroup[contains(@Condition,'" + config
              + "')]/vst:OutputPath");
          String configOutput = extractProjectProperty(configOutputExpression, projectFile);
          buildConfOutputDirMap.put(config, new File(projectDir, configOutput));
        }
        project.setBuildConfOutputDirMap(buildConfOutputDirMap);
      }

      XPathExpression projectTypeExpression = xpath.compile("/vst:Project/vst:PropertyGroup/vst:OutputType");
      XPathExpression assemblyNameExpression = xpath.compile("/vst:Project/vst:PropertyGroup/vst:AssemblyName");
      XPathExpression rootNamespaceExpression = xpath.compile("/vst:Project/vst:PropertyGroup/vst:RootNamespace");
      XPathExpression debugOutputExpression = xpath.compile("/vst:Project/vst:PropertyGroup[contains(@Condition,'Debug')]/vst:OutputPath");
      XPathExpression releaseOutputExpression = xpath
          .compile("/vst:Project/vst:PropertyGroup[contains(@Condition,'Release')]/vst:OutputPath");
      XPathExpression silverlightExpression = xpath.compile("/vst:Project/vst:PropertyGroup/vst:SilverlightApplication");

      // Extracts the properties of a Visual Studio Project
      String typeStr = extractProjectProperty(projectTypeExpression, projectFile);
      String silverlightStr = extractProjectProperty(silverlightExpression, projectFile);
      String assemblyName = extractProjectProperty(assemblyNameExpression, projectFile);
      String rootNamespace = extractProjectProperty(rootNamespaceExpression, projectFile);
      String debugOutput = extractProjectProperty(debugOutputExpression, projectFile);
      String releaseOutput = extractProjectProperty(releaseOutputExpression, projectFile);

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

      if (StringUtils.isNotEmpty(silverlightStr)) {
        project.setSilverlightProject(true);
      }

      project.setBinaryReferences(getBinaryReferences(xpath, projectFile));

      assessTestProject(project, testProjectNamePattern);

      return project;
    } catch (XPathExpressionException xpee) {
      throw new DotNetToolsException("Error while processing the project " + projectFile, xpee);
    } finally {
      // Replaces the class loader after usage
      Thread.currentThread().setContextClassLoader(savedClassloader);
    }
  }

  private static List<BinaryReference> getBinaryReferences(XPath xpath, File projectFile) throws DotNetToolsException {
    List<BinaryReference> result = new ArrayList<BinaryReference>();
    try {

      XPathExpression targetFwkIdExpression = xpath.compile("/vst:Project/vst:PropertyGroup/vst:TargetFrameworkIdentifier");
      XPathExpression targetFwkVersionExpression = xpath.compile("/vst:Project/vst:PropertyGroup/vst:TargetFrameworkVersion");

      String fwkId = extractProjectProperty(targetFwkIdExpression, projectFile);
      String fwkversion = extractProjectProperty(targetFwkVersionExpression, projectFile);

      final String systemVersion;
      if (StringUtils.isEmpty(fwkId)) {
        systemVersion = fwkversion;
      } else {
        systemVersion = fwkId + '.' + fwkversion;
      }

      XPathExpression binaryRefExpression = xpath.compile("/vst:Project/vst:ItemGroup/vst:Reference");
      InputSource inputSource = new InputSource(new FileInputStream(projectFile));
      NodeList nodes = (NodeList) binaryRefExpression.evaluate(inputSource, XPathConstants.NODESET);
      int countNodes = nodes.getLength();
      for (int idxNode = 0; idxNode < countNodes; idxNode++) {
        Element includeElement = (Element) nodes.item(idxNode);
        // We filter the files
        String includeAttr = includeElement.getAttribute("Include");
        if (StringUtils.isEmpty(includeAttr)) {
          LOG.debug("Binary reference ignored, Include attribute missing");
        } else {
          BinaryReference reference = new BinaryReference();

          int versionIndex = includeAttr.indexOf(VERSION_KEY);
          if (versionIndex == -1) {
            reference.setAssemblyName(includeAttr);
            reference.setVersion(systemVersion);
          } else {
            String assemblyName = includeAttr.substring(0, versionIndex);
            int versionEndIndex = includeAttr.indexOf(",", versionIndex + 1);
            if (versionEndIndex < 0) {
              versionEndIndex = includeAttr.length();
            }

            String version = includeAttr.substring(versionIndex + VERSION_KEY.length(), versionEndIndex);
            reference.setAssemblyName(assemblyName);
            reference.setVersion(version);
          }
          result.add(reference);
        }
      }

    } catch (XPathExpressionException exception) {
      // Should not happen
      LOG.debug("xpath error", exception);
    } catch (FileNotFoundException exception) {
      // Should not happen
      LOG.debug("project file not found", exception);
    }
    return result;
  }

  public static VisualStudioProject getWebProject(File solutionRoot, File projectRoot, String projectName, String definition)
      throws FileNotFoundException {

    // We define the namespace prefix for Visual Studio
    VisualStudioProject project = new VisualStudioWebProject();
    project.setName(projectName);

    // Extracts the properties of a Visual Studio Project
    String assemblyName = projectName;
    String rootNamespace = "";
    String debugOutput = extractSolutionProperty("Debug.AspNetCompiler.TargetPath", definition);
    String releaseOutput = extractSolutionProperty("Release.AspNetCompiler.TargetPath", definition);

    // The project is populated
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
   * Gets the relative paths of all the files in a project, as they are defined in the .csproj file.
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
      XPathExpression filesExpression = xpath.compile("/vst:Project/vst:ItemGroup/vst:Compile");
      InputSource inputSource = new InputSource(new FileInputStream(project));
      NodeList nodes = (NodeList) filesExpression.evaluate(inputSource, XPathConstants.NODESET);
      int countNodes = nodes.getLength();
      for (int idxNode = 0; idxNode < countNodes; idxNode++) {
        Element compileElement = (Element) nodes.item(idxNode);
        // We filter the files
        String filePath = compileElement.getAttribute("Include");
        if ((filePath != null) && filePath.endsWith(".cs")) {

          // fix tests on unix system
          // but should not be necessary
          // on windows build machines
          filePath = StringUtils.replace(filePath, "\\", File.separatorChar + "");
          result.add(filePath);
        }
      }

    } catch (XPathExpressionException exception) {
      // Should not happen
      LOG.debug("xpath error", exception);
    } catch (FileNotFoundException exception) {
      // Should not happen
      LOG.debug("project file not found", exception);
    }
    return result;
  }

  /**
   * Extracts a string project data.
   * 
   * @param expression
   * @param projectFile
   * @return
   * @throws DotNetToolsException
   * @throws FileNotFoundException
   */
  private static String extractProjectProperty(XPathExpression expression, File projectFile) throws DotNetToolsException {
    try {
      FileInputStream file = new FileInputStream(projectFile);
      InputSource source = new InputSource(file);
      return expression.evaluate(source);
    } catch (Exception e) {
      throw new DotNetToolsException("Could not evaluate the expression " + expression + " on project " + projectFile, e);
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
        throw new IllegalStateException("Null prefix");
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
   * Checks a file existence in a directory.
   * 
   * @param basedir
   *          the directory containing the file
   * @param fileName
   *          the file name
   * @return <code>null</code> if the file doesn't exist, the file if it is found
   */
  public static File checkFileExistence(File basedir, String fileName) {
    File checkedFile = new File(basedir, fileName);
    if (checkedFile.exists()) {
      return checkedFile;
    }
    return null;
  }
}
