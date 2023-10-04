class PrimaryConstructor(string ctorParamUri = "file://blah.txt", // Noncompliant
                         string ctorParam = "file://blah.txt") // Compliant
{
    void Method(string methodParamUri = "file://blah.txt", // Noncompliant
                string methodParam = "file://blah.txt") // Compliant
    {
        var lambda = (string lambdaParamUri = "file://blah.txt") => lambdaParamUri; // Noncompliant
        var lambda2 = (string lambdaParam = "file://blah.txt") => lambdaParam; // Compliant
    }
}
