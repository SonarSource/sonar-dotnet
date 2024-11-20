/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import org.junit.Test;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Path;
import java.security.NoSuchAlgorithmException;

import static org.assertj.core.api.Assertions.assertThat;

public class HashProviderTest {
  private static final byte[] HEX_ARRAY = "0123456789abcdef".getBytes(StandardCharsets.US_ASCII);

  @Test
  public void computeHash() throws NoSuchAlgorithmException, IOException {
    HashProvider sut = new HashProvider();

    assertThat(computeHash(sut, "src/test/resources/HashProvider/EmptyWithBom.cs")).isEqualTo("f1945cd6c19e56b3c1c78943ef5ec18116907a4ca1efc40a57d48ab1db7adfc5");
    assertThat(computeHash(sut, "src/test/resources/HashProvider/EmptyNoBom.cs")).isEqualTo("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855");
    assertThat(computeHash(sut, "src/test/resources/HashProvider/CodeWithBom.cs")).isEqualTo("b98aaf2ce5a3f9cdf8ab785563951f2309d577baa6351098f78908300fdc610a");
    assertThat(computeHash(sut, "src/test/resources/HashProvider/CodeNoBom.cs")).isEqualTo("8c7535a8e3679bf8cc241b5749cef5fc38243401556f2b7869495c7b48ee4980");
    assertThat(computeHash(sut, "src/test/resources/HashProvider/Utf8.cs")).isEqualTo("13aa54e315a806270810f3a91501f980a095a2ef1bcc53167d4c750a1b78684d");
    assertThat(computeHash(sut, "src/test/resources/HashProvider/Utf16.cs")).isEqualTo("a9b3c4402770855d090ba4b49adeb5ad601cb3bbd6de18495302f45f242ef932");
    assertThat(computeHash(sut, "src/test/resources/HashProvider/Ansi.cs")).isEqualTo("b965073262109da4f106cd90a5eeea025e2441c244af272537afa2cfb03c3ab8");
  }

  private static String computeHash(HashProvider sut, String fileName) throws NoSuchAlgorithmException, IOException {
    return bytesToHex(sut.computeHash(Path.of(fileName)));
  }

  private static String bytesToHex(byte[] bytes) {
    byte[] hexChars = new byte[bytes.length * 2];
    for (int i = 0; i < bytes.length; i++) {
      int v = bytes[i] & 0xFF; // take care of negative numbers
      hexChars[i * 2] = HEX_ARRAY[v >>> 4];
      hexChars[i * 2 + 1] = HEX_ARRAY[v & 0x0F];
    }
    return new String(hexChars, StandardCharsets.UTF_8);
  }
}
