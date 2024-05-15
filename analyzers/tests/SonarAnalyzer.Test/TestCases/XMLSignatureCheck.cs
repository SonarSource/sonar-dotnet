using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Linq;

public class XMLSignatures
{
    public void TestCases(RSACryptoServiceProvider rsaCryptoServiceProvider)
    {
        XmlDocument xmlDoc = new XmlDocument() { PreserveWhitespace = true };
        xmlDoc.Load("/data/login.xml");
        SignedXml signedXml = new SignedXml(xmlDoc);
        signedXml.LoadXml((XmlElement)xmlDoc.GetElementsByTagName("Signature").Item(0));

        _ = signedXml.CheckSignature(rsaCryptoServiceProvider);                     // A key is provided.
        _ = signedXml.CheckSignature("other");                                      // Custom defined extension method.
        _ = signedXml.CheckSignatureReturningKey("other");                          // Custom defined extension method.
        _ = new[] { rsaCryptoServiceProvider }.Select(signedXml.CheckSignature);    // Used as an Func<T, bool>, parameter is provided.

        _ = signedXml.CheckSignature();                                             // Noncompliant {{Change this code to only accept signatures computed from a trusted party.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^
        _ = signedXml.CheckSignatureReturningKey(out var signingKey);               // Noncompliant {{Change this code to only accept signatures computed from a trusted party.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    }
}

public static class SignedXmlExtensions
{
    public static bool CheckSignature(this SignedXml signedXml, string other) =>
        true;

    public static bool CheckSignatureReturningKey(this SignedXml signedXml, string other) =>
        true;
}
