/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class ExceptionsNeedStandardConstructorsTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<ExceptionsNeedStandardConstructors>();

        [TestMethod]
        public void ExceptionsNeedStandardConstructors() =>
            builder.AddPaths("ExceptionsNeedStandardConstructors.cs").Verify();

        [TestMethod]
        public void ExceptionsNeedStandardConstructors_InvalidCode() =>
            builder.AddSnippet("""
                public class  : Exception
                {
                    My_07_Exception() {}

                    My_07_Exception(string message) { }

                    My_07_Exception(string message, Exception innerException) {}

                    My_07_Exception(SerializationInfo info, StreamingContext context) {}
                }
                """)
                .VerifyNoIssuesIgnoreErrors();
    }
}
