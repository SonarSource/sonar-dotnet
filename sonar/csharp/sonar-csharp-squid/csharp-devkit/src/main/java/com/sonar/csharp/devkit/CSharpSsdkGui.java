/*
 * Sonar C# Plugin :: C# Squid :: Devkit
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
package com.sonar.csharp.devkit;

import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.devkit.SsdkGui;

public final class CSharpSsdkGui {

  private CSharpSsdkGui() {
  }

  public static void main(String[] args) {
    System.setProperty("com.apple.mrj.application.apple.menu.about.name", "SSDK");
    SsdkGui ssdkGui = new SsdkGui(CSharpParser.create(), CSharpSourceCodeColorizer.getTokenizers());
    ssdkGui.setVisible(true);
    ssdkGui.setSize(1000, 800);
    ssdkGui.setTitle("C# : SonarSource Development Kit");
  }

}
