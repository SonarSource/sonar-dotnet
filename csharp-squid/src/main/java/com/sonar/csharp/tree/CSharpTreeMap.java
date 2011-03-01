/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree;

import java.util.HashMap;
import java.util.Set;

import org.sonar.api.resources.File;
import org.sonar.api.resources.Resource;
import org.sonar.squid.api.SourceCode;
import org.sonar.squid.api.SourceFile;

import com.google.common.collect.Maps;

/**
 * Map that keeps track of links between logical resources (types and members) and their enclosing source files.
 */
public class CSharpTreeMap {

  private HashMap<String, Resource<?>> logicalToPhysicalResourcesMap;

  public CSharpTreeMap() {
    logicalToPhysicalResourcesMap = Maps.newHashMap();
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
   * @param namespaceName the namespace of the type 
   * @param typeName the type name
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
