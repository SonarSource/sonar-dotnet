class PrimaryConstructor(string ctorParam = "C:\\TMP\\f",        // Noncompliant
                         string ctorParam2 = $"C:\\{"TM"}P\\f")  // FN
{
    void Method(string methodParam = "C:\\TMP\\f",        // Noncompliant
                string methodParam2 = $"C:\\{"TM"}P\\f")  // FN
    {
        const string p = "p";
        var lambda = (string lambdaParam = "C:\\TMP\\f",                // Noncompliant
                      string lambdaParam2 = $"C:\\{"TM"}P\\f",          // FN
                      string lambdaParam3 = $"/tem{p}") => lambdaParam; // Noncompliant
    }
}
