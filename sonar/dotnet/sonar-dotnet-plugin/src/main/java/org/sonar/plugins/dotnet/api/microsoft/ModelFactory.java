/*
 * Sonar .NET Plugin :: Core
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
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
package org.sonar.plugins.dotnet.api.microsoft;

import org.codehaus.plexus.util.xml.Xpp3Dom;

import org.apache.maven.model.Plugin;

import com.google.common.collect.Maps;

import org.sonar.api.utils.SonarException;

import com.google.common.collect.Lists;

import org.sonar.api.batch.bootstrap.ProjectDefinition;

import org.sonar.plugins.dotnet.api.DotNetConstants;

import org.sonar.plugins.dotnet.api.DotNetConfiguration;

import org.apache.maven.project.MavenProject;

import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.WildcardPattern;
import org.sonar.plugins.dotnet.api.DotNetException;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;

import javax.xml.XMLConstants;
import javax.xml.namespace.NamespaceContext;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpression;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Utility classes for the parsing of a Visual Studio project
 *
 * @author Fabrice BELLINGARD
 * @author Jose CHILLAN Aug 14, 2009
 */
public final class ModelFactory {

  private static final Logger LOG = LoggerFactory.getLogger(ModelFactory.class);

  /** The NPanday relative(to maven build directory) test assemblies directory*/
  private static final String NPANDAY_TEST_ASSEMBLIES_DIR = "test-assemblies";
  private static final String NPANDAY_ASP_NET_PACKAGING = "asp";
  private static final String NPANDAY_SILVERLIGHT_PACKAGING = "silverlight-application";
  private static final String NPANDAY_DOTNET_EXECUTABLE_PACKAGING = "dotnet-executable";
  private static final String NPANDAY_COMPILE_PLUGIN_KEY = "org.apache.npanday.plugins:maven-compile-plugin";
  private static final String NPANDAY_ROOTNAMESPACE_KEY = "rootNamespace";
  private static final String NPANDAY_TEST_ROOTNAMESPACE_KEY = "testRootNamespace";

  /**
   * Pattern used to define if a project is a test project or not
   */
  private static String testProjectNamePattern = "*.Tests";

  /**
   * Pattern used to define if a project is an integ test project or not
   */
  private static String integTestProjectNamePattern = null;

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

