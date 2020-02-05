using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFramework45
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        protected void XmlReader_WithMemoryStream()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader.Create(new MemoryStream(), settings, "resources/"); // Noncompliant
        }

        protected void XmlDocument_1()
        {
            XmlDocument doc = new XmlDocument(); // Noncompliant
        }

        protected void XmlReader_1()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Prohibit;
            settings.XmlResolver = new XmlUrlResolver();
            XmlReader.Create(new MemoryStream(), settings, "resources/"); // ok
        }
    }
}
