class PrimaryConstructor(string ctorParam = "192.168.0.1", string ctorParam2 = $"192.168.{"0"}.1") // Noncompliant
                                                                                                          // Noncompliant@-1
{
    void Method(string methodParam = "192.168.0.1", string methodParam2 = $"192.168.{"0"}.1") // Noncompliant
                                                                                              // Noncompliant@-1
    {
        var lambda = (string lambdaParam = "192.168.0.1", string lambdaParam2 = $"192.168.{"0"}.1") => lambdaParam; // Noncompliant
                                                                                                                    // Noncompliant@-1

    }
}
