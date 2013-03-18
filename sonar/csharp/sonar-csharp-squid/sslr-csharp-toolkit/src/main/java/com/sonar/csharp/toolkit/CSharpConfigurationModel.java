/*
 * Sonar C# Plugin :: C# Squid :: Toolkit
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
package com.sonar.csharp.toolkit;

import com.google.common.annotations.VisibleForTesting;
import com.google.common.base.Preconditions;
import com.google.common.base.Splitter;
import com.google.common.collect.ImmutableList;
import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.impl.Parser;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.colorizer.Tokenizer;
import org.sonar.sslr.toolkit.AbstractConfigurationModel;
import org.sonar.sslr.toolkit.ConfigurationProperty;
import org.sonar.sslr.toolkit.ValidationCallback;
import org.sonar.sslr.toolkit.Validators;

import java.io.File;
import java.nio.charset.Charset;
import java.util.List;

public class CSharpConfigurationModel extends AbstractConfigurationModel {

  private static final Logger LOG = LoggerFactory.getLogger(CSharpConfigurationModel.class);

  private static final String CHARSET_PROPERTY_KEY = "sonar.sourceEncoding";
  private static final String LIB_DIRECTORIES = "sonar.cpp.library.directories";

  @VisibleForTesting
  ConfigurationProperty charsetProperty = new ConfigurationProperty("Charset", CHARSET_PROPERTY_KEY,
      getPropertyOrDefaultValue(CHARSET_PROPERTY_KEY, "UTF-8"),
      Validators.charsetValidator());
  @VisibleForTesting
  ConfigurationProperty libDirectoriesProperty = new ConfigurationProperty("Library directories", LIB_DIRECTORIES,
      getPropertyOrDefaultValue(LIB_DIRECTORIES, ""),
      new ValidationCallback() {

        public String validate(String newValueCandidate) {
          try {
            toFiles(newValueCandidate);
            return "";
          } catch (Exception e) {
            return e.getMessage();
          }
        }

      });

  // FIXME SSLR 1.18
  /*
   * @Override
   * public Charset getCharset() {
   * return Charset.forName(charsetProperty.getValue());
   * }
   */

  public List<ConfigurationProperty> getProperties() {
    ImmutableList.Builder<ConfigurationProperty> builder = ImmutableList.builder();

    builder.add(
        charsetProperty,
        libDirectoriesProperty);

    return builder.build();
  }

  @Override
  public Parser<? extends Grammar> doGetParser() {
    return CSharpParser.create(getConfiguration());
  }

  @Override
  public List<Tokenizer> doGetTokenizers() {
    return CSharpColorizer.getTokenizers();
  }

  @VisibleForTesting
  CSharpConfiguration getConfiguration() {
    return new CSharpConfiguration(Charset.forName(charsetProperty.getValue()));
  }

  @VisibleForTesting
  static String getPropertyOrDefaultValue(String propertyKey, String defaultValue) {
    String propertyValue = System.getProperty(propertyKey);

    if (propertyValue == null) {
      LOG.info("The property \"" + propertyKey + "\" is not set, using the default value \"" + defaultValue + "\".");
      return defaultValue;
    } else {
      LOG.info("The property \"" + propertyKey + "\" is set, using its value \"" + propertyValue + "\".");
      return propertyValue;
    }
  }

  private List<File> toFiles(String value) {
    ImmutableList.Builder<File> builder = ImmutableList.builder();

    for (String path : Splitter.on(',').omitEmptyStrings().trimResults().split(value)) {
      File file = new File(path);
      Preconditions.checkArgument(file.exists(), "Not exists: " + path);
      Preconditions.checkArgument(file.isDirectory(), "Not a folder: " + path);
      builder.add(file);
    }

    return builder.build();
  }

}
