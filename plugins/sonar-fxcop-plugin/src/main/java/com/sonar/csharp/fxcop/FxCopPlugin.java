/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Plugin;

import com.sonar.csharp.fxcop.maven.FxCopPluginHandler;
import com.sonar.csharp.fxcop.profiles.FxCopProfileImporter;
import com.sonar.csharp.fxcop.profiles.SonarWay2Profile;
import com.sonar.csharp.fxcop.profiles.SonarWayProfile;

/**
 */
public class FxCopPlugin implements Plugin {

  public String getKey() {
    return Constants.PLUGIN_KEY;
  }

  public String getName() {
    return Constants.PLUGIN_NAME;
  }

  public String getDescription() {
    return "FxCop is an application that analyzes managed code assemblies (code that targets the .NET Framework common language runtime) "
        + "and reports information about the assemblies, such as possible design, localization, performance, and security improvements. "
        + "You can find more by going to the <a href='http://msdn.microsoft.com/en-us/library/bb429476(v=VS.80).aspx'>FxCop web site</a>.";
  }

  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(FxCopSensor.class);

    // Rules and profiles
    list.add(FxCopRuleRepository.class);
    list.add(FxCopProfileImporter.class);
    list.add(SonarWayProfile.class);
    list.add(SonarWay2Profile.class);

    list.add(FxCopPluginHandler.class); // TODO remove later
    list.add(FxCopExecutor.class);
    return list;
  }

  @Override
  public String toString() {
    return getKey();
  }
}
