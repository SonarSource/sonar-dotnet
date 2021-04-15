using System.IO;
using System.Xml;

namespace Tests.Diagnostics
{
    /// <summary>
    /// Tests for .NET Framework 4.5.2+
    /// The constructor is safe. One must assign an unsafe resolver to become unsafe.
    /// </summary>
    class NoncompliantTests_After_Net_4_5_2
    {
        XmlTextReader reader = new XmlTextReader("resources/") { XmlResolver = new XmlUrlResolver() }; // Noncompliant

        protected void XmlTextReader_ParameterResolver(XmlUrlResolver parameter)
        {
            XmlTextReader reader = new XmlTextReader("resources/");
            // we don't really know it's null or not
            reader.XmlResolver = parameter; // Noncompliant {{Disable access to external entities in XML parsing.}}
        }

        protected void XmlTextReader_NewResolver(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }

        protected void XmlTextReader_DifferentConstructorWithNewResolver(string xmlFragment, XmlNodeType fragType, XmlParserContext context)
        {
            XmlTextReader reader = new XmlTextReader(xmlFragment, fragType, context);
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }

        protected void XmlTextReader_SetDtdProcessing(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.DtdProcessing = DtdProcessing.Parse; // Noncompliant
        }

        protected void XmlTextReader_SetDtdProcessingWithNullResolver()
        {
            XmlTextReader reader = new XmlTextReader("resources/");
            reader.DtdProcessing = DtdProcessing.Parse; // Noncompliant
            reader.XmlResolver = null;
        }

        protected void XmlTextReader_SetNonNullResolverWithDtdProcessingProhibit()
        {
            XmlTextReader reader = new XmlTextReader("resources/");
            reader.DtdProcessing = DtdProcessing.Prohibit;
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }

        protected void XmlTextReader_WithoutConstructor(XmlTextReader reader)
        {
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }
    }

    class CompliantTests_After_Net_4_5_2
    {
        // System.Xml.XmlTextReader
        protected void XmlTextReader_Constructor(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
        }

        protected void XmlTextReader_SafeResolver(string xmlFragment, XmlNodeType fragType, XmlParserContext context)
        {
            XmlTextReader reader = new XmlTextReader(xmlFragment, fragType, context);
            reader.XmlResolver = new XmlSecureResolver(null, "");
        }

        protected void XmlTextReader_SetProhibit(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.DtdProcessing = DtdProcessing.Prohibit;
        }

        protected void XmlTextReader_SetIgnore(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.DtdProcessing = DtdProcessing.Ignore;
        }

        protected void XmlTextReader_SetIgnoreAndNullResolver(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.DtdProcessing = DtdProcessing.Ignore;
            reader.XmlResolver = null;
        }
    }

    /// <summary>
    ///  These are not testing the APIs per se, but other test combinations
    /// </summary>
    class VariousUnsafeCombinations
    {
        private void LoopAndTry()
        {
            XmlTextReader reader = new XmlTextReader("resources/");
            while (true)
            {
                reader.DtdProcessing = DtdProcessing.Prohibit;
            }
            try
            {
                reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
            }
            catch
            {

            }
        }

        protected void InsideIf_MakeUnsafeInCondition_SanitizeAfter(bool foo)
        {
            XmlTextReader reader = new XmlTextReader("resources/");
            if (foo)
            {
                reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
            }
            reader.XmlResolver = null; // this is ok
        }

        protected void InsideIf_SanitizeInsideCondition(bool foo)
        {
            XmlTextReader reader = new XmlTextReader("resources/"); // FN
            if (foo)
            {
                reader.XmlResolver = null; // not enough, conditional
            }
        }

        protected void InsideIf_MakeUnsafeInCondition(bool foo)
        {
            XmlTextReader reader = new XmlTextReader("resources/");
            if (foo)
            {
                reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
            }
        }

        public void MultipleVulnerableApisInSameMethod()
        {
            XmlTextReader reader1 = new XmlTextReader("resources/");
            XmlTextReader reader2 = new XmlTextReader("resources/");
            reader1.XmlResolver = new XmlUrlResolver(); // Noncompliant
            reader2.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }

        delegate XmlUrlResolver TestDelegate();
        private void LambdaFunction()
        {
            TestDelegate resolverFactory = () => new XmlUrlResolver();
            XmlTextReader reader = new XmlTextReader("resources/");
            reader.XmlResolver = resolverFactory(); // Noncompliant
        }

        protected void PropagateValues_Parse(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            var parse = DtdProcessing.Parse;
            reader.DtdProcessing = parse; // Noncompliant
        }

        protected void PropagateValues_Null(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            XmlUrlResolver nullResolver = null;
            reader.XmlResolver = nullResolver; // Noncompliant FP
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
            using (XmlTextReader r2 = new XmlTextReader(fs2, XmlNodeType.Element, null))
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

        public void WithUsingExpression()
        {
            using FileStream fs = new FileStream("", FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            using XmlTextReader r = new XmlTextReader(fs, XmlNodeType.Element, null);
            r.XmlResolver = null; // no DTD resolving
            while (r.Read())
            {
            }
        }
    }

    public class OtherTests
    {
        protected void XmlTextReader_NoAssignmentToResolver(XmlUrlResolver parameter)
        {
            XmlTextReader reader = new XmlTextReader("resources/");
            XmlUrlResolver unusedResolver = new XmlUrlResolver(); // initialized but not assigned
        }
    }
}
