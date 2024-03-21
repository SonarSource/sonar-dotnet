using System.Xml;

public class RuleExceptions
{
    // Repro for https://github.com/SonarSource/sonar-dotnet/issues/8967
    static XmlNamespaceManager CreateNamespaceManager()
    {
        var namespaceManager = new XmlNamespaceManager(new NameTable());
        namespaceManager.AddNamespace("ffc", "http://www.redacted.com/namespaces/ffc"); // Noncompliant FP
        namespaceManager.AddNamespace("dex", "http://www.redacted.com/namespaces/dex"); // Noncompliant FP
        return namespaceManager;
    }
}
