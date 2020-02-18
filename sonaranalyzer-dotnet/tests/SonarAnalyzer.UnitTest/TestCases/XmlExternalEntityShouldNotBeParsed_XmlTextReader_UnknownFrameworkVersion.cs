using System.Xml;

namespace NetFrameworkUnknown
{
    /// <summary>
    /// Unknown framework - we assume it is like in .NET Framework 4.5.2 : safe constructor
    /// </summary>
    class XmlTextReaderTest
    {
        private const string Url = "resources/";

        protected static void XmlTextReader_Constructor()
        {
            new XmlTextReader(Url);
        }

        protected void XmlTextReader_SanitizeWithNull()
        {
            var reader = new XmlTextReader("resources/");
            reader.XmlResolver = null;
        }

        protected void XmlTextReader_SetUnsafeResolver(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }

        protected static XmlTextReader XmlTextReader_SetUnsafeResolverFromParameter(XmlUrlResolver parameter)
        {
            var reader = new XmlTextReader(Url);
            reader.XmlResolver = parameter; // Noncompliant
            return reader;
        }

        protected static void XmlTextReader_SanitizeWithProperty()
        {
            var reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Prohibit;
        }

        protected static void XmlTextReader_SanitizeWithProperty2()
        {
            XmlTextReader reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}
