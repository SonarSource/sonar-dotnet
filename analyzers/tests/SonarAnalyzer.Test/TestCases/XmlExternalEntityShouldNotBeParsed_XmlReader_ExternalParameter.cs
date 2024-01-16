using System.Xml;

namespace SonarAnalyzer.Test.TestCases
{
    public class XmlReaderExternalParameter
    {
        internal static readonly XmlReaderSettings InternalSettings = new XmlReaderSettings()
        {
            DtdProcessing = DtdProcessing.Parse, // Secondary [1,2]
            XmlResolver = new XmlUrlResolver()   // Secondary [1,2]
        };

        private static readonly XmlReader FieldReader = XmlReader.Create("uri", InternalSettings); // Noncompliant [1]

        public void XmlReader_ExternalParameter()
        {
            XmlReader.Create("uri", XmlReaderParameterProvider.Settings); // Ok - semantic model cannot resolve symbol info

            XmlReader.Create("uri", InternalSettings); // Noncompliant [2]
        }
    }
}
