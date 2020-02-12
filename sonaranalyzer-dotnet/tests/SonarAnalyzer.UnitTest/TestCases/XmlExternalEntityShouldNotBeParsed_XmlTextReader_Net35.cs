using System.Xml;

namespace NetFramework35
{
    class XmlTextReaderTest
    {
        private const string Url = "resources/";

        protected static void XmlTextReader_1()
        {
            var reader = new XmlTextReader(Url); // Noncompliant
        }

        protected static void XmlTextReader_2()
        {
            var reader = new XmlTextReader(Url);
            reader.ProhibitDtd = true; // ok
        }

        protected static void XmlTextReader_3()
        {
            var reader = new XmlTextReader(Url); // Noncompliant in 3.5
            reader.ProhibitDtd = false; // Noncompliant duplicate (it's setting what's already default)
        }
    }
}
