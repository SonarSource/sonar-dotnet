/*
 * Sonar C# Plugin :: Core
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

package org.sonar.plugins.csharp.api;

import java.util.Map;
import java.util.Set;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.BatchExtension;
import org.sonar.api.resources.File;
import org.sonar.api.resources.Resource;
import org.sonar.squid.api.SourceCode;
import org.sonar.squid.api.SourceFile;

import com.google.common.collect.Maps;

/**
 * Class that gives links between logical resources (C# types and members) and their enclosing source files. <br/>
 * It is useful when parsing third-party tool reports that contain references to the logical structure of the code: it is then necessary to
 * find to which physical file a given logical item belongs to, so that it is possible to save measure or violations to the correct Sonar
 * resource.
 */
public class CSharpResourcesBridge implements BatchExtension {

  private static final Logger LOG = LoggerFactory.getLogger(CSharpResourcesBridge.class);
  private Map<String, Resource<?>> logicalToPhysicalResourcesMap = Maps.newHashMap();

  private boolean canIndexFiles = true;

  public CSharpResourcesBridge() {
  }

  /**
   * After invoking this method, the {@link CSharpResourcesBridge} class won't be able to index files anymore: if
   * {@link #indexFile(SourceFile, File)} is called, a {@link IllegalStateException} will be thrown.
   */
  public void lock() {
    this.canIndexFiles = false;
  }

  /**
   * Method used to populate the map with the logical resources found in the Squid file and link them to the Sonar file. <br/>
   * This method must be called only by plugins that have the ability to populate this bridge (e.g. C# Squid Plugin).<br/>
   * <br/>
   * <b>Note</b>: If the CSharpResourcesBridge has been locked (see {@link #lock()}), an {@link IllegalStateException} will be thrown if
   * this method is called.
   * 
   * @param squidFile
   *          the Squid file
   * @param sonarFile
   *          the Sonar file
   * @throws IllegalStateException
   *           if the CSharpResourcesBridge is locked and cannot index more files
   */
  public void indexFile(SourceFile squidFile, File sonarFile) {
    if (canIndexFiles) {
      LOG.debug("C# BRIDGE is indexing {}:", squidFile.getKey());
      indexChildren(squidFile.getChildren(), sonarFile);
    } else {
      throw new IllegalStateException(
          "The CSharpResourcesBridge has been locked to prevent future modifications. It is impossible to index new files.");
    }
  }

  private void indexChildren(Set<SourceCode> sourceCodes, File sonarFile) {
    if (sourceCodes != null) {
      for (SourceCode children : sourceCodes) {
        LOG.debug("  - {}", children.getKey());
        logicalToPhysicalResourcesMap.put(children.getKey(), sonarFile);
        indexChildren(children.getChildren(), sonarFile);
      }
    }
  }

  /**
   * Returns the physical file that contains the definition of the type referenced by its namespace and its name.
   * 
   * @param namespaceName
   *          the namespace of the type
   * @param typeName
   *          the type name
   * @return the resource that contains this type, or NULL if none
   */
  public Resource<?> getFromTypeName(String namespaceName, String typeName) {
    StringBuilder typeFullName = new StringBuilder();
    if ( !StringUtils.isEmpty(namespaceName)) {
      typeFullName.append(namespaceName);
      typeFullName.append(".");
    }
    typeFullName.append(typeName);
    return getFromTypeName(typeFullName.toString());
  }

  /**
   * Returns the physical file that contains the definition of the type referenced by its full name.
   * 
   * @param typeFullName
   *          the type full name
   * @return the resource that contains this type, or NULL if none
   */
  public Resource<?> getFromTypeName(String typeFullName) {
    return logicalToPhysicalResourcesMap.get(typeFullName);
  }

  /**
   * /!\ Do not use for the moment! <br>
   * <br>
   * For the moment, method key ends with ':XXXX', where 'XXXX' is the line number, so this API does not work. <br>
   * <br>
   * TODO: Need to work on that.
   * 
   * @param memberFullName
   * @return
   */
  public Resource<?> getFromMemberName(String memberFullName) {
    return logicalToPhysicalResourcesMap.get(memberFullName);
  }

}
