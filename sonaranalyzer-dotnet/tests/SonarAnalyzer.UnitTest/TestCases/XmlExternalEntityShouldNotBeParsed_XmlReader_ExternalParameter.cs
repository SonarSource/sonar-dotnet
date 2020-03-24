using System.Xml;

namespace SonarAnalyzer.UnitTest.TestCases
{
    public class XmlReaderExternalParameter
    {
        public void XmlReader_ExternalParameter()
        {
            XmlReader.Create("uri", XmlReaderParameterProvider.Settings); // Ok - semantic model cannot resolve symbol info
        }
    }
}
