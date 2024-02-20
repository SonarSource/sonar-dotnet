using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

[assembly: XmlnsPrefix("http://schemas.catelproject.com", "catel")]
[assembly: XmlnsDefinition("http://schemas.catelproject.com", "Catel.MVVM")]
[assembly: XmlnsCompatibleWith("http://www.adatum.com/2003/controls", "http://www.adatum.com/2005/controls")]

namespace Tests.Diagnostics
{
    class ClearTextProtocols
    {
        private const string a = "http://foo.com"; // Noncompliant {{Using http protocol is insecure. Use https instead.}}
        private const string b = "https://foo.com";
        private const string c = "http://localhost";
        private const string d = "http://localhost/path/query?query=xxx";
        private const string e = "http://foo.com/localhost/?query=xxx"; // Noncompliant
        private const string f = "http://foo.com/path/?query=localhost"; // Noncompliant
        private const string g = "http://127.0.0.1";
        private const string h = "http://::1";

        private const string i = @"telnet://anonymous@foo.com"; // Noncompliant {{Using telnet protocol is insecure. Use ssh instead.}}
        private const string j = @"ssh://anonymous@foo.com";
        private const string k = @"telnet://anonymous@localhost";
        private const string l = "telnet://anonymous@127.0.0.1";
        private const string m = "telnet://anonymous@::1";

        private readonly string n = $"ftp://anonymous@foo.com"; // Noncompliant {{Using ftp protocol is insecure. Use sftp, scp or ftps instead.}}
        private readonly string o = $"sftp://anonymous@foo.com";
        private readonly string p = $"ftp://anonymous@localhost";
        private readonly string q = $"ftp://anonymous@127.0.0.1";
        private readonly string r = $"ftp://anonymous@::1";

        public void Method(string part, string user, string domain, string ftp)
        {
            var a = "http://foo.com"; // Noncompliant
            var b = "https://foo.com";
            var c = "http://localservername"; // Noncompliant, it's not a "localhost"
            var d = "http://localhosting";
            var e = "See http://www.foo.com for more information";
            var httpProtocolScheme = "http://"; // It's compliant when standalone

            var f = $"ftp://anonymous@foo.com"; // Noncompliant
            var g = $"ftp://{part}@foo.com"; // Noncompliant
            var h = "ssh://anonymous@foo.com";
            var i = "sftp://anonymous@foo.com";
            var ftpProtocolScheme = "ftp://"; // It's compliant when standalone
            var j = "ftp://" + user + "@foo.com"; // Compliant - FN (protocol is compliant when standallone, check previous case)
            var k = "ftp://anonymous@" + domain;// Noncompliant
            var l = $"ftp://anonymous@{domain}"; // Noncompliant
            var m = $"{ftp}://anonymous@foo.com"; // Compliant

            var n = @"telnet://anonymous@foo.com"; // Noncompliant
            var telnetProtocolScheme = "telnet://"; // It's compliant when standalone

            var uri = new Uri("http://foo.com"); // Noncompliant
            var uriSafe = new Uri("https://foo.com");

            using var wc = new WebClient();
            wc.DownloadData("http://foo.com"); // Noncompliant
            wc.DownloadData("https://foo.com");
        }

        public void Smtp()
        {
            using var notSet = new SmtpClient("host", 25); // Noncompliant {{EnableSsl should be set to true.}}
            using var constructorFalse = new SmtpClient("host", 25) { EnableSsl = false }; // Noncompliant

            using var constructor42 = new SmtpClient("host", 25) { EnableSsl = 42 }; // Error [CS0029] Cannot implicitly convert type 'int' to 'bool'
            // Noncompliant@-1 FP

            using var localhosting = new SmtpClient("localhosting", 25); // Noncompliant
            using var localhost = new SmtpClient("localhost", 25); // Compliant due to well known value
            using var loopback = new SmtpClient("127.0.0.1", 25); // Compliant due to well known value
            using var constructorTrue = new SmtpClient("host", 25) { EnableSsl = true };

            using var propertyTrue = new SmtpClient("host", 25); // Compliant, property is set below
            propertyTrue.EnableSsl = true;

            using var propertyFalse = new SmtpClient("host", 25); // Noncompliant {{EnableSsl should be set to true.}}
            propertyFalse.EnableSsl = false;

            using var setReset = new SmtpClient("host", 25) { EnableSsl = true }; // FN - it is later set to false
            setReset.EnableSsl = false;
        }

