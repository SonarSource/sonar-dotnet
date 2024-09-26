/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonarsource.dotnet.shared;

import org.junit.Test;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertThrows;
import static org.sonarsource.dotnet.shared.CallableUtils.lazy;

public class CallableUtilsTests {

    @Test
    public void lazy_null(){
        var sut = lazy(() -> null);
        assertEquals("null", sut.toString());
    }

    @Test
    public void lazy_value(){
        var sut = lazy(() -> 1);
        assertEquals("1", sut.toString());
    }

    @Test
    public void lazy_throws(){
        var message = "message";
        var sut = lazy(() -> throwingMethod(message));

        var exception = assertThrows(LazyCallException.class, sut::toString);
        assertEquals("An error occurred when calling a lazy operation", exception.getMessage());
        assertEquals(message, exception.getCause().getMessage());
    }

    private int throwingMethod(String message) {
        throw new RuntimeException(message);
    }
}
