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
package org.sonar.plugins.dotnet.api;

import com.google.common.annotations.Beta;
import org.sonar.api.resources.Resource;

/**
 * Classes that implement this interface give links between logical resources (types and members) and their enclosing source files.
 * <br/>
 * <br/>
 * It is useful when parsing third-party tool reports that contain references to the logical structure of the code: it is then necessary to
 * find to which physical file a given logical item belongs to, so that it is possible to save measure or violations to the correct Sonar
 * resource.
 * <br/>
 * <br/>
 * <b>Note: this API is still in beta and needs to be reworked to be really useful.</b>
 */
@Beta
public interface DotNetResourceBridge {

  /**
   * Returns the key of the language to which this resource bridge applies
   *
   * @return the key of the language
   */
  String getLanguageKey();

  /**
   * Returns the physical file that contains the definition of the type referenced by its namespace and its name.
   *
   * @param namespaceName
   *          the namespace of the type
   * @param typeName
   *          the type name
   * @return the resource that contains this type, or NULL if none
   */
  Resource getFromTypeName(String namespaceName, String typeName);

  /**
   * Returns the physical file that contains the definition of the type referenced by its full name.
   *
   * @param typeFullName
   *          the type full name
   * @return the resource that contains this type, or NULL if none
   */
  Resource getFromTypeName(String typeFullName);

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
  Resource getFromMemberName(String memberFullName);

}