        public void Ftp()
        {
            bool variable;
            var notSet = (FtpWebRequest)WebRequest.Create(UntrackedSource()); // Compliant, FN

            var setToFalse = (FtpWebRequest)WebRequest.Create(UntrackedSource());
            setToFalse.EnableSsl = false;       // Noncompliant {{EnableSsl should be set to true.}}
            variable = false;
            setToFalse.EnableSsl = variable;    // Noncompliant

            var setToTrue = (FtpWebRequest)WebRequest.Create(UntrackedSource());
            setToTrue.EnableSsl = true;
            variable = true;
            setToFalse.EnableSsl = variable;
        }

        private string UntrackedSource() => string.Empty;

        public void TelnetExample() // This line is compliant, even when it contains "Telnet" keyword
        {
            // Method names
            var a1 = Telnet(); // Noncompliant
            var a2 = TelnetClient(); // Noncompliant
            var a3 = TcpTelnet40(); // Noncompliant
            var a4 = TcpTelnetClient(); // Noncompliant
            var a5 = Tcp_Telnet_Client(); // Noncompliant
            var a6 = TelnetLocalMethod(); // Noncompliant

            // Namespaces
            var b1 = new Company.Telnet.Client(); // Noncompliant
            var b2 = new Telnet.Stream(); // Noncompliant
            var b3 = new Company.TcpTelnetClient.Implementation(); // Noncompliant
            var b4 = Company.TcpTelnetClient.Implementation.Connect(); // Noncompliant

            // Types
            var c1 = new ClassNames.Telnet(); // Noncompliant
            var c2 = new ClassNames.TelnetClient(); // Noncompliant
            var c3 = new ClassNames.TcpTelnetClient(); // Noncompliant

            // Class names
            var d1 = TelNet();
            var d2 = Telnetwork();
            var d3 = HotelNetwork();

            // Namespaces
            var e1 = new Company.TelNet.Client();
            var e2 = new TelNet.Stream();
            var e3 = new Company.TcpTelnetwork.Implementation();
            var e4 = Company.TcpTelnetwork.Implementation.Connect();

            // Types
            var f1 = new ClassNames.TelNet();
            var f2 = new ClassNames.TelNet();
            var f3 = new ClassNames.HotelNetwork();

            string TelnetLocalMethod() => string.Empty;
        }

        private static Stream Telnet() => null;
        private Stream TelnetClient() => null;
        private Stream TcpTelnet40() => null; // With protocol version
        private Stream TcpTelnetClient() => null;
        private Stream Tcp_Telnet_Client() => null;
        private static Stream TelNet() => null;
        private Stream Telnetwork() => null;
        private Stream HotelNetwork() => null;

        private readonly List<string> links = new List<string>
        {
            "http://foo.com", // Noncompliant
            "Http://foo.com", // Noncompliant
            "HTTP://foo.com", // Noncompliant
            "https://foo.com",
            "telnet://anonymous@foo.com", // Noncompliant
            "TELNET://anonymous@foo.com", // Noncompliant
            "ftp://anonymous@foo.com", // Noncompliant
            "FTP://anonymous@foo.com" // Noncompliant
        };

        private readonly List<string> commonlyUsedXmlDomains = new List<string>
        {
            "http://www.w3.org",
            "http://xml.apache.org",
            "http://schemas.xmlsoap.org",
            "http://schemas.openxmlformats.org",
            "http://rdfs.org",
            "http://purl.org",
            "http://xmlns.com",
            "http://schemas.google.com",
            "http://schemas.microsoft.com",
            "http://a9.com",
            "http://ns.adobe.com",
            "http://ltsc.ieee.org",
            "http://docbook.org",
            "http://graphml.graphdrawing.org",
            "http://json-schema.org",
            "http://www.sitemaps.org/schemas/sitemap/0.9",
            "http://exslt.org/common",
            "http://collations.microsoft.com",
            "http://schemas.microsoft.com/framework/2003/xml/xslt/internal",
            "http://docs.oasis-open.org/wss/2004/01/",
            "http://ws-i.org/",
            "http://schemas.android.com/apk/res/android",
            "http://maven.apache.org/POM/4.0.0",
            "http://www.omg.org/spec/UML/20131001",
            "http://www.opengis.net/kml/2.2",
            "http://www.itunes.com",
            "http://www.itunes.com/dtds/podcast-1.0.dtd",
            "http://email:password@subdomain.www.w3.org",  // Noncompliant
            "http://domain.com/www.w3.org",                // Noncompliant
            "http://domain.com/path?domain=www.w3.org",    // Noncompliant
            "http://domain.com?q=www.w3.org",              // Noncompliant
            "http://domain.com#www.w3.org",                // Noncompliant
            "http://subdomain.www.w3.org",                 // Noncompliant
            "http://subdomain.xml.apache.org",             // Noncompliant
            "http://subdomain#www.itunes.com",             // Noncompliant
            "http://subdomain.schemas.xmlsoap.org",        // Noncompliant
            "http://subdomain.schemas.openxmlformats.org", // Noncompliant
            "http://subdomain.rdfs.org",                   // Noncompliant
            "http://subdomain.purl.org",                   // Noncompliant
            "http://subdomain.xmlns.com",                  // Noncompliant
            "http://subdomain.schemas.google.com",         // Noncompliant
            "http://subdomain.schemas.microsoft.com",      // Noncompliant
            "http://subdomain.a9.com",                     // Noncompliant
            "http://subdomain.ns.adobe.com",               // Noncompliant
            "http://subdomain.ltsc.ieee.org",              // Noncompliant
            "http://subdomain.docbook.org",                // Noncompliant
            "http://subdomain.graphml.graphdrawing.org",   // Noncompliant
            "http://subdomain.json-schema.org",            // Noncompliant
        };

