/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class InvocationResolvesToOverrideWithParamsTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<InvocationResolvesToOverrideWithParams>();

        [TestMethod]
        public void InvocationResolvesToOverrideWithParams()
        {
            var anotherAssembly = TestCompiler.CompileCS("""
                public class FromAnotherAssembly
                {
                    protected int ProtectedOverload(object a, string b) => 42;
                    public int ProtectedOverload(string a, params string[] bs) => 42;

                    private protected int PrivateProtectedOverload(object a, string b) => 42;
                    public int PrivateProtectedOverload(string a, params string[] bs) => 42;

                    protected internal int ProtectedInternalOverload(object a, string b) => 42;
                    public int ProtectedInternalOverload(string a, params string[] bs) => 42;

                    internal int InternalOverload(object a, string b) => 42;
                    public int InternalOverload(string a, params string[] bs) => 42;
                }
                """).Model.Compilation.ToMetadataReference();
            builder.AddPaths("InvocationResolvesToOverrideWithParams.cs")
                .AddReferences(new[] { anotherAssembly })
                .WithOptions(LanguageOptions.FromCSharp8)
                .Verify();
        }

#if NET

        [TestMethod]
        public void InvocationResolvesToOverrideWithParams_TopLevelStatements() =>
            builder.AddPaths("InvocationResolvesToOverrideWithParams.TopLevelStatements.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void InvocationResolvesToOverrideWithParams_CS_Latest() =>
            builder.AddPaths("InvocationResolvesToOverrideWithParams.Latest.cs")
                .WithOptions(LanguageOptions.CSharpLatest)
                .Verify();

#endif

    }
}
