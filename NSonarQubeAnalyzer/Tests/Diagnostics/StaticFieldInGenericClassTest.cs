using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
{
    [TestClass]
    public class StaticFieldInGenericClassTest
    {
        [TestMethod]
        public void StaticFieldInGenericClass()
        {
            Verifier.Verify(@"TestCases\StaticFieldInGenericClass.cs", new StaticFieldInGenericClass());
        }
    }
}
