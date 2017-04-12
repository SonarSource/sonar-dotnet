/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2017 SonarSource SA
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

import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.symbol.NewSymbol;
import org.sonar.api.batch.sensor.symbol.NewSymbolTable;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.SymbolReferenceInfo;

import java.util.function.Predicate;

import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.toTextRange;

class SymbolRefsImporter extends ProtobufImporter<SonarAnalyzer.SymbolReferenceInfo> {

  private final SensorContext context;

  SymbolRefsImporter(SensorContext context, Predicate<InputFile> inputFileFilter) {
    super(SonarAnalyzer.SymbolReferenceInfo.parser(), context, inputFileFilter, SonarAnalyzer.SymbolReferenceInfo::getFilePath);
    this.context = context;
  }

  @Override
  void consumeFor(InputFile inputFile, SymbolReferenceInfo message) {
    NewSymbolTable symbolTable = context.newSymbolTable().onFile(inputFile);

    for (SonarAnalyzer.SymbolReferenceInfo.SymbolReference tokenInfo : message.getReferenceList()) {
      NewSymbol symbol = symbolTable.newSymbol(toTextRange(inputFile, tokenInfo.getDeclaration()));
      for (SonarAnalyzer.TextRange refTextRange : tokenInfo.getReferenceList()) {
        symbol.newReference(toTextRange(inputFile, refTextRange));
      }
    }
    symbolTable.save();
  }

}
