/*
 * Sonar C# Plugin :: NDeps
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
package org.sonar.plugins.csharp.ndeps.results;

import java.io.File;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamException;

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
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.api.CSharpResourcesBridge;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;

public class NDepsResultParser implements BatchExtension {

  private static final Logger LOG = LoggerFactory.getLogger(NDepsResultParser.class);

  private final CSharpResourcesBridge csharpResourcesBridge;

  private final SensorContext context;

  private final Project project;

  private final VisualStudioSolution vsSolution;

  private final VisualStudioProject vsProject;

  public NDepsResultParser(MicrosoftWindowsEnvironment env, CSharpResourcesBridge csharpResourcesBridge, Project project, SensorContext context) {
    super();
    this.csharpResourcesBridge = csharpResourcesBridge;
    this.context = context;
    this.project = project;
    vsSolution = env.getCurrentSolution();
    if (vsSolution == null) {
      // not a C# project
      vsProject = null;
    } else {
      vsProject = vsSolution.getProjectFromSonarProject(project);

    }
  }

  public void parse(String scope, File file) {
    SMInputFactory inputFactory = new SMInputFactory(XMLInputFactory.newInstance());
    try {
      SMHierarchicCursor cursor = inputFactory.rootElementCursor(file);
      SMInputCursor assemblyCursor = cursor.advance().descendantElementCursor("Assembly");
      parseAssemblyBlocs(scope, assemblyCursor);
      cursor.getStreamReader().closeCompletely();
    } catch (XMLStreamException e) {
      throw new SonarException("Error while reading NDeps result file: " + file.getAbsolutePath(), e);
    }
  }

  private void parseAssemblyBlocs(String scope, SMInputCursor cursor) throws XMLStreamException {

    // Cursor is on <Assembly>
    while (cursor.getNext() != null) {
      if (cursor.getCurrEvent().equals(SMEvent.START_ELEMENT)) {

        String assemblyName = cursor.getAttrValue("name");
        String assemblyVersion = cursor.getAttrValue("version");

        final Resource<?> from;
        VisualStudioProject vsProjectFromReport = vsSolution.getProject(assemblyName);
        if (vsProject.equals(vsProjectFromReport)) {
          // direct dependencies of current project
          from = project;
        } else if (vsProjectFromReport == null) {
          // indirect dependencies
          from = getResource(assemblyName, assemblyVersion);
        } else {
          // dependencies of other projects from the same solution
          // (covered by the analysis of these same projects)
          from = null;
        }

        if (from != null) {
          SMInputCursor childCursor = cursor.childElementCursor();
          while (childCursor.getNext() != null) {
            if ("References".equals(childCursor.getLocalName())) {
              SMInputCursor referenceCursor = childCursor.childElementCursor();
              parseReferenceBlock(scope, referenceCursor, from);
            }
            else if ("TypeReferences".equals(childCursor.getLocalName())) {
              SMInputCursor typeReferenceCursor = childCursor.childElementCursor();
              parseTypeReferenceBlock(typeReferenceCursor);
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

        LOG.debug("Saving dependency from {} to {}", from.getName(), to.getName());
      }
    }
  }

  private void parseTypeReferenceBlock(SMInputCursor cursor) throws XMLStreamException {
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
              // get the parent folder
              Resource<?> fromParentFolderResource = (Resource<?>) fromResource.getParent();
              Resource<?> toParentFolderResource = (Resource<?>) toResource.getParent();

              // find the folder to folder dependency
              Dependency folderDependency = findDependency(fromParentFolderResource, toParentFolderResource);
              if (folderDependency == null) {
                folderDependency = new Dependency(fromParentFolderResource, toParentFolderResource);
                folderDependency.setUsage("USES");
              }

              // save it
              folderDependency.setWeight(folderDependency.getWeight() + 1);
              context.saveDependency(folderDependency);
              LOG.debug("Saving dependency from {} to {}", fromParentFolderResource.getName(), toParentFolderResource.getName());

              // save the file to file dependency
              Dependency fileDependency = new Dependency(fromResource, toResource);
              fileDependency.setParent(folderDependency);
              fileDependency.setUsage("USES");
              fileDependency.setWeight(1);
              context.saveDependency(fileDependency);
              LOG.debug("Saving dependency from {} to {}", fromResource.getName(), toResource.getName());
            }
          }
        }
      }
    }
  }

  private Dependency findDependency(Resource<?> from, Resource<?> to) {
    for (Dependency d : context.getDependencies()) {
      if (d.getFrom().equals(from) && d.getTo().equals(to)) {
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
