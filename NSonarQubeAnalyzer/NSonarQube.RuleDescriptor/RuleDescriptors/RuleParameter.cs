using System.Xml;
using System.Xml.Serialization;

namespace NSonarQube.RuleDescriptor.RuleDescriptors
{
    public class RuleParameter
    {
        [XmlElement("key")]
        public string Key { get; set; }
        [XmlIgnore]
        public string Description { get; set; }

        [XmlElement("description")]
        public XmlCDataSection DescriptionCDataSection
        {
            get
            {
                return new XmlDocument().CreateCDataSection(Description);
            }
            set { Description = value == null ? "" : value.Value; }
        }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("defaultValue")]
        public string DefaultValue { get; set; }
    }
}