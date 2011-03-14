/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

package org.sonar.plugin.dotnet.coverage;

import org.w3c.dom.Element;

@Deprecated
public class NCover3ParsingStrategy extends AbstractParsingStrategy {

  public NCover3ParsingStrategy() {
    setFilePath("/coverage/documents/doc[@url!=\"None\"]");
    setMethodPath("/coverage/module/class/method");
    setPointElement("seqpnt");
    setFileIdPointAttribute("doc");
    setCountVisitsPointAttribute("vc");
    setStartLinePointAttribute("l");
    setEndLinePointAttribute("el");
  }

  @Override
  String findAssemblyName(Element methodElement) {
    Element moduleElement = (Element) methodElement.getParentNode()
        .getParentNode();
    return moduleElement.getAttribute("assembly");
  }

  @Override
  boolean isCompatible(Element rootElement) {
    return "coverage".equals(rootElement.getNodeName());
  }

}
