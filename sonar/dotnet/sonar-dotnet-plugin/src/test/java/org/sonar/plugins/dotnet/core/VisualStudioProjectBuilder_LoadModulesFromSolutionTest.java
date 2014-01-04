/*
 * Sonar .NET Plugin :: Core
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
package org.sonar.plugins.dotnet.core;
import org.junit.Test ;
import org.junit.Before;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.batch.bootstrap.ProjectReactor;
import org.sonar.api.config.Settings;
import org.sonar.plugins.dotnet.api.DotNetConfiguration;
import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;
import static org.junit.Assert.assertTrue;
import static org.junit.Assert.assertFalse;

import java.io.File;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;

public class VisualStudioProjectBuilder_LoadModulesFromSolutionTest {

    public static final String SONAR_MODULES = "sonar.modules";
    private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
    private ProjectReactor reactor;
    private ProjectDefinition root;
    private File solutionBaseDir;
    private VisualStudioProjectBuilder projectBuilder;
    private Settings settings;

    @Before
    public void initBuilder() {
        microsoftWindowsEnvironment = new MicrosoftWindowsEnvironment();
        settings = Settings.createForComponent(new DotNetCorePlugin());
        root = ProjectDefinition.create();
        reactor = new ProjectReactor(root);
        projectBuilder = new VisualStudioProjectBuilder(reactor,
                new DotNetConfiguration(settings), microsoftWindowsEnvironment);
    }

    @Test
    public void noSettings_ShouldLoadFromSolutionTest() {
        boolean result=shouldLoadModulesFromSolution();
        assertTrue(result);
    }

    @Test
    public void modulesSet_ShouldNotLoadFromSolutionTest() {
        settings.appendProperty(SONAR_MODULES,"some");
        boolean result=shouldLoadModulesFromSolution();
        assertFalse(result);
    }

    @Test
         public void modulesEmpty_ShouldLoadFromSolutionTest() {
        settings.appendProperty(SONAR_MODULES,"");
        boolean result=shouldLoadModulesFromSolution();
        assertTrue(result);
    }

    private Boolean shouldLoadModulesFromSolution() {
        return (Boolean)invokePrivateMethod(projectBuilder,"shouldLoadModulesFromSolution",null);
    }
    private static Object invokePrivateMethod(Object object,String methodName, Object objectParams[])  {
        Object result;
        Method methods[] = object.getClass().getDeclaredMethods();
        Method privateMethod=null ;
        for(Method method: methods) {
            if (method.getName().equals(methodName)) {
                privateMethod=method;

            }
        }

        if(privateMethod == null) {
            throw new RuntimeException("Could not find method " + methodName);
        }
        privateMethod.setAccessible(true);
        try {
        result=privateMethod.invoke(object,objectParams);
        } catch (IllegalAccessException e) {
            throw new RuntimeException(e.getMessage());
        } catch (IllegalArgumentException e) {
            throw new RuntimeException(e.getMessage());
        } catch (InvocationTargetException e) {
            throw new RuntimeException(e.getMessage());
        }
        return result;
    }
}
