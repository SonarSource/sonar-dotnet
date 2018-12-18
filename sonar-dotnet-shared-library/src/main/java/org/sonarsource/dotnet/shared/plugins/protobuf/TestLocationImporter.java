/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2018 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonarsource.dotnet.shared.plugins.protobuf;

import com.google.protobuf.Parser;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.TestLocation;

public class TestLocationImporter extends RawProtobufImporter<TestLocation> {

  private final Map<String, List<String>> testNamesPerFile = new HashMap<>();

  TestLocationImporter(Parser<TestLocation> parser) {
    super(parser);
  }

  @Override
  void consume(TestLocation message) {
    testNamesPerFile.put(message.getFilePath(), message.getTestNamesList());
  }

  public Map<String, List<String>> getTestNamesPerFile() {
    return Collections.unmodifiableMap(testNamesPerFile);
  }
}
