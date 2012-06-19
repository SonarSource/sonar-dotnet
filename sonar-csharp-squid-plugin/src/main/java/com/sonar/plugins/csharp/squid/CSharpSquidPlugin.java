/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid;

import com.sonar.plugins.csharp.squid.colorizer.CSharpSourceCodeColorizer;
import com.sonar.plugins.csharp.squid.cpd.CSharpCPDMapping;
import org.sonar.api.*;

import java.util.ArrayList;
import java.util.List;

@Properties({
  @Property(key = CSharpSquidConstants.CPD_MINIMUM_TOKENS_PROPERTY, defaultValue = "" + CoreProperties.CPD_MINIMUM_TOKENS_DEFAULT_VALUE,
    name = "Minimum tokens",
    description = "The number of duplicate tokens above which a block is considered as a duplication in a C# program.", global = true,
    project = true),
  @Property(key = CSharpSquidConstants.CPD_IGNORE_LITERALS_PROPERTY, defaultValue = CSharpSquidConstants.CPD_IGNORE_LITERALS_DEFVALUE
    + "", name = "Ignore literals", description = "if true, CPD ignores literal value differences when evaluating a duplicate block. "
    + "This means that 'my first text'; and 'my second text'; will be seen as equivalent.", project = true, global = true),
  @Property(
    key = CSharpSquidConstants.IGNORE_HEADER_COMMENTS,
    defaultValue = "true",
    name = "Ignore header comments",
    description = "Set to 'true' to enable, or 'false' to disable.",
    project = true, global = true)
})
public class CSharpSquidPlugin extends SonarPlugin {

  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> extensions = new ArrayList<Class<? extends Extension>>();
    extensions.add(CSharpCPDMapping.class);
    extensions.add(CSharpSourceCodeColorizer.class);
    extensions.add(CSharpSquidSensor.class);

    // rules
    extensions.add(CSharpRuleRepository.class);
    extensions.add(CSharpRuleProfile.class);

    return extensions;
  }
}
