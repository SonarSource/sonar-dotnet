/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
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
