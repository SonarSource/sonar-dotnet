using System.Xml;

namespace SonarAnalyzer.Test.TestCases
{
    public class XmlReaderUnsafe
    {
        public void XmlReader_NonCompliant_InlineInitialization()
        {
            //                                                                                                 Secondary@+1
            XmlReader.Create("uri", new XmlReaderSettings() { DtdProcessing = DtdProcessing.Parse}).Dispose(); // Noncompliant
        }

        public void XmlReader_NonCompliant_WithUnsafeSettings_Parse()
        {
            var settings = new XmlReaderSettings();

            settings.DtdProcessing = DtdProcessing.Parse; // Secondary

            XmlReader.Create("uri", settings).Dispose();  // Noncompliant
        }

        public void XmlReader_NonCompliant_WithUnsafeSettings_ParseAndResolver()
        {
            var settings = new XmlReaderSettings();

            settings.DtdProcessing = DtdProcessing.Parse; // Secondary
            settings.XmlResolver = new XmlUrlResolver();  // Secondary

            XmlReader.Create("uri", settings).Dispose();  // Noncompliant
        }

        public void XmlReader_NonCompliantAndCompliantMix()
        {
            var safeSettings = new XmlReaderSettings();

            safeSettings.DtdProcessing = DtdProcessing.Parse;
            safeSettings.XmlResolver = null;

            var unsafeSettings = new XmlReaderSettings();
            unsafeSettings.DtdProcessing = DtdProcessing.Parse; // Secondary
            unsafeSettings.XmlResolver = new XmlUrlResolver();  // Secondary

            XmlReader.Create("uri", safeSettings).Dispose();    // Compliant
            XmlReader.Create("uri", unsafeSettings).Dispose();  // Noncompliant
        }

        public void XmlReader_NonCompliant_ObjectInitialization()
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse      // Secondary
            };

            XmlReader.Create("uri", settings).Dispose(); // Noncompliant
        }

        public void XmlReader_NonCompliant_ResolverAsParameter(XmlUrlResolver urlResolver)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse,     // Secondary
                XmlResolver = urlResolver                // Secondary
            };

            XmlReader.Create("uri", settings).Dispose(); // Noncompliant
        }
    }

    public class XmlReaderSafe
    {
        public void XmlReader_NoSettings()
        {
            XmlReader.Create("uri").Dispose(); // Compliant - it is safe by default
        }

        public void XmlReader_DefaultSettings()
        {
            var settings = new XmlReaderSettings();

            XmlReader.Create("uri", settings).Dispose(); // Compliant - both settings and reader are safe by default
            XmlReader.Create("uri", new XmlReaderSettings()).Dispose(); // Compliant - both settings and reader are safe by default
        }

        public void XmlReader_DefaultDtdProcessing_NullResolver()
        {
            var settings = new XmlReaderSettings();

            settings.XmlResolver = null;

            XmlReader.Create("uri", settings).Dispose(); // Compliant - XmlResolver is null and DtdProcessing is not Parse by default
        }

        public void XmlReader_DefaultDtdProcessing_SecureResolver(XmlSecureResolver resolver)
        {
            var settings = new XmlReaderSettings();

            settings.XmlResolver = resolver;

            XmlReader.Create("uri", settings).Dispose(); // Compliant - XmlResolver is secure and DtdProcessing is not Parse by default
        }

        public void XmlReader_DefaultDtdProcessing_NullResolverDtoProcessingEnabled()
        {
            var settings = new XmlReaderSettings();

            settings.DtdProcessing = DtdProcessing.Parse;
            settings.XmlResolver = null;

            XmlReader.Create("uri", settings).Dispose(); // Compliant - xml resolver is null
        }

        public void XmlReader_DefaultDtdProcessing_UnsecureResolver()
        {
            var settings = new XmlReaderSettings();

            settings.XmlResolver = new XmlUrlResolver();

            XmlReader.Create("uri", settings).Dispose(); // Compliant - DtdProcessing is not Parse by default
        }

        public void XmlReader_EnabledDtdProcessing_SecureResolver(XmlSecureResolver secureResolver)
        {
            var settings = new XmlReaderSettings();

            settings.DtdProcessing = DtdProcessing.Parse;
            settings.XmlResolver = secureResolver;

            XmlReader.Create("uri", settings).Dispose(); // Compliant - XmlSecureResolver is secure
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
    }
}
