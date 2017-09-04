        [TestMethod]
        [TestCategory("Rule")]
        public void $DiagnosticClassName$_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\$DiagnosticClassName$.vb",
                new SonarAnalyzer.Rules.VisualBasic.$DiagnosticClassName$());
        }