/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.plugin;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.CoreProperties;
import org.sonar.api.Extension;
import org.sonar.api.Plugin;
import org.sonar.api.Properties;
import org.sonar.api.Property;

import com.sonar.csharp.plugin.colorizer.CSharpSourceCodeColorizer;
import com.sonar.csharp.plugin.cpd.CSharpCPDMapping;

@Properties({
    @Property(key = CSharpConstants.CPD_MINIMUM_TOKENS_PROPERTY, defaultValue = "" + CoreProperties.CPD_MINIMUM_TOKENS_DEFAULT_VALUE,
        name = "Minimum tokens",
        description = "The number of duplicate tokens above which a block is considered as a duplication in a C# program.", global = true,
        project = true),
    @Property(key = CSharpConstants.CPD_IGNORE_LITERALS_PROPERTY, defaultValue = CSharpConstants.CPD_IGNORE_LITERALS_DEFVALUE + "",
        name = "Ignore literals", description = "if true, CPD ignores literal value differences when evaluating a duplicate block. "
            + "This means that 'my first text'; and 'my second text'; will be seen as equivalent.", project = true, global = true) })
public class CSharpPlugin implements Plugin {

  public String getKey() {
    return CSharpConstants.PLUGIN_KEY;
  }

  public String getName() {
    return CSharpConstants.PLUGIN_NAME;
  }

  public String getDescription() {
    return "Analysis of C# projects";
  }

  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> extensions = new ArrayList<Class<? extends Extension>>();
    extensions.add(CSharp.class);
    extensions.add(CSharpCPDMapping.class);
    extensions.add(CSharpSourceCodeColorizer.class);
    extensions.add(CSharpSourceImporter.class);
    extensions.add(CSharpSquidSensor.class);
    return extensions;
  }
}