  public static void setIntegTestProjectNamePattern(String testProjectNamePattern) {
    ModelFactory.integTestProjectNamePattern = testProjectNamePattern;
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
   * @param integTestProjectPatterns
   */
  protected static void assessTestProject(VisualStudioProject visualStudioProject, String testProjectPatterns, String integTestProjectPatterns) {

    String assemblyName = visualStudioProject.getAssemblyName();

    boolean testFlag = nameMatchPatterns(assemblyName, testProjectPatterns);
    boolean integTestFlag = nameMatchPatterns(assemblyName, integTestProjectPatterns);

    if (testFlag) {
      visualStudioProject.setProjectType(ProjectType.UNIT_TEST_PROJECT);
      // TODO why we need this ?
      // if (StringUtils.isEmpty(integTestProjectPatterns)) {
      // visualStudioProject.setProjectType(ProjectType.IT_TEST_PROJECT);
      // }
    }

    if (integTestFlag) {
      visualStudioProject.setProjectType(ProjectType.IT_TEST_PROJECT);
    }

    if (testFlag || integTestFlag) {
      LOG.info("The project '{}' has been qualified as a test project.", visualStudioProject.getName());
    }
  }

  private static boolean nameMatchPatterns(String assemblyName, String testProjectPatterns) {
    if (StringUtils.isEmpty(testProjectPatterns)) {
      return false;
    }
    String[] patterns = StringUtils.split(testProjectPatterns, ";");
    boolean testFlag = false;

    for (int i = 0; i < patterns.length; i++) {
      if (WildcardPattern.create(patterns[i]).match(assemblyName)) {
        testFlag = true;
        break;
      }
    }
    return testFlag;
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
   * @throws DotNetException
   */
  public static VisualStudioSolution getSolution(File baseDirectory, String solutionName) throws IOException, DotNetException {
    File solutionFile = new File(baseDirectory, solutionName);
    return getSolution(solutionFile);
  }

  /**
   * @param solutionFile
   *          the solution file
   * @return a new visual studio solution
   * @throws IOException
   * @throws DotNetException
   */
  public static VisualStudioSolution getSolution(File solutionFile) throws IOException, DotNetException {

    String solutionContent = FileUtils.readFileToString(solutionFile);
    List<BuildConfiguration> buildConfigurations = getBuildConfigurations(solutionContent);

    List<VisualStudioProject> projects = getProjects(solutionFile, solutionContent, buildConfigurations);
    VisualStudioSolution solution = new VisualStudioSolution(solutionFile, projects);
    solution.setBuildConfigurations(buildConfigurations);
    solution.setName(solutionFile.getName());
    return solution;
  }

  private static List<BuildConfiguration> getBuildConfigurations(String solutionContent) {
    // A pattern to extract the build configurations from a visual studio solution
    String confExtractExp = "(\tGlobalSection\\(SolutionConfigurationPlatforms\\).*?^\tEndGlobalSection$)";
    Pattern confExtractPattern = Pattern.compile(confExtractExp, Pattern.MULTILINE + Pattern.DOTALL);
    List<BuildConfiguration> buildConfigurations = new ArrayList<BuildConfiguration>();
    // Extracts all the projects from the solution
    Matcher blockMatcher = confExtractPattern.matcher(solutionContent);
    if (blockMatcher.find()) {
      String buildConfigurationBlock = blockMatcher.group(1);
      String buildConfExtractExp = " = (.*)\\|(.*)";
      Pattern buildConfExtractPattern = Pattern.compile(buildConfExtractExp);
      Matcher buildConfMatcher = buildConfExtractPattern.matcher(buildConfigurationBlock);
      while (buildConfMatcher.find()) {
        String buildConfiguration = buildConfMatcher.group(1);
        String platform = buildConfMatcher.group(2);
        buildConfigurations.add(new BuildConfiguration(buildConfiguration, platform));
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
   * @throws DotNetException
   */
  private static List<VisualStudioProject> getProjects(File solutionFile, String solutionContent, List<BuildConfiguration> buildConfigurations)
      throws IOException, DotNetException {

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

    // This pattern extracts the projects from a Visual Studio solution:
    // 1. normal projects (currently only csproj and vbproj)
    String normalProjectExp = "\\s*Project\\([^\\)]*\\)\\s*=\\s*\"([^\"]*)\"\\s*,\\s*\"([^\"]*?\\.(cs|vb)proj)\"";
    // 2. web projects which have a different declaration structure
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
        if (!projectFile.exists()) {
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
          if (projectPath.endsWith("\\")) {
            projectPath = StringUtils.chop(projectPath);
          }
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
   * @throws DotNetException
   * @throws FileNotFoundException
   */
  public static VisualStudioProject getProject(File projectFile) throws FileNotFoundException, DotNetException {
    String projectName = projectFile.getName();
    return getProject(projectFile, projectName, null);
  }

  /**
   * Generates a list of projects from the path of the visual studio projects files (.*proj)
   *
   * @param projectFile
   *          the project file
   * @param projectName
   *          the name of the project
   * @throws DotNetException
   * @throws FileNotFoundException
   *           if the file was not found
   */
  public static VisualStudioProject getProject(File projectFile, String projectName, List<BuildConfiguration> buildConfigurations)
      throws FileNotFoundException, DotNetException {

    VisualStudioProject project = new VisualStudioProject();
    project.setProjectFile(projectFile);
    project.setName(projectName);
    File projectDir = projectFile.getParentFile();

    XPathFactory factory = XPathFactory.newInstance();
    XPath xpath = factory.newXPath();

    // This is a workaround to avoid Xerces class-loading issues
    try {
      // We define the namespace prefix for Visual Studio
      xpath.setNamespaceContext(new VisualStudioNamespaceContext());

      if (buildConfigurations != null) {
        Map<BuildConfiguration, File> buildConfOutputDirMap = new HashMap<BuildConfiguration, File>();
        for (BuildConfiguration config : buildConfigurations) {
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

      XPathExpression silverlightExpression = xpath.compile("/vst:Project/vst:PropertyGroup/vst:SilverlightApplication");
      XPathExpression projectGuidExpression = xpath.compile("/vst:Project/vst:PropertyGroup/vst:ProjectGuid");

      // Extracts the properties of a Visual Studio Project
      String typeStr = extractProjectProperty(projectTypeExpression, projectFile);
      String silverlightStr = extractProjectProperty(silverlightExpression, projectFile);
      String assemblyName = extractProjectProperty(assemblyNameExpression, projectFile);
      String rootNamespace = extractProjectProperty(rootNamespaceExpression, projectFile);
      String projectGuid = extractProjectProperty(projectGuidExpression, projectFile);

      // because the GUID starts with { and ends with }, remove these characters
      projectGuid = projectGuid.substring(1, projectGuid.length() - 2);

      // Assess if the artifact is a library or an executable
      ArtifactType type = ArtifactType.LIBRARY;
      if (StringUtils.containsIgnoreCase(typeStr, "exe")) {
        type = ArtifactType.EXECUTABLE;
      }
      // The project is populated
      project.setProjectGuid(UUID.fromString(projectGuid));
      project.setProjectFile(projectFile);
      project.setType(type);
      project.setDirectory(projectDir);
      project.setAssemblyName(assemblyName);
      project.setRootNamespace(rootNamespace);

      if (StringUtils.isNotEmpty(silverlightStr)) {
        project.setProjectType(ProjectType.SILVERLIGHT_PROJECT);
      }

      // Get all source files to find the assembly version
      // [assembly: AssemblyVersion("1.0.0.0")]
      Collection<SourceFile> sourceFiles = project.getSourceFiles();
      project.setAssemblyVersion(findAssemblyVersion(sourceFiles));

      assessTestProject(project, testProjectNamePattern, integTestProjectNamePattern);

      return project;
    } catch (XPathExpressionException xpee) {
      throw new DotNetException("Error while processing the project " + projectFile, xpee);
    }
  }

  protected static String findAssemblyVersion(Collection<SourceFile> sourceFiles) {
    String version = null;

    // first parse: in general, it's in the "Properties\AssemblyInfo.*"
    for (SourceFile file : sourceFiles) {
      if (StringUtils.startsWithIgnoreCase(file.getName(), "assemblyinfo")) {
        version = tryToGetVersion(file);
        if (version != null) {
          break;
        }
      }
    }

    // second parse: try to read all files
    for (SourceFile file : sourceFiles) {
      version = tryToGetVersion(file);
      if (version != null) {
        break;
      }
    }
    return version;
  }

  private static String tryToGetVersion(SourceFile file) {
    String content;
    try {
      content = org.apache.commons.io.FileUtils.readFileToString(file.getFile(), "UTF-8");
      if (content.startsWith("\uFEFF") || content.startsWith("\uFFFE")) {
        content = content.substring(1);
      }

      // Search for AssemblyVersion("...") which is not in a comment line (which starts by / in C# or ' in VB)
      Pattern p = Pattern.compile("^[^/']*AssemblyVersion\\(\"([^\"]*)\"\\).*$", Pattern.MULTILINE);
      Matcher m = p.matcher(content);
      if (m.find())
      {
        return m.group(1);
      }

    } catch (IOException e) {
      LOG.warn("Not able to read the file " + file.getFile().getAbsolutePath() + " to find project version", e);
    }
    return null;
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

    Map<BuildConfiguration, File> buildConfOutputDirMap = new HashMap<BuildConfiguration, File>();
    buildConfOutputDirMap.put(new BuildConfiguration("Debug"), new File(solutionRoot, debugOutput));
    buildConfOutputDirMap.put(new BuildConfiguration("Release"), new File(solutionRoot, releaseOutput));
    project.setBuildConfOutputDirMap(buildConfOutputDirMap);

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
   * Gets the relative paths of all the files in a project, as they are defined in the .*proj file.
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
        if (filePath != null) {
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
   * @throws DotNetException
   * @throws FileNotFoundException
   */
  private static String extractProjectProperty(XPathExpression expression, File projectFile) throws DotNetException {
    try {
      FileInputStream file = new FileInputStream(projectFile);
      InputSource source = new InputSource(file);
      return expression.evaluate(source);
    } catch (Exception e) {
      throw new DotNetException("Could not evaluate the expression " + expression + " on project " + projectFile, e);
    }
  }

  /**
   * A Namespace context specialized for the handling of .*proj files
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

  /**
   * Gets the solution from sonar root project.
   * @param root the sonar root project.
   * @param configuration the .net configurations.
   * @return the solution instance.
   */
  public static VisualStudioSolution getSolution(final ProjectDefinition root, DotNetConfiguration configuration) {
    MavenProject mvnRootPrj = extractMavenProject(root);
    List<VisualStudioProject> projects = Lists.newLinkedList();
    List<ProjectDefinition> subProjects = getSubProjects(root);
    for (ProjectDefinition subProject : subProjects) {
      MavenProject mvnSubProject = extractMavenProject(subProject);
      boolean isTestProject = nameMatchPatterns(mvnSubProject.getArtifactId(), testProjectNamePattern + ((integTestProjectNamePattern != null) ? ";" + integTestProjectNamePattern
          : StringUtils.EMPTY));
      if(isTestProject) {
        projects.add(buildVisualStudioTestProject(mvnSubProject, configuration));
      } else {
        projects.add(buildVisualStudioProject(mvnSubProject, configuration));
        if (hasUnitTests(mvnSubProject)) {
          projects.add(buildVisualStudioInnerTestProject(mvnSubProject, configuration));
        }
      }
    }
    // create solution
    VisualStudioSolution solution = new VisualStudioSolution(mvnRootPrj.getFile(), projects);
    // set build configurations
    List<BuildConfiguration> buildConfigurations = Lists.newLinkedList();
    buildConfigurations.add(new BuildConfiguration("Release"));
    solution.setBuildConfigurations(buildConfigurations);
    solution.setName(root.getName());

    return solution;
  }

  /**
   * Check if given project has unit test files.
   * @param mvnProject the maven project.
   * @return <code>true</code> if given project has unit test files, otherwise <code>false</code>.
   */
  private static boolean hasUnitTests(MavenProject mvnProject) {
    boolean hasUnitTests = false;
    List<String> testRoots = mvnProject.getTestCompileSourceRoots();
    for (String testRoot : testRoots) {
      File testDirecory = new File(testRoot);
      if (!testDirecory.exists() || !testDirecory.isDirectory()) {
        continue;
      }
      Collection<File> testFiles = FileUtils.listFiles(testDirecory, new String[] {"cs", "vb"}, true);
      if (!testFiles.isEmpty()) {
        hasUnitTests = true;
        break;
      }
    }

    return hasUnitTests;
  }

  /**
   * Gets all sub project from root.
   * @param root the root project.
   * @return the list of projects.
   */
  private static List<ProjectDefinition> getSubProjects(final ProjectDefinition root) {
    List<ProjectDefinition> projects = Lists.newLinkedList();
    for (ProjectDefinition subProject : root.getSubProjects()) {
      collectProjects(subProject, projects);
    }

    return projects;
  }

  /**
   * Populates list of projects from hierarchy.
   * @param project the project
   * @param collected the list of collected projects.
   */
  private static void collectProjects(ProjectDefinition project, List<ProjectDefinition> collected) {
    collected.add(project);
    for (ProjectDefinition child : project.getSubProjects()) {
      collectProjects(child, collected);
    }
  }

  /**
   * Build Visual Studio Project from given maven project.
   * @param mvnProject the maven project.
   * @param configuration the dotnet configuration.
   */
  public static VisualStudioProject buildVisualStudioProject(MavenProject mvnProject, DotNetConfiguration configuration) {
    VisualStudioProject vsProject = null;
    String mvnPackaging = mvnProject.getPackaging();
    if (NPANDAY_ASP_NET_PACKAGING.equalsIgnoreCase(mvnPackaging)) {
      vsProject = buildVisualStudioWebProject(mvnProject, configuration);
    } else {
      vsProject = buildVisualStudioBaseProject(mvnProject, configuration);
    }
    return vsProject;
  }

  /**
   * Build Visual Studio Web Project based on given maven project.
   * @param mvnProject the maven project.
   * @param configuration the .net configurations.
   * @return the Visual Studio Web project instance.
   */
  private static VisualStudioWebProject buildVisualStudioWebProject(MavenProject mvnProject, DotNetConfiguration configuration) {
    // We define the namespace prefix for Visual Studio
    VisualStudioWebProject webProject = new VisualStudioWebProject();
    webProject.setProjectType(ProjectType.WEB_PROJECT);
    String artifactId = mvnProject.getArtifactId();
    // use artifact id instead of final name, since final name is not been handled by NPanday
    String projectName = artifactId;
    String mvnBuildDir = mvnProject.getBuild().getDirectory();
    final String preferedFileSeparator = "/";
    mvnBuildDir = StringUtils.replace(mvnBuildDir, "\\", preferedFileSeparator);
    // maven relative build directory to project base directory
    String relativeMvnBuildDir = StringUtils.substringAfterLast(mvnBuildDir, preferedFileSeparator);

    webProject.setName(projectName);
    String assemblyName = projectName;
    String rootNamespace = "";
    File outputDir = new File(mvnBuildDir + preferedFileSeparator + artifactId);

    // The project is populated
    webProject.setDirectory(mvnProject.getBasedir());
    webProject.setSourceDirectory(new File(mvnProject.getBuild().getSourceDirectory()));
    webProject.setAssemblyName(assemblyName);
    webProject.setRootNamespace(rootNamespace);

    Map<BuildConfiguration, File> buildConfOutputDirMap = new HashMap<BuildConfiguration, File>();
    buildConfOutputDirMap.put(new BuildConfiguration("Release"), outputDir);
    webProject.setBuildConfOutputDirMap(buildConfOutputDirMap);
    webProject.setForcedOutputDir(relativeMvnBuildDir + preferedFileSeparator + artifactId);
    webProject.setReferenceDirectory(relativeMvnBuildDir + preferedFileSeparator
      + configuration.getString(DotNetConstants.DOTNET_MAVEN_DEPENDENCY_DIR_KEY));
    return webProject;
  }

  /**
   * Build Visual Studio Project based on given maven project.
   * @param mvnProject the maven project.
   * @param configuration the .net configurations.
   */
  private static VisualStudioProject buildVisualStudioBaseProject(MavenProject mvnProject, DotNetConfiguration configuration) {
    VisualStudioProject vsProject = new VisualStudioProject();
    if (isExecutableProject(mvnProject)) {
      vsProject.setType(ArtifactType.EXECUTABLE);
    } else {
      vsProject.setType(ArtifactType.LIBRARY);
    }
    vsProject.setName(mvnProject.getName());
    // use artifact id instead of final name, since final name is not been handled by NPanday
    String artifactId = mvnProject.getArtifactId();
    vsProject.setAssemblyName(artifactId);
    vsProject.setAssemblyVersion(mvnProject.getVersion());
    vsProject.setRootNamespace(findRootNamespace(mvnProject, false));
    vsProject.setDirectory(mvnProject.getBasedir());
    vsProject.setSourceDirectory(new File(mvnProject.getBuild().getSourceDirectory()));
    Map<BuildConfiguration, File> buildConfOutputDirMap = Maps.newHashMap();
    buildConfOutputDirMap.put(new BuildConfiguration("Release"), new File(mvnProject.getBuild().getDirectory()));
    vsProject.setBuildConfOutputDirMap(buildConfOutputDirMap);

    // check if is silverlight project
    if (isSilverlightProject(mvnProject)) {
      vsProject.setProjectType(ProjectType.SILVERLIGHT_PROJECT);
    }

    return vsProject;
  }

  /**
   * Check if is executable project.
   */
  private static boolean isExecutableProject(MavenProject mvnProject) {
    return NPANDAY_DOTNET_EXECUTABLE_PACKAGING.equalsIgnoreCase(mvnProject.getPackaging());
  }

  /**
   * Build Visual Studio UnitTest Project on given maven test project.
   * @param mvnProject the maven project.
   * @param configuration the .net configurations.
   */
  private static VisualStudioProject buildVisualStudioTestProject(MavenProject mvnProject, DotNetConfiguration configuration) {
    VisualStudioProject vsProject = new VisualStudioProject();
    vsProject.setProjectType(ProjectType.UNIT_TEST_PROJECT);
    vsProject.setType(ArtifactType.LIBRARY);
    vsProject.setName(mvnProject.getName());
    // use artifact id instead of final name, since final name is not been handled by NPanday
    String artifactId = mvnProject.getArtifactId();
    vsProject.setAssemblyName(artifactId);
    vsProject.setAssemblyVersion(mvnProject.getVersion());
    vsProject.setRootNamespace(findRootNamespace(mvnProject, false));
    vsProject.setDirectory(mvnProject.getBasedir());
    vsProject.setSourceDirectory(new File(mvnProject.getBuild().getSourceDirectory()));
    Map<BuildConfiguration, File> buildConfOutputDirMap = Maps.newHashMap();
    buildConfOutputDirMap.put(new BuildConfiguration("Release"), new File(mvnProject.getBuild().getDirectory(), NPANDAY_TEST_ASSEMBLIES_DIR));
    vsProject.setBuildConfOutputDirMap(buildConfOutputDirMap);

    return vsProject;
  }

  /**
   * Build Visual Studio UnitTest Project on given maven project.
   * @param mvnProject the maven project.
   * @param configuration the .net configurations.
   */
  private static VisualStudioProject buildVisualStudioInnerTestProject(MavenProject mvnProject, DotNetConfiguration configuration) {
    VisualStudioProject vsProject = new VisualStudioProject();
    vsProject.setProjectType(ProjectType.UNIT_TEST_PROJECT);
    vsProject.setType(ArtifactType.LIBRARY);
    vsProject.setName(mvnProject.getName() + "Test");
    // use artifact id instead of final name, since final name is not been handled by NPanday
    String artifactId = mvnProject.getArtifactId() + "-test";
    vsProject.setAssemblyName(artifactId);
    vsProject.setAssemblyVersion(mvnProject.getVersion());
    vsProject.setRootNamespace(findRootNamespace(mvnProject, true));
    vsProject.setDirectory(mvnProject.getBasedir());
    vsProject.setSourceDirectory(new File(mvnProject.getBuild().getTestSourceDirectory()));
    Map<BuildConfiguration, File> buildConfOutputDirMap = Maps.newHashMap();
    buildConfOutputDirMap.put(new BuildConfiguration("Release"), new File(mvnProject.getBuild().getDirectory(), NPANDAY_TEST_ASSEMBLIES_DIR));
    vsProject.setBuildConfOutputDirMap(buildConfOutputDirMap);

    return vsProject;
  }

  /**
   * Check if is silverlight project.
   */
  private static boolean isSilverlightProject(MavenProject mvnProject) {
    return NPANDAY_SILVERLIGHT_PACKAGING.equalsIgnoreCase(mvnProject.getPackaging());
  }

  /**
   * Find rootnamespace of .net project.
   * @param mvnProject the maven .net project.
   * @param isTestNamespace <code>true</code> if should find rootnamespace for test project.
   * @return the rootnamespace value.
   */
  private static String findRootNamespace(MavenProject mvnProject, boolean isTestNamespace) {
    String rootnamespace = StringUtils.EMPTY;
    Plugin npandayCompilePlugin = mvnProject.getPlugin(NPANDAY_COMPILE_PLUGIN_KEY);
    if (npandayCompilePlugin != null) {
      // Object configuration is a DOM representation.
      Object configuration = npandayCompilePlugin.getConfiguration();
      if (configuration instanceof Xpp3Dom) {
        Xpp3Dom domConfig = (Xpp3Dom) configuration;
        String namespaceKey = isTestNamespace ? NPANDAY_TEST_ROOTNAMESPACE_KEY : NPANDAY_ROOTNAMESPACE_KEY;
        Xpp3Dom rootNamespaceDom = domConfig.getChild(namespaceKey);
        rootnamespace = rootNamespaceDom != null ? rootNamespaceDom.getValue() : StringUtils.EMPTY;
      }
    }

    return rootnamespace;
  }

  /**
   * Extract maven project.
   * @param the sonar project definition.
   * @return maven  project
   * @throws SonarException if maven project doesn't exist in provided sonar project definition.
   */
  public static MavenProject extractMavenProject(ProjectDefinition root) {
    for (Object extension : root.getContainerExtensions()) {
      if (extension instanceof MavenProject) {
        return (MavenProject) extension;
      }
    }
    throw new SonarException("Cannot find required maven project in project '" + root.getName() + "' for NPanday support!");
  }
}
