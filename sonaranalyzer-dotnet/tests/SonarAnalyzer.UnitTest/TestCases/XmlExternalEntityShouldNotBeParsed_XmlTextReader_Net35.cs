using System.Xml;

namespace NetFramework35
{
    /// <summary>
    /// In .NET Framework 3.5
    /// - the constructor is unsafe by default
    /// - the ProhibitDtd is false by default and should be set to true
    /// </summary>
    class XmlTextReaderTest
    {
        private const string Url = "resources/";

        protected static void XmlTextReader_Constructor()
        {
            var reader = new XmlTextReader(Url); // Noncompliant
        }

        protected static void XmlTextReader_ConstructorWithSanitize()
        {
            var reader = new XmlTextReader(Url) { ProhibitDtd = true };
        }

        protected static void XmlTextReader_SetOnObject(XmlTextReader reader)
        {
            reader.ProhibitDtd = false; // Noncompliant
        }

        protected static void XmlTextReader_Sanitize()
        {
            var reader = new XmlTextReader(Url);
            reader.ProhibitDtd = true; // ok
        }

        protected static void XmlTextReader_ConstructorWithVulnerableSet()
        {
            var reader = new XmlTextReader(Url); // Noncompliant in 3.5
            reader.ProhibitDtd = false; // Noncompliant duplicate (it's setting what's already default)
        }

        protected static void XmlTextReader_InsideIf(bool foo)
        {
            var reader = new XmlTextReader(Url); // Noncompliant
            if (foo)
            {
                reader.ProhibitDtd = true; // this is set conditionally, so not enough
            }
        }

    }
}
