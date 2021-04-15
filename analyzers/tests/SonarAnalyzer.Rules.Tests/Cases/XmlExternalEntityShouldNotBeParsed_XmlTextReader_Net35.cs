using System.IO;
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

        public void WithUsingStatement()
        {
            using (FileStream fs = new FileStream("", FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
            using (XmlTextReader r = new XmlTextReader(fs, XmlNodeType.Element, null))
            {
                r.XmlResolver = null; // no DTD resolving
                while (r.Read())
                {
                }
            }

            using (FileStream fs2 = new FileStream("", FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
            using (XmlTextReader r2 = new XmlTextReader(fs2, XmlNodeType.Element, null)) // Noncompliant
            {
                r2.XmlResolver = new XmlUrlResolver(); // Noncompliant
                while (r2.Read())
                {
                }
            }

        }

        public void WithUsingInsideUsingStatement(bool b)
        {
            using (FileStream fs = new FileStream("", FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
            {
                using (XmlTextReader r = new XmlTextReader(fs, XmlNodeType.Element, null))
                {
                    r.XmlResolver = null; // no DTD resolving
                    while (r.Read())
                    {
                    }
                    if (b)
                    {

                    }
                    else
                    {
                        string s = "";
                        if (s != null)
                        {
                            using (FileStream innerFs = new FileStream("", FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
                            using (XmlTextReader innerReader = new XmlTextReader(fs, XmlNodeType.Element, null))
                            {
                                innerReader.XmlResolver = null; // no DTD resolving
                            }
                        }
                    }
                }
            }
        }

    }
}