        private readonly List<string> commonlyUsedExampleDomains = new List<string>
        {
            "http://example.com",
            "http://example.org",
            "http://test.com",
            "http://subdomain.example.com",
            "http://subdomain.example.org",
            "http://subdomain.test.com",
            "http://email:password@subdomain.example.com",
            "http://domain.com/example.com",               // Noncompliant
            "http://domain.com/path?domain=example.com",   // Noncompliant
            "http://domain.com?q=example.com",             // Noncompliant
            "http://domain.com#example.com",               // Noncompliant
        };
    }

    [XmlRoot(ElementName = "SonarProjectConfig", Namespace = "http://www.sonarsource.com/msbuild/analyzer/2021/1")]
    public class NamespaceLikeAssignment
    {
        private string NamespaceLikeField = "http://www.sonarsource.com/msbuild/analyzer/2021/1";
        private string NamespaceLikeProperty { get; set; }

        private string FooNamespace = "http://www.sonarsource.com/msbuild/analyzer/2021/1";
        private string FOO_NAMESPACE { get; set; }

        public void Foo(string namespaceLikeArgument)
        {
            var namespaceLikeVar = "http://www.sonarsource.com/msbuild/analyzer/2021/1";
            namespaceLikeArgument = "http://www.sonarsource.com/msbuild/analyzer/2021/1";
            NamespaceLikeField = "http://www.sonarsource.com/msbuild/analyzer/2021/1";
            NamespaceLikeProperty = "http://www.sonarsource.com/msbuild/analyzer/2021/1";
            FooNamespace = "http://www.sonarsource.com/msbuild/analyzer/2021/1";
            FOO_NAMESPACE = "http://www.sonarsource.com/msbuild/analyzer/2021/1";
        }

        public void Bar(string NamespaceLikeArgument = "http://www.sonarsource.com/msbuild/analyzer/2021/123") { }
    }

    [MyAttribute("http://www.sonarsource.com/msbuild/analyzer/2021/123")]  // Noncompliant
    [XmlRoot(ElementName = "SonarProjectConfig", Namespace = "ftp://a:a@foo.com/")]  // Noncompliant
    public class NamespaceLikeAssignment2
    {
        private string NamespaceLikeField = "ftp://a:a@foo.com/";  // Noncompliant
        private string NamespaceLikeProperty { get; set; }


        private string NamefooLikeField = "http://www.sonarsource.com/msbuild/analyzer/2021/1";  // Noncompliant
        private string NamefooLikeProperty { get; set; }

        public void Foo(string namefooLikeArgument, string namespaceLikeArgument = "ftp://a:a@foo.com/")  // Noncompliant
        {
            var namespaceLikeVar = "ftp://a:a@foo.com/";  // Noncompliant
            namespaceLikeArgument = "ftp://a:a@foo.com/";  // Noncompliant
            NamespaceLikeField = "ftp://a:a@foo.com/";  // Noncompliant
            NamespaceLikeProperty = "ftp://a:a@foo.com/";  // Noncompliant

            string[] namespaceArray = new string[1];
            namespaceArray[0] = "http://www.sonarsource.com/msbuild/analyzer/2021/1";  // Noncompliant

            var namefooLikeVar = "http://www.sonarsource.com/msbuild/analyzer/2021/1";  // Noncompliant
            namefooLikeArgument = "http://www.sonarsource.com/msbuild/analyzer/2021/1";  // Noncompliant
            NamefooLikeField = "http://www.sonarsource.com/msbuild/analyzer/2021/1";  // Noncompliant
            NamefooLikeProperty = "http://www.sonarsource.com/msbuild/analyzer/2021/1";  // Noncompliant
        }
    }

