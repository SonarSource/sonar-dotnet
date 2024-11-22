/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
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

using SonarAnalyzer.Core.Common;

namespace SonarAnalyzer.Core.Test.Common;

[TestClass]
public class NaturalLanguageDetectorTest
{
    [DataTestMethod]
    [DataRow(null, 0)]
    [DataRow("Hello02139710238712987", 1.3262863)]
    [DataRow("This is an english text!", 4.6352161)]
    [DataRow("Hello", 4.7598878)]
    [DataRow("Hello hello hello hello", 4.7598878)]
    [DataRow("Hleol", 2.0215728)]
    [DataRow("Hleol hleol", 2.0215728)]
    [DataRow("Hleol Hello hleol", 2.9343445)]
    [DataRow("Hleol Incomprehensibility hleol", 3.5209606)]
    [DataRow("Incomprehensibility ", 4.3978577)]
    [DataRow("slrwaxquavy", 0.783135)]
    [DataRow("SlRwAxQuAvY", 0.5729079)]
    [DataRow("012345678", 1)]
    [DataRow("images/blob/50281d86d6ed5c61975971150adf", 1.1821769)]
    [DataRow("js/commit/8863b9d04c722b278fa93c5d66ad1e", 0.9126614)]
    [DataRow("net/core/builder/e426a9ae7167c5807b173d5", 1.9399531)]
    [DataRow("net/more/builder/3ad489866f41084fa4f3307", 1.9014789)]
    [DataRow("project/commit/c5acf965067478784b54e2d24", 1.2177787)]
    [DataRow("/var/lib/openshift/51122e382d5271c5ca000", 1.3230153)]
    [DataRow("examples/commit/16ad89c4172c259f15bce56e", 1.6869377)]
    [DataRow("examples/commit/8e1d746900f5411e9700fea0", 1.48724)]
    [DataRow("examples/commit/c95b6a84b6fd1efc832a46cd", 1.503256)]
    [DataRow("examples/commit/d6f6ef7457d99e31990fa64b", 1.4204883)]
    [DataRow("examples/commit/ea15f07ce79366a08fee5b60", 1.8357153)]
    [DataRow("cn/res/chinapostplan/structure/181041269", 3.494024)]
    [DataRow("com/istio/proxy/blob/bcdc1684df0839a6125", 1.5356048)]
    [DataRow("com/kriskowal/q/blob/b0fa72980717dc202ff", 1.3069352)]
    [DataRow("com/ph/logstash/de2ba3f964ae7039b7b74a4a", 1.4612998)]
    [DataRow("default/src/test/java/org/xwiki/componen", 2.6909549)]
    [DataRow("search_my_organization-example.json", 3.6890879)]
    [DataRow("org.apache.tomcat.util.buf.UDecoder.ALLOW_ENCODED_SLASH", 3.5768316)]
    [DataRow("org.apache.catalina.startup.EXIT_ON_INIT_FAILURE", 4.2315959)]
    [DataRow("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/", 1.2038558)]
    [DataRow("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_", 1.2038558)]
    [DataRow("ABCDEFGHIJKLMNOPQRSTUVWXYZ234567", 1.2252754)]
    [DataRow("0123456789ABCDEFGHIJKLMNOPQRSTUV", 1.2310129)]
    [DataRow("abcdefghijklmnopqrstuvwxyz", 1.1127479)]
    [DataRow("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 1.1127479)]
    [DataRow("org.eclipse.jetty.server.HttpChannelState.DEFAULT_TIMEOUT", 3.2985092)]
    [DataRow("org.apache.tomcat.websocket.WS_AUTHENTICATION_PASSWORD", 4.061177)]
    public void CalculateHumanLanguageScore(string input, double expectedScore) =>
        NaturalLanguageDetector.HumanLanguageScore(input).Should().BeApproximately(expectedScore, 0.01);
}
