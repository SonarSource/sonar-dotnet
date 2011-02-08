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

package org.sonar.plugin.dotnet.cpd;

import static org.sonar.plugin.dotnet.cpd.Constants.*;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.text.ParseException;


import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.maven.DependsUponMavenPlugin;
import org.sonar.api.batch.maven.MavenPluginHandler;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.utils.XpathParser;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;

import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.StringUtils;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.utils.ParsingUtils;
import org.sonar.api.utils.XmlParserException;



/**
 * Mostly Copy/paste from the flex CpdSensor .
 * 
 * @author Alexandre VICTOOR
 * 
 */
public class CpdSensor implements Sensor, DependsUponMavenPlugin {

  private final DotnetCpdPluginHandler dotnetCpdPluginHandler;
  private final CSharpFileLocator fileLocator;
  
  public CpdSensor(DotnetCpdPluginHandler dotnetCpdPluginHandler, CSharpFileLocator fileLocator) {
    this.dotnetCpdPluginHandler = dotnetCpdPluginHandler;
    this.fileLocator = fileLocator;
  }
  
  public void analyse(Project project, SensorContext context) {
    File xmlFile = new File(project.getFileSystem().getBuildDir(), "cpd.xml");
    collect(xmlFile, project, context);
  }
  
  public MavenPluginHandler getMavenPluginHandler(Project project) {
    String mode = getCpdMode(project);
    final MavenPluginHandler pluginHandlerReturned;
    if (CPD_DEFAULT_MODE.equalsIgnoreCase(mode)) {
      pluginHandlerReturned = dotnetCpdPluginHandler;
    } else {
      pluginHandlerReturned = null;
    }
    return pluginHandlerReturned;
  }
  
  public boolean shouldExecuteOnProject(Project project) {
    String packaging = project.getPackaging();
    // We only accept the "sln" packaging
    String mode = getCpdMode(project);
    return "sln".equals(packaging) && CPD_DEFAULT_MODE.equalsIgnoreCase(mode);
  }
  
  private String getCpdMode(Project project) {
    String mode = project.getConfiguration().getString(CPD_MODE_KEY, CPD_DEFAULT_MODE);
    return mode;
  }
  
  protected void collect(File xmlFile, Project pom, SensorContext context) {
    try {
      if (xmlFile != null && xmlFile.exists()) {
        XpathParser parser = new XpathParser();
        String xml = readXmlWithoutEncodingErrors(xmlFile);
        parser.parse(xml);

        NodeList duplications = parser.executeXPathNodeList("/pmd-cpd/duplication");
        Map<Resource, ClassDuplicationData> duplicationsData = new HashMap<Resource, ClassDuplicationData>();
        for (int i = 0; i < duplications.getLength(); i++) {
          Element duplication = (Element) duplications.item(i);
          NodeList files = parser.executeXPathNodeList(duplication, "file");

          Element fileA = (Element) files.item(0);
          Element fileB = (Element) files.item(1);

          processClassMeasure(context, duplicationsData, fileB, fileA, duplication, pom);
          processClassMeasure(context, duplicationsData, fileA, fileB, duplication, pom);
        }

        for (ClassDuplicationData data : duplicationsData.values()) {
          data.saveUsing(context);
        }
      }
    } catch (ParseException e) {
      throw new XmlParserException(e);
    }
  }

  private String readXmlWithoutEncodingErrors(File file) {
    try {
      // First step : the file is read with system charset encoding. It should resolve the problem in most cases
      String xml = FileUtils.readFileToString(file);

      // second step : remove CDATA nodes that contain wrong characters. Those nodes are not needed by the collector.
      return removeCDataNodes(xml);

    } catch (IOException e) {
      throw new XmlParserException("can not read the file " + file.getAbsolutePath(), e);
    }
  }

  private String removeCDataNodes(String xml) {
    String result = xml;
    String startNode = "<codefragment>";
    String endNode = "</codefragment>";
    String[] subs = StringUtils.substringsBetween(xml, startNode, endNode);
    if (subs != null) {
      for (String sub : subs) {
        result = StringUtils.remove(result, startNode + sub + endNode);
      }
    }
    return result;
  }

  private void processClassMeasure(SensorContext context, Map<Resource, ClassDuplicationData> fileContainer, Element fileEl, Element targetFileEl, Element duplication, Project project) throws ParseException {
    Resource csFile =  fileLocator.locate(project, new File(fileEl.getAttribute("path")), false); 
    Resource targetCsFile = fileLocator.locate(project, new File(targetFileEl.getAttribute("path")), false);
    if (csFile != null && targetCsFile != null) {
      ClassDuplicationData data = fileContainer.get(csFile);
      if (data == null) {
        data = new ClassDuplicationData(csFile, context);
        fileContainer.put(csFile, data);
      }
      data.cumulate(
        targetCsFile,
        ParsingUtils.parseNumber(targetFileEl.getAttribute("line")),
        ParsingUtils.parseNumber(fileEl.getAttribute("line")),
        ParsingUtils.parseNumber(duplication.getAttribute("lines"))
      );
    }
  }

  private static final class ClassDuplicationData {
    protected double duplicatedLines;
    protected double duplicatedBlocks;
    protected Resource resource;
    private SensorContext context;
    private List<StringBuilder> duplicationXMLEntries = new ArrayList<StringBuilder>();

    private ClassDuplicationData(Resource resource, SensorContext context) {
      this.context = context;
      this.resource = resource;
    }

    protected void cumulate(Resource targetResource, Double targetDuplicationStartLine, Double duplicationStartLine, Double duplicatedLines) {
      Resource resolvedResource = context.getResource(targetResource);
      if (resolvedResource != null) {
        StringBuilder xml = new StringBuilder();
        xml.append("<duplication lines=\"").append(duplicatedLines.intValue())
          .append("\" start=\"").append(duplicationStartLine.intValue())
          .append("\" target-start=\"").append(targetDuplicationStartLine.intValue())
          .append("\" target-resource=\"").append(resolvedResource.getEffectiveKey()).append("\"/>");

        duplicationXMLEntries.add(xml);

        this.duplicatedLines += duplicatedLines;
        this.duplicatedBlocks++;
      }
    }

    protected void saveUsing(SensorContext context) {
      context.saveMeasure(resource, CoreMetrics.DUPLICATED_FILES, 1d);
      context.saveMeasure(resource, CoreMetrics.DUPLICATED_LINES, duplicatedLines);
      context.saveMeasure(resource, CoreMetrics.DUPLICATED_BLOCKS, duplicatedBlocks);
      context.saveMeasure(resource, new Measure(CoreMetrics.DUPLICATIONS_DATA, getDuplicationXMLData()));
    }

    private String getDuplicationXMLData() {
      StringBuilder duplicationXML = new StringBuilder("<duplications>");
      for (StringBuilder xmlEntry : duplicationXMLEntries) {
        duplicationXML.append(xmlEntry);
      }
      duplicationXML.append("</duplications>");
      return duplicationXML.toString();
    }
  }

  public String toString() {

    return getClass().getSimpleName();
  }

 

}