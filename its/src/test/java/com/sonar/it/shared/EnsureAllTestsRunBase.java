/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2020 SonarSource SA
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
package com.sonar.it.shared;

import io.github.classgraph.AnnotationClassRef;
import io.github.classgraph.ClassGraph;
import io.github.classgraph.ClassInfo;
import io.github.classgraph.ScanResult;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import static org.assertj.core.api.Assertions.assertThat;

public class EnsureAllTestsRunBase {

  protected void assertAllTestClassesAreReferencedInTestSuite(String packageName, String testSuiteClassName)
  {
    try (ScanResult scanResult = new ClassGraph()
      .enableAllInfo()
      .acceptPackages(packageName)
      .scan()) {

      List<String> classNamesInTestsSuiteAnnotation = getClassNamesMentionedInSuiteClasses(scanResult, testSuiteClassName);
      String[] testClassesInPackage = getTestClassesInThisPackage(scanResult, packageName, testSuiteClassName).toArray(new String[0]);

      assertThat(classNamesInTestsSuiteAnnotation).containsExactlyInAnyOrder(testClassesInPackage);
    }
  }

  private List<String> getClassNamesMentionedInSuiteClasses(ScanResult scanResult, String testSuiteClassName)
  {
    Object[] suiteAnnotationParams = (Object[]) scanResult.getClassInfo(testSuiteClassName)
      .getAnnotationInfo("org.junit.runners.Suite$SuiteClasses")
      .getParameterValues()
      .getValue("value");

    List<String> classNamesInTestsSuite = new ArrayList<>();
    for (Object annotation : suiteAnnotationParams)
    {
      classNamesInTestsSuite.add(((AnnotationClassRef)annotation).getName());
    }

    return classNamesInTestsSuite;
  }

  private List<String> getTestClassesInThisPackage(ScanResult scanResult, String packageName, String testSuiteClassName)
  {
    Map<String, ClassInfo> allClasses = scanResult.getAllClassesAsMap();
    List<String> testClassesInPackage = new ArrayList<>();
    for (String name : allClasses.keySet())
    {
      if (name.startsWith(packageName)
        && !name.equals(testSuiteClassName)
        && allClasses.get(name).getMethodAnnotations().getNames().contains("org.junit.Test")) {
        testClassesInPackage.add(allClasses.get(name).getName());
      }
    }
    return testClassesInPackage;
  }
}
