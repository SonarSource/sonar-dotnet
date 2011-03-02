/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.api.tree;

import java.util.HashMap;
import java.util.Set;

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
public class CSharpResourcesBridge {

  private static CSharpResourcesBridge instance;

  private HashMap<String, Resource<?>> logicalToPhysicalResourcesMap;

  private CSharpResourcesBridge() {
    logicalToPhysicalResourcesMap = Maps.newHashMap();
  }

  public static CSharpResourcesBridge getInstance() {
    if (instance == null) {
      synchronized (CSharpResourcesBridge.class) {
        instance = new CSharpResourcesBridge();
      }
    }
    return instance;
  }

  /**
   * Method used to populate the map with the logical resources found in the Squid file and link them to the Sonar file.
   * 
   * @param squidFile
   *          the Squid file
   * @param sonarFile
   *          the Sonar file
   */
  public void indexFile(SourceFile squidFile, File sonarFile) {
    indexChildren(squidFile.getChildren(), sonarFile);
  }

  private void indexChildren(Set<SourceCode> sourceCodes, File sonarFile) {
    if (sourceCodes != null) {
      for (SourceCode children : sourceCodes) {
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
    return getFromTypeName(namespaceName + "." + typeName);
  }

  public Resource<?> getFromTypeName(String typeFullName) {
    return logicalToPhysicalResourcesMap.get(typeFullName);
  }

  public Resource<?> getFromMemberName(String memberFullName) {
    return logicalToPhysicalResourcesMap.get(memberFullName);
  }

}
