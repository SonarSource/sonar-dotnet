using System.Xml;

namespace Tests.Diagnostics
{
    class NoncompliantTests_After_Net_4_5_2
    {
        // System.Xml.XmlTextReader
        protected void XmlTextReader_1(XmlUrlResolver parameter)
        {
            XmlTextReader reader = new XmlTextReader("resources/"); // this is safe in .NET 4.5.2+
            // this is not used, but the rule is not advanced enough to figure it out
            XmlUrlResolver res = new XmlUrlResolver();
            // we don't really know it's null or not
            reader.XmlResolver = parameter; // Noncompliant
        }

        protected void XmlTextReader_2(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }

        protected void XmlTextReader_3(string xmlFragment, XmlNodeType fragType, XmlParserContext context)
        {
            XmlTextReader reader = new XmlTextReader(xmlFragment, fragType, context);
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }

        protected void XmlTextReader_4(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.DtdProcessing = DtdProcessing.Parse; // Noncompliant
        }

        protected void XmlTextReader_5()
        {
            XmlTextReader reader = new XmlTextReader("resources/");
            reader.DtdProcessing = DtdProcessing.Parse; // Noncompliant
            reader.XmlResolver = null;
        }

        protected void XmlTextReader_6()
        {
            XmlTextReader reader = new XmlTextReader("resources/");
            reader.DtdProcessing = DtdProcessing.Prohibit;
            reader.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }
    }


    class CompliantTests_After_Net_4_5_2
    {
        // System.Xml.XmlTextReader
        protected void XmlTextReader_1(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
        }

        protected void XmlTextReader_2(string xmlFragment, XmlNodeType fragType, XmlParserContext context)
        {
            XmlTextReader reader = new XmlTextReader(xmlFragment, fragType, context);
            reader.XmlResolver = new XmlSecureResolver(null, "");
        }

        protected void XmlTextReader_3(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.DtdProcessing = DtdProcessing.Prohibit;
        }

        protected void XmlTextReader_4(XmlNameTable table)
        {
            XmlTextReader reader = new XmlTextReader("resources/", table);
            reader.DtdProcessing = DtdProcessing.Ignore;
        }

        protected void XmlTextReader_5(XmlNameTable table)
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

        public void MultipleVulnerableApisInSameMethod()
        {
            XmlTextReader reader1 = new XmlTextReader("resources/");
            XmlTextReader reader2 = new XmlTextReader("resources/");
            reader1.XmlResolver = new XmlUrlResolver(); // Noncompliant
            reader2.XmlResolver = new XmlUrlResolver(); // Noncompliant
        }
    }
}
