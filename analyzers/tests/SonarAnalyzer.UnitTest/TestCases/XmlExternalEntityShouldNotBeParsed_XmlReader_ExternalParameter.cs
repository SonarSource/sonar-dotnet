using System.Xml;

namespace SonarAnalyzer.UnitTest.TestCases
{
    public class XmlReaderExternalParameter
    {
        internal static readonly XmlReaderSettings InternalSettings = new XmlReaderSettings()
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = new XmlUrlResolver()
        };

        private static readonly XmlReader FieldReader = XmlReader.Create("uri", InternalSettings); // Noncompliant

        public void XmlReader_ExternalParameter()
        {
            XmlReader.Create("uri", XmlReaderParameterProvider.Settings); // Ok - semantic model cannot resolve symbol info

            XmlReader.Create("uri", InternalSettings); // Noncompliant
        }
    }
}
