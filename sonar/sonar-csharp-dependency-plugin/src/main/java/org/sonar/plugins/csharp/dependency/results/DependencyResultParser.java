/*
 * Sonar C# Plugin :: Dependency
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

package org.sonar.plugins.csharp.dependency.results;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.InputStreamReader;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamException;

import org.apache.commons.io.IOUtils;
import org.apache.commons.lang.StringUtils;
import org.codehaus.staxmate.SMInputFactory;
import org.codehaus.staxmate.in.SMEvent;
import org.codehaus.staxmate.in.SMHierarchicCursor;
import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.BatchExtension;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.design.Dependency;
import org.sonar.api.resources.Library;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.utils.SonarException;
import org.sonar.plugins.csharp.api.CSharpResourcesBridge;
import org.sonar.plugins.csharp.core.AbstractStaxParser;

public class DependencyResultParser implements BatchExtension {

  private static final Logger LOG = LoggerFactory.getLogger(DependencyResultParser.class);

  private final CSharpResourcesBridge csharpResourcesBridge;

  private final SensorContext context;
  
  private final Project project;
 
  public DependencyResultParser(CSharpResourcesBridge csharpResourcesBridge, Project project, SensorContext context) {
    super();
    this.csharpResourcesBridge = csharpResourcesBridge;
    this.context = context;
    this.project = project;
  }
  
  public void parse(String scope, File file) {
    SMInputFactory inputFactory = new SMInputFactory(XMLInputFactory.newInstance());;
    try {
      SMHierarchicCursor cursor = inputFactory.rootElementCursor(file);
      SMInputCursor assemblyCursor = cursor.advance().descendantElementCursor("Assembly");
      parseAssemblyBlocs(scope, assemblyCursor);
      cursor.getStreamReader().closeCompletely();
    } catch (XMLStreamException e) {
      throw new SonarException("Error while reading dependencyparser result file: " + file.getAbsolutePath(), e);
    } 
  }

  private void parseAssemblyBlocs(String scope, SMInputCursor cursor) throws XMLStreamException {

    // Cursor is on <Assembly>
    while (cursor.getNext() != null) {
      if (cursor.getCurrEvent().equals(SMEvent.START_ELEMENT)) {

        String assemblyName = cursor.getAttrValue("name");
        String assemblyVersion = cursor.getAttrValue("version");

        Resource<?> from = getResource(assemblyName, assemblyVersion);

        // ignore references from another project of the solution, because we will resolve that later
        if (from instanceof Project && !from.equals(project)) {
          from = null; // TODO what is the point of assigning null ?
        } else {
          SMInputCursor childCursor = cursor.childElementCursor();
          while (childCursor.getNext() != null) {
            if (childCursor.getLocalName().equals("References")) {
              SMInputCursor referenceCursor = childCursor.childElementCursor();
              parseReferenceBlock(scope, referenceCursor, from);
            }
            else if (childCursor.getLocalName().equals("TypeReferences")) {
              SMInputCursor typeReferenceCursor = childCursor.childElementCursor();
              parseTypeReferenceBlock(typeReferenceCursor, from);
            }
          }
        }
      }
    }
  }

  private void parseReferenceBlock(String scope, SMInputCursor cursor, Resource<?> from) throws XMLStreamException {
    // Cursor is on <Reference>
    while (cursor.getNext() != null) {
      if (cursor.getCurrEvent().equals(SMEvent.START_ELEMENT)) {
        String referenceName = cursor.getAttrValue("name");
        String referenceVersion = cursor.getAttrValue("version");

        Resource<?> to = getResource(referenceName, referenceVersion);

        // keep the dependency in cache
        Dependency dependency = new Dependency(from, to);
        dependency.setUsage(scope);
        dependency.setWeight(1);
        context.saveDependency(dependency);
        
        LOG.info("Saving dependency from " + from.getName() + " to " + to.getName());
      }
    }
  }

  private void parseTypeReferenceBlock(SMInputCursor cursor, Resource<?> from) throws XMLStreamException {
    // Cursor is on <From>
    while (cursor.getNext() != null) {
      if (cursor.getCurrEvent().equals(SMEvent.START_ELEMENT)) {
        String fromType = cursor.getAttrValue("fullname");

        SMInputCursor toCursor = cursor.childElementCursor();
        while (toCursor.getNext() != null) {
          if (toCursor.getCurrEvent().equals(SMEvent.START_ELEMENT)) {
            String toType = toCursor.getAttrValue("fullname");

            Resource<?> fromResource = csharpResourcesBridge.getFromTypeName(fromType);
            Resource<?> toResource = csharpResourcesBridge.getFromTypeName(toType);

            // check if the source is not filtered
            if (fromResource != null && toResource != null) {

              // get the real resource
              fromResource = context.getResource(fromResource);
              toResource = context.getResource(toResource);

              // get the parent folder
              Resource<?> fromParentFolderResource = (Resource<?>)fromResource.getParent();
              Resource<?> toParentFolderResource = (Resource<?>)toResource.getParent();
              
              // find the folder to folder dependency
              Dependency folderDependency = findDependency(fromParentFolderResource, toParentFolderResource);
              if(folderDependency == null){
                folderDependency = new Dependency(fromParentFolderResource, toParentFolderResource);
                folderDependency.setUsage("USES");
              }
              
              // save it
              folderDependency.setWeight(folderDependency.getWeight()+1);
              context.saveDependency(folderDependency);
              
              // save the file to file dependency
              Dependency fileDependency = new Dependency(fromResource, toResource);
              fileDependency.setParent(folderDependency);
              fileDependency.setUsage("USES");
              fileDependency.setWeight(1);
              context.saveDependency(fileDependency);
            }
          }
        }
      }
    }
  }
  
  private Dependency findDependency(Resource<?> from, Resource<?> to){
    for(Dependency d : context.getDependencies()){
      if(d.getFrom().equals(from) && d.getTo().equals(to)){
        return d;
      }
    }
    
    return null;
  }

  private Resource<?> getProjectFromKey(Project parentProject, String projectKey) {
    for (Project subProject : parentProject.getModules()) {
      if (subProject.getKey().equals(projectKey)) {
        return subProject;
      }
    }

    return null;
  }

  private Resource<?> getResource(String name, String version) {
    // try to find the project
    String projectKey = StringUtils.substringBefore(project.getParent().getKey(), ":") + ":" + StringUtils.deleteWhitespace(name);
    Resource<?> result = getProjectFromKey(project.getParent(), projectKey);

    // if not found in the solution, get the binary
    if (result == null) {

      Library library = new Library(name, version); // key, version
      library.setName(name);
      result = context.getResource(library);

      // not already exists, save it
      if (result == null) {
        context.index(library);
      }
      result = library;

    }

    return result;
  }
}
