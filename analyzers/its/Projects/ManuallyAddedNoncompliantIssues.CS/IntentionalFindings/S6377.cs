using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace IntentionalFindings;

public class S6377
{
    public void CheckSignature(XmlDocument xmlDoc, RSACryptoServiceProvider rsaCryptoServiceProvider)
    {
        var signedXml = new SignedXml(xmlDoc);
        signedXml.LoadXml((XmlElement)xmlDoc.GetElementsByTagName("Signature").Item(0));

        _ = signedXml.CheckSignature(rsaCryptoServiceProvider);
        _ = signedXml.CheckSignature(); // The key is missing.
    }
}
