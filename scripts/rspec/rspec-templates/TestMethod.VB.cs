        // FIXME: Move this up
        private readonly VerifierBuilder builderVB = new VerifierBuilder<$DiagnosticClassName$>();

        [TestMethod]
        public void $DiagnosticClassName$_VB() =>
            builderVB.AddPaths("$DiagnosticClassName$.vb").Verify();
