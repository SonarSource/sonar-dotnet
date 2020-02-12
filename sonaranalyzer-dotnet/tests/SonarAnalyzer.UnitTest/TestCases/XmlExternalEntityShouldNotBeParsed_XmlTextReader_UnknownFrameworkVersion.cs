using System.Xml;

namespace NetFrameworkUnknown
{
    class XmlTextReaderTest
    {
        private const string Url = "resources/";

        protected static void XmlTextReader_before_4_5_2()
        {
            new XmlTextReader(Url);
        }

        protected void XmlTextReader_1()
        {
            var reader = new XmlTextReader("resources/");
            reader.XmlResolver = null;
        }

        protected void XmlTextReader_2(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }

        protected static XmlTextReader XmlTextReader_3(XmlUrlResolver parameter)
        {
            var reader = new XmlTextReader(Url);
            reader.XmlResolver = parameter; // Noncompliant
            return reader;
        }

        protected static void XmlTextReader_4()
        {
            var reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Prohibit;
        }

        protected static void XmlTextReader_5()
        {
            XmlTextReader reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}
