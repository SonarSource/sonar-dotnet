        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.$DiagnosticClassName$>();    // FIXME: Move this up

        [TestMethod]
        public void $DiagnosticClassName$_VB() =>
            builderVB.AddPaths("$DiagnosticClassName$.vb").Verify();