    public class MyAttribute : Attribute
    {
        public MyAttribute(string str) {
        }
    }

    [XmlRoot(Namespace=XML_NAMESPACE)]
    public class NamespaceInConstant {
        public const string XML_NAMESPACE = "http://x";
        public const string XML_FOOSPACE = "http://x";  // Noncompliant
    }

    public static class Constants {
        public const String NAMESPACE1 = "http://x";
        public const String FOOSPACE1 = "http://x";  // Noncompliant
    }

    [XmlType(Namespace = "http://www.cpandl.com")]
    public class Serialize
    {
        [XmlElement(Namespace = "http://www.cpandl.com")]
        public string Title;

        [XmlAttribute(Namespace = "http://www.cpandl.com")]
        public string Currency;
    }

    public class XmlSerializerTests
    {
        public void XmlSerializerTypes()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("books", "http://www.cpandl.com");
            ns.Add("money", "http://www.cohowinery.com");
        }

        public class TestXmlSerializationWriter : XmlSerializationWriter
        {
            public void M()
            {
                this.WriteElementStringRaw("name", "http://www.cohowinery.com", "value");
            }

            protected override void InitCallbacks() { }
        }
    }

    public class xmlTests
    {
        public void XmlTypes()
        {
            var qn = new XmlQualifiedName("books", "http://www.cpandl.com");
            var nsManager = new XmlNamespaceManager(null);
            nsManager.AddNamespace("prefix", "http://www.cpandl.com");
            nsManager.RemoveNamespace("prefix", "http://www.cpandl.com");
            nsManager.LookupPrefix("http://www.cpandl.com");
            var doc = new XmlDocument();
            doc.CreateAttribute("prefix", "localName", "http://www.cpandl.com");
            doc.CreateElement("prefix", "localName", "http://www.cpandl.com");
            doc.CreateNode("nodeTypeString", "localName", "http://www.cpandl.com");
            using (XmlWriter writer = XmlWriter.Create("output.xml"))
            {
                writer.WriteStartElement("localName", "http://www.cpandl.com");
                writer.WriteElementString("localName", "http://www.cpandl.com", "value");
                writer.WriteElementString("localName", "http://www.cpandl.com"); // Noncompliant. uri is passed to the "value" parameter
            }
        }
    }

    public class XamlTests
    {
        public void XmlnsDictionaryTest()
        {
            var dict = new XmlnsDictionary();
            dict.Add("prefix", "http://www.cpandl.com");
            dict.LookupPrefix("http://www.cpandl.com");
        }
    }

    public class XmlLinqTests
    {
        public void XNamespaceTest()
        {
            XNamespace.Get("http://www.cpandl.com");
            XNamespace ns = "http://www.cpandl.com"; // Noncompliant FP. Implicit conversion from string to XNamespace is not supported
        }
    }

    public class ConstructorInitializerTest
    {
        public class Base
        {
            public Base(): this("http://www.cpandl.com") // Noncompliant
            { }
            public Base(string name) {  }
        }

        public class Derived: Base
        {
            public Derived(): base("http://www.cpandl.com") // Noncompliant
            { }
        }
    }
}

namespace Company.Telnet
{
    public class Client { }
}

namespace Telnet
{
    public class Stream { }
}

namespace Company.TcpTelnetClient
{
    public class Implementation
    {
        public static Stream Connect() => null;
    }
}

namespace ClassNames
{
    public class Telnet { }
    public class TelnetClient { }
    public class TcpTelnetClient { }
}

namespace Company.TelNet
{
    public class Client { }
}

namespace TelNet
{
    public class Stream { }
}

namespace Company.TcpTelnetwork
{
    public class Implementation
    {
        public static Stream Connect() => null;
    }
}

namespace ClassNames
{
    public class TelNet { }
    public class Telnetwork { }
    public class HotelNetwork { }
}

// Fakes. This can be removed once the WPF framework and System.Xaml can be referenced
namespace System.Windows.Markup
{
    public sealed class XmlnsPrefixAttribute : Attribute
    {
        public XmlnsPrefixAttribute(string ns1, string ns2) { }
    }
    public sealed class XmlnsDefinitionAttribute : Attribute
    {
        public XmlnsDefinitionAttribute(string ns1, string ns2) { }
    }
    public sealed class XmlnsCompatibleWithAttribute : Attribute
    {
        public XmlnsCompatibleWithAttribute(string ns1, string ns2) { }
    }
    public class XmlnsDictionary
    {
        public void Add(string prefix, string xmlNamespace) => throw new NotImplementedException();
        public string LookupPrefix(string xmlNamespace) => throw new NotImplementedException();
    }
}
