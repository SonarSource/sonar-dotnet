using System.Xml;

namespace NetFramework4
{
    /// <summary>
    /// In .NET Framework 4
    /// - the constructor is unsafe
    /// - the ProhibitDtd property is deprecated, being replaced by DtdProcessing which by default is Parse
    /// - to sanitize, the XmlResolver must be set to null or set DtdProcessing to Ignore/Prohibit
    /// </summary>
    class XmlTextReaderTest
    {
        private const string Url = "resources/";

        protected static void XmlTextReader_Constructor()
        {
            new XmlTextReader(Url); // Noncompliant
        }

        protected void XmlTextReader_ConstructorWithSetToNull()
        {
            var reader = new XmlTextReader("resources/");
            reader.XmlResolver = null; // compliant before 4.5.2, as well

            var another = new XmlTextReader("resources/") { XmlResolver = null };
        }

        protected static XmlTextReader XmlTextReader_SetWithParameter(XmlUrlResolver parameter)
        {
            var reader = new XmlTextReader(Url); // Noncompliant
            reader.XmlResolver = parameter; // Noncompliant - duplicate issue
            return reader;
        }

        protected static void XmlTextReader_ValidateWithDtdProcessing_Parse()
        {
            var reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Prohibit; // ok
        }

        protected static void XmlTextReader_ValidateWithDtdProcessing_Ignore()
        {
            XmlTextReader reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Ignore; // ok
        }

        protected static void XmlTextReader_ValidateAndSet()
        {
            XmlTextReader reader = new XmlTextReader(Url);
            reader.DtdProcessing = DtdProcessing.Ignore; // ok
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant - FP, but probably it's not that bad that we raise
        }

        protected void XmlTextReader_InsideIf(bool foo)
        {
            var reader = new XmlTextReader("resources/"); // Noncompliant
            if (foo)
            {
                reader.XmlResolver = null; // conditionally set; not enough
            }
        }

        protected void XmlTextReader_InsideIf2(bool foo)
        {
            var reader = new XmlTextReader("resources/");
            if (foo)
            {
                reader.XmlResolver = null;
            }
            reader.XmlResolver = null; // this is ok
        }

        protected void XmlTextReader_InsideIf3(bool foo)
        {
            var reader = new XmlTextReader("resources/"); // sanitized after the if
            if (foo)
            {
                reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
            }
            reader.XmlResolver = null;
        }
    }
}
