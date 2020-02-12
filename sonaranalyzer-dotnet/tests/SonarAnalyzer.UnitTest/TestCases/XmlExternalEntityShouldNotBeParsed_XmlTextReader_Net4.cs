using System.Xml;

namespace NetFramework4
{
    class XmlTextReaderTest
    {
        private const string Url = "resources/";

        protected static void XmlTextReader_before_4_5_2()
        {
            new XmlTextReader(Url); // Noncompliant
        }

        protected void XmlTextReader_1()
        {
            var reader = new XmlTextReader("resources/");
            reader.XmlResolver = null; // compliant before 4.5.2, as well
        }

        protected static XmlTextReader XmlTextReader_after_4_5_2(XmlUrlResolver parameter)
        {
            var reader = new XmlTextReader(Url); // Noncompliant
            reader.XmlResolver = parameter; // Noncompliant - duplicate issue
            return reader;
        }

        protected static void XmlTextReader_before4_5_2_compliant1()
        {
            var reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Prohibit; // ok
        }

        protected static void XmlTextReader_before4_5_2_compliant2()
        {
            XmlTextReader reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Ignore; // ok
        }
    }
}
