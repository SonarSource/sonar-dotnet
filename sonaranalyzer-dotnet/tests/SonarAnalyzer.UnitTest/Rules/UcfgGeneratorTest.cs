/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

extern alias csharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using csharp::SonarAnalyzer.Rules.CSharp;
using FluentAssertions;
using Google.Protobuf;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using SonarAnalyzer.Common;
using SonarAnalyzer.Protobuf.Ucfg;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UcfgGeneratorTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Rule")]
        public void UcfgGenerator_GenerateProtobuf()
        {
            var workDir = Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName, "0");
            Directory.CreateDirectory(workDir);

            Verifier.VerifyCSharpAnalyzer(@"
class Program
{
    string Property2
    {
        set { var x = value; }
        get { return ""; }
    }
    void Method1(string s) { }
    string Method2() => null;
    string Property1 => null;
    public Program(string s) { }
    ~Program() { } // not generated, cannot accept tainted input
    public static string operator +(Program left, string right)
    {
        return null;
    }
    public event System.EventHandler Event
    {
        add { } // not generated, cannot accept tainted input
        remove { } // not generated, cannot accept tainted input
    }
}
",
                new UcfgGenerator(new TestAnalyzerConfiguration(workDir, "S3649")));

            var ucfgPath = Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName, "ucfg_cs");
            Directory.Exists(ucfgPath).Should().BeTrue();
            Directory.GetFiles(ucfgPath).Select(Path.GetFileName).Should().BeEquivalentTo(
                new[]
                {
                    "ucfg_0_1.pb",
                    "ucfg_0_2.pb",
                    "ucfg_0_3.pb",
                    "ucfg_0_4.pb",
                    "ucfg_0_5.pb",
                    "ucfg_0_6.pb",
                    "ucfg_0_7.pb",
                });
            Directory.GetFiles(ucfgPath).Select(GetProtobufMethodId).Should().BeEquivalentTo(
                new[]
                {
                    "Program.Property1.get",
                    "Program.Property2.get",
                    "Program.Property2.set",
                    "Program.Program(string)",
                    "Program.operator +(Program, string)",
                    "Program.Method1(string)",
                    "Program.Method2()",
                });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UcfgGenerator_Disabled_No_Rules()
        {
            var workDir = Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName, "0");
            Directory.CreateDirectory(workDir);

            UcfgGenerator_Disabled(workDir);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UcfgGenerator_Enabled_S2091()
        {
            UcfgGenerator_Enabled(enabledRules: "S2091");
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UcfgGenerator_Enabled_S3649()
        {
            UcfgGenerator_Enabled(enabledRules: "S3649");
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UcfgGenerator_Enabled_S2631()
        {
            UcfgGenerator_Enabled(enabledRules: "S2631");
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UcfgGenerator_Enabled_S2083()
        {
            UcfgGenerator_Enabled(enabledRules: "S2083");
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UcfgGenerator_Enabled_S2078()
        {
            UcfgGenerator_Enabled(enabledRules: "S2078");
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UcfgGenerator_Enabled_S2076()
        {
            UcfgGenerator_Enabled(enabledRules: "S2076");
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UcfgGenerator_Disabled_OtherRule()
        {
            var workDir = Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName, "0");
            Directory.CreateDirectory(workDir);

            UcfgGenerator_Disabled(workDir, "Sxxxx");
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UcfgGenerator_Disabled_NoOutputFolder()
        {
            UcfgGenerator_Disabled(null, "S3649");
        }


        [TestMethod]
        public void UcfgGenerator_Enabled_Generate_NoErrorForUnsupportedLanguageConstructs()
        {
            var workDir = Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName, "0");
            Directory.CreateDirectory(workDir);

            using (new AssertIgnoreScope())
            {

                Verifier.VerifyCSharpAnalyzer(@"
public class Class1
{
    public void Foo()
    {
        int j = inner(123);

        // The C# CFG will throw NotImplementedException at the local function
        int inner(int i)
        {
            return i;
        }
    }
}",
                    new UcfgGenerator(new TestAnalyzerConfiguration(workDir, "S3649")));
            }

            var ucfgPath = Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName, "ucfg_cs");
            Directory.Exists(ucfgPath).Should().BeTrue();
            Directory.GetFiles(ucfgPath).Select(Path.GetFileName).Should().BeEmpty();
        }

        private void UcfgGenerator_Disabled(string workDir, params string[] enabledRules)
        {
            var ucfgGeneratorMock = new Mock<UcfgGenerator>(new TestAnalyzerConfiguration(workDir, enabledRules))
            {
                CallBase = true
            };
            ucfgGeneratorMock.Protected()
                .Setup("WriteProtobuf", ItExpr.IsAny<UCFG>(), ItExpr.IsAny<string>());

            Verifier.VerifyCSharpAnalyzer(@"class Program { void Method1(string s) { } }", ucfgGeneratorMock.Object);

            ucfgGeneratorMock.Protected()
                .Verify("WriteProtobuf", Times.Never(), ItExpr.IsAny<UCFG>(), ItExpr.IsAny<string>());
        }

        [TestMethod]
        public void UcfgGenerator_Generate_Dot_Enabled()
        {
            var workDir = Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName, "0");
            Directory.CreateDirectory(workDir);

            Environment.SetEnvironmentVariable("SONARANALYZER_GENERATE_DOT", "TRUE");

            Verifier.VerifyCSharpAnalyzer(@"class Program { void Method1(string s) { } }",
                new UcfgGenerator(new TestAnalyzerConfiguration(workDir, "S3649")));

            Environment.SetEnvironmentVariable("SONARANALYZER_GENERATE_DOT", null);

            var ucfgPath = Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName, "ucfg_cs");
            Directory.Exists(ucfgPath).Should().BeTrue();
            Directory.GetFiles(ucfgPath).Select(Path.GetFileName).Should().BeEquivalentTo(
                new[]
                {
                    "ucfg_0_1.pb",
                    "ucfg_0_1.dot",
                    "cfg_0_1.dot",
                });
        }

        [TestMethod]
        public void UcfgGenerator_Generate_Dot_Disabled()
        {
            var workDir = Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName, "0");
            Directory.CreateDirectory(workDir);

            var ucfgGeneratorMock = new Mock<UcfgGenerator>(new TestAnalyzerConfiguration(workDir, "S3649"))
            {
                CallBase = true
            };
            ucfgGeneratorMock.Protected()
                .Setup("WriteDot", ItExpr.IsAny<string>(), ItExpr.IsAny<Action<StreamWriter>>());

            // no env var should be declared at this time
            Environment.GetEnvironmentVariable("SONARANALYZER_GENERATE_DOT").Should().BeNull();

            Verifier.VerifyCSharpAnalyzer(@"class Program { void Method1(string s) { } }", ucfgGeneratorMock.Object);

            Environment.SetEnvironmentVariable("SONARANALYZER_GENERATE_DOT", "FALSE");

            Verifier.VerifyCSharpAnalyzer(@"class Program { void Method1(string s) { } }", ucfgGeneratorMock.Object);

            Environment.SetEnvironmentVariable("SONARANALYZER_GENERATE_DOT", null);

            ucfgGeneratorMock.Protected()
                .Verify("WriteDot", Times.Never(), ItExpr.IsAny<string>(), ItExpr.IsAny<Action<StreamWriter>>());
        }

        private void UcfgGenerator_Enabled(params string[] enabledRules)
        {
            var workDir = Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName, "0");
            Directory.CreateDirectory(workDir);

            Verifier.VerifyCSharpAnalyzer(@"class Program { void Method1(string s) { } }",
                new UcfgGenerator(new TestAnalyzerConfiguration(workDir, enabledRules)));

            var ucfgPathExists = Directory.Exists(
                Path.Combine(TestContext.TestRunResultsDirectory, TestContext.TestName, "ucfg_cs"));

            ucfgPathExists.Should().BeTrue();
        }

        [TestMethod]
        public void Ucfg_Block_Without_Terminator_IsInvalid()
        {
            var ucfg = new UCFG
            {
                Entries = { "0" },
                BasicBlocks =
                {
                    new BasicBlock { Id = "0" }, // no Jump or Ret
                },
            };

            UcfgGenerator.IsValid(ucfg).Should().BeFalse();
        }

        [TestMethod]
        public void Ucfg_Block_With_Jump_IsValid()
        {
            var ucfg = new UCFG
            {
                Entries = { "0" },
                BasicBlocks =
                {
                    new BasicBlock
                    {
                        Id = "0",
                        Jump = new Jump { Destinations = { "0" } },
                    },
                },
            };

            UcfgGenerator.IsValid(ucfg).Should().BeTrue();
        }

        [TestMethod]
        public void Ucfg_Block_With_Ret_IsValid()
        {
            var ucfg = new UCFG
            {
                Entries = { "0" },
                BasicBlocks =
                {
                    new BasicBlock
                    {
                        Id = "0",
                        Ret = new Return
                        {
                            ReturnedExpression = new Expression { Var = new Variable { Name = "a" } }
                        },
                    },
                },
            };

            UcfgGenerator.IsValid(ucfg).Should().BeTrue();
        }

        [TestMethod]
        public void Ucfg_Missing_Entries_IsInvalid()
        {
            var ucfg = new UCFG
            {
                Entries = { "1" }, // There is no such block
                BasicBlocks =
                {
                    new BasicBlock
                    {
                        Id = "0",
                        Jump = new Jump { Destinations = { "0" } },
                    },
                },
            };

            UcfgGenerator.IsValid(ucfg).Should().BeFalse();
        }

        [TestMethod]
        public void Ucfg_Block_With_Missing_Jump_Destinations_IsInvalid()
        {
            var ucfg = new UCFG
            {
                Entries = { "0" },
                BasicBlocks =
                {
                    new BasicBlock
                    {
                        Id = "0",
                        Jump = new Jump { Destinations = { "1" } }, // There is no such block
                    },
                },
            };

            UcfgGenerator.IsValid(ucfg).Should().BeFalse();
        }

        private static string GetProtobufMethodId(string protobufPath)
        {
            File.Exists(protobufPath).Should().BeTrue($"File {protobufPath} must exist.");
            using (var stream = File.OpenRead(protobufPath))
            {
                var ucfg = new UCFG();
                ucfg.MergeFrom(stream);
                return ucfg.MethodId;
            }
        }

        private class TestAnalyzerConfiguration : IAnalyzerConfiguration
        {
            public TestAnalyzerConfiguration(string projectOutputPath, params string[] enabledRules)
            {
                ProjectOutputPath = projectOutputPath;
                EnabledRules = enabledRules;
            }

            public IReadOnlyCollection<string> EnabledRules { get; }

            public string ProjectOutputPath { get; }

            public void Read(AnalyzerOptions options)
            {
                // do nothing
            }
        }
    }
}
