using System;
using System.IO;
using System.Text;
using System.Xml;

namespace SonarAnalyzer.UnitTest.TestCases
{
    public class XmlReaderUnsafe
    {
        public void XmlReader_NonCompliant_Inline_Settings()
        {
            XmlReader.Create("uri", new XmlReaderSettings() {DtdProcessing = DtdProcessing.Parse, XmlResolver = new XmlUrlResolver()}).Dispose(); // Noncompliant
        }

        public void XmlReader_NonCompliant_ConstructorInitializer()
        {
            var settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Parse,
                XmlResolver = new XmlUrlResolver()
            };

            XmlReader.Create("uri", settings).Dispose(); // Noncompliant
        }

        public void XmlReader_NonCompliant_InvalidDtdProcessingAndXmlResolver()
        {
            var settings = new XmlReaderSettings();

            // Starting with .Net 4.5.2 this is safe because the XmlResolver property is null
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.XmlResolver = new XmlUrlResolver(); // Populating the XmlResolver makes the settings unsafe

            XmlReader.Create("uri", settings).Dispose(); // Noncompliant
        }

        public void XmlReader_NonCompliantAndCompliantMix()
        {
            var safeSettings = new XmlReaderSettings();
            var unsafeSettings = new XmlReaderSettings();

            safeSettings.DtdProcessing = DtdProcessing.Parse;
            unsafeSettings.DtdProcessing = DtdProcessing.Parse;

            safeSettings.XmlResolver = null;
            unsafeSettings.XmlResolver = new XmlUrlResolver();

            XmlReader.Create("uri", safeSettings).Dispose(); // Compliant
            XmlReader.Create("uri", unsafeSettings).Dispose(); // Noncompliant
        }
    }

    public class XmlReaderSafe
    {
        public void XmlReader_NoSettings()
        {
            XmlReader.Create("uri").Dispose(); // Compliant
        }

        public void XmlReader_DefaultSettings()
        {
            XmlReader.Create("uri", new XmlReaderSettings()).Dispose(); // Compliant
        }

        public void XmlReader_InlineSettings_NullResolver()
        {
            XmlReader.Create("uri", new XmlReaderSettings(){ DtdProcessing = DtdProcessing.Parse}).Dispose(); // Compliant
            XmlReader.Create("uri", new XmlReaderSettings(){ DtdProcessing = DtdProcessing.Parse, XmlResolver =  null}).Dispose(); // Compliant
        }

        public void XmlReader_ConstructorInitializerOnlyParsing()
        {
            var settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Parse
            };

            XmlReader.Create("uri", settings).Dispose(); // Compliant - xml resolver is null by default
        }

        public void XmlReader_ConstructorInitializerOnlyXmlResolver()
        {
            var settings = new XmlReaderSettings()
            {
                XmlResolver = new XmlUrlResolver()
            };

            XmlReader.Create("uri", settings).Dispose(); // Compliant - parsing is disabled by default
        }

        public void XmlReader_DefaultDtdProcessing_SecureResolver(XmlSecureResolver resolver)
        {
            var settings = new XmlReaderSettings();

            settings.DtdProcessing = DtdProcessing.Parse;
            settings.XmlResolver = resolver;

            XmlReader.Create("uri", settings).Dispose(); // Compliant - XmlResolver is secure and DtdProcessing is not Parse by default
        }

        public void XmlReader_DtdProcessingAsParameter(DtdProcessing processing)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = processing,
                XmlResolver = new XmlUrlResolver()
            };

            XmlReader.Create("uri", settings).Dispose(); // Compliant - we don't track DtdProcessing change between methods
        }

        public void XmlReader_SettingsAsParameter(XmlReaderSettings settings)
        {
            XmlReader.Create("uri", settings).Dispose(); // Compliant - we don't track settings change between methods
        }

        public void processXml(string xml)
        {
            var settings = new XmlReaderSettings();

            settings.ProhibitDtd = false;
            settings.XmlResolver = new XmlSafeResolver(); // In order to avoid false positives we check the exact type of XmlResolver not to be XmlUrlResolver or XmlPreloadedResolver

            XmlReader.Create(stream, settings).Dispose(); // Compliant
        }
    }

    // Example of partial sanitization recommended by microsoft
    // https://docs.microsoft.com/en-us/archive/msdn-magazine/2009/november/xml-denial-of-service-attacks-and-defenses#defending-against-external-entity-attacks
    internal class XmlSafeResolver : XmlUrlResolver
    {
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            return null;
        }
    }
}
