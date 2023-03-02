/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
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

using System.IO;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class WildcardPatternMatcherTest
    {
        /// <summary>
        /// Based on https://github.com/SonarSource/sonar-plugin-api/blob/master/plugin-api/src/test/java/org/sonar/api/utils/WildcardPatternTest.java.
        /// </summary>
        [DataTestMethod]

        [DataRow("Foo", "Foo", true)]
        [DataRow("foo", "FOO", false)]
        [DataRow("Foo", "Foot", false)]
        [DataRow("Foo", "Bar", false)]

        [DataRow("org/T?st.java", "org/Test.java", true)]
        [DataRow("org/T?st.java", "org/Tost.java", true)]
        [DataRow("org/T?st.java", "org/Teeest.java", false)]

        [DataRow("org/*.java", "org/Foo.java", true)]
        [DataRow("org/*.java", "org/Bar.java", true)]

        [DataRow("org/**", "org/Foo.java", true)]
        [DataRow("org/**", "org/foo/bar.jsp", true)]

        [DataRow("org/**/Test.java", "org/Test.java", true)]
        [DataRow("org/**/Test.java", "org/foo/Test.java", true)]
        [DataRow("org/**/Test.java", "org/foo/bar/Test.java", true)]

        [DataRow("org/**/*.java", "org/Foo.java", true)]
        [DataRow("org/**/*.java", "org/foo/Bar.java", true)]
        [DataRow("org/**/*.java", "org/foo/bar/Baz.java", true)]

        [DataRow("o?/**/*.java", "org/test.java", false)]
        [DataRow("o?/**/*.java", "o/test.java", false)]
        [DataRow("o?/**/*.java", "og/test.java", true)]
        [DataRow("o?/**/*.java", "og/foo/bar/test.java", true)]
        [DataRow("o?/**/*.java", "og/foo/bar/test.jav", false)]

        [DataRow("org/sonar/**", "org/sonar/commons/Foo", true)]
        [DataRow("org/sonar/**", "org/sonar/Foo.java", true)]

        [DataRow("xxx/org/sonar/**", "org/sonar/Foo", false)]

        [DataRow("org/sonar/**/**", "org/sonar/commons/Foo", true)]
        [DataRow("org/sonar/**/**", "org/sonar/commons/sub/Foo.java", true)]

        [DataRow("org/sonar/**/Foo", "org/sonar/commons/sub/Foo", true)]
        [DataRow("org/sonar/**/Foo", "org/sonar/Foo", true)]

        [DataRow("*/foo/*", "org/foo/Bar", true)]
        [DataRow("*/foo/*", "foo/Bar", false)]
        [DataRow("*/foo/*", "foo", false)]
        [DataRow("*/foo/*", "org/foo/bar/Hello", false)]

        [DataRow("hell?", "hell", false)]
        [DataRow("hell?", "hello", true)]
        [DataRow("hell?", "helloworld", false)]

        [DataRow("**/Reader", "java/io/Reader", true)]
        [DataRow("**/Reader", "org/sonar/channel/CodeReader", false)]

        [DataRow("**", "java/io/Reader", true)]

        [DataRow("**/app/**", "com/app/Utils", true)]
        [DataRow("**/app/**", "com/application/MyService", false)]

        [DataRow("**/*$*", "foo/bar", false)]
        [DataRow("**/*$*", "foo/bar$baz", true)]
        [DataRow("a+", "aa", false)]
        [DataRow("a+", "a+", true)]
        [DataRow("[ab]", "a", false)]
        [DataRow("[ab]", "[ab]", true)]

        [DataRow("\\n", "\n", false)]
        [DataRow("foo\\bar", "foo/bar", true)]

        [DataRow("/foo", "foo", true)]
        [DataRow("\\foo", "foo", true)]

        [DataRow("foo\\bar", "foo\\bar", true)]
        [DataRow("foo/bar", "foo\\bar", true)]
        [DataRow("foo\\bar/baz", "foo\\bar\\baz", true)]

        public void IsMatch_MatchesPatternsAsExpected(string pattern, string input, bool expectedResult)
        {
            // The test cases are copied from the plugin-api and the directory separators need replacing as Roslyn will not give us the paths with '/'.
            input = input.Replace("/", Path.DirectorySeparatorChar.ToString());

            WildcardPatternMatcher.IsMatch(pattern, input).Should().Be(expectedResult);
        }
    }
}
