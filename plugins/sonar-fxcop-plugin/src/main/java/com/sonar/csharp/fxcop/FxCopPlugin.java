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
import org.sonar.api.Properties;
import org.sonar.api.Property;

import com.sonar.csharp.fxcop.profiles.FxCopProfileExporter;
import com.sonar.csharp.fxcop.profiles.FxCopProfileImporter;
import com.sonar.csharp.fxcop.profiles.SonarWay2Profile;
import com.sonar.csharp.fxcop.profiles.SonarWayProfile;
import com.sonar.csharp.fxcop.runner.FxCopRunner;

/**
 * Main class of the FxCop plugin.
 */
@Properties({
    @Property(key = FxCopConstants.EXECUTABLE_KEY, defaultValue = FxCopConstants.EXECUTABLE_DEFVALUE, name = "FxCop executable",
        description = "Absolute path of the FxCop program.", global = true, project = false),
    @Property(key = FxCopConstants.ASSEMBLIES_TO_SCAN_KEY, defaultValue = FxCopConstants.ASSEMBLIES_TO_SCAN_DEFVALUE,
        name = "Assemblies to scan", description = "Comma-seperated list of paths of assemblies that must be scanned.", global = false,
        project = true),
    @Property(key = FxCopConstants.ASSEMBLY_DEPENDENCY_DIRECTORIES_KEY,
        defaultValue = FxCopConstants.ASSEMBLY_DEPENDENCY_DIRECTORIES_DEFVALUE, name = "Assembly dependency directories",
        description = "Comma-seperated list of folders to search for assembly dependencies.", global = true, project = true),
    @Property(key = FxCopConstants.IGNORE_GENERATED_CODE_KEY, defaultValue = FxCopConstants.IGNORE_GENERATED_CODE_DEFVALUE + "",
        name = "Ignore generated code", description = "Suppress analysis results against generated code.", global = true, project = true),
    @Property(key = FxCopConstants.TIMEOUT_MINUTES_KEY, defaultValue = FxCopConstants.TIMEOUT_MINUTES_DEFVALUE + "",
        name = "FxCop program timeout", description = "Maximum number of minutes before the FxCop program will be stopped.", global = true,
        project = true) })
public class FxCopPlugin implements Plugin {

  /**
   * {@inheritDoc}
   */
  public String getKey() {
    return FxCopConstants.PLUGIN_KEY;
  }

  /**
   * {@inheritDoc}
   */
  public String getName() {
    return FxCopConstants.PLUGIN_NAME;
  }

  /**
   * {@inheritDoc}
   */
  public String getDescription() {
    return "FxCop is an application that analyzes managed code assemblies (code that targets the .NET Framework common language runtime) "
        + "and reports information about the assemblies, such as possible design, localization, performance, and security improvements. "
        + "You can find more by going to the <a href='http://msdn.microsoft.com/en-us/library/bb429476(v=VS.80).aspx'>FxCop web site</a>.";
  }

  /**
   * {@inheritDoc}
   */
  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(FxCopSensor.class);

    // Rules and profiles
    list.add(FxCopRuleRepository.class);
    list.add(FxCopProfileImporter.class);
    list.add(FxCopProfileExporter.class);
    list.add(SonarWayProfile.class);
    list.add(SonarWay2Profile.class);

    // Running FxCop
    list.add(FxCopRunner.class);
    return list;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public String toString() {
    return getKey();
  }
}
