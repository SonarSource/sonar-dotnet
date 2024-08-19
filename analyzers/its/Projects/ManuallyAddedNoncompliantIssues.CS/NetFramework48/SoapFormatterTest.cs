/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System.IO;
using System.Runtime.Serialization.Formatters.Soap;

namespace NetFramework48
{
    public class SoapFormatterTest
    {
        internal void SoapFormatterDeserialize(Stream stream)
        {
            new SoapFormatter().Deserialize(stream); // Noncompliant (S5773) {{Restrict types of objects allowed to be deserialized.}}

            new SoapFormatter {Binder = new SafeBinder()}.Deserialize(stream);
        }
    }
}
