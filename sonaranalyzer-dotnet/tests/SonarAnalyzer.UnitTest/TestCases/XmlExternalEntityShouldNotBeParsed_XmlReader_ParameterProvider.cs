using System.Xml;

namespace SonarAnalyzer.UnitTest.TestCases
{
    public class XmlReaderParameterProvider
    {
        internal static readonly XmlReaderSettings Settings = new XmlReaderSettings
        {
            Async = false,
        };
    }
}
