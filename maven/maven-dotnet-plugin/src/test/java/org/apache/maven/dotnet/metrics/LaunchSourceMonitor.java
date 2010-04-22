/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
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
/*
 * Created on Apr 9, 2009
 *
 */
package org.apache.maven.dotnet.metrics;

import java.io.File;

import org.apache.maven.dotnet.SrcMonCommandGenerator;
import org.codehaus.plexus.util.SelectorUtils;



public class LaunchSourceMonitor
{
  public static void main(String[] args)
  {
    System.out.println("os.arch=" + System.getProperty("os.arch"));
    System.out.println("Match:" + SelectorUtils.match("*-test", "spring-test"));
//    System.out.println("java.version=" + System.getProperty("java.version"));
//    System.out.println("java.vm.version=" + System.getProperty("java.vm.version"));
//    System.out.println("java.vm.specification.version=" + System.getProperty("java.vm.specification.version"));
    //System.out.println("Launching the generation");
    
//    CodeMetricsGenerator generator = new CodeMetricsGenerator();
//    generator.setWorkDirectory(new File("C:\\Work\\Temp"));
//    generator.setCheckPointName("CURRENT_SNAPSHOT");
//    generator.setGeneratedFile("C:\\Work\\Temp\\Source-report.xml");
//    generator.setSourcePath("C:\\Work\\PRB\\dotnet");
//    generator.setSourceMonitorPath("C:\\Program Files\\SourceMonitor\\SourceMonitor.exe");
//    generator.setProjectFile("C:\\Work\\Temp\\report.smp");

    try
    {
//      generator.launch();
      System.out.println("Generation done");
    }
    catch (Exception e)
    {
      e.printStackTrace();
    }
 }
}
